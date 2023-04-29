// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.AccountDataAccess.Patient
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient.Api;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Validations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Handle Patient data.
    /// </summary>
    internal class PatientRepository : IPatientRepository
    {
        /// <summary>
        /// The generic cache domain to store patients against.
        /// </summary>
        private const string PatientCacheDomain = "PatientV2";

        private readonly ICacheProvider cacheProvider;
        private readonly int cacheTtl;

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientRepository> logger;

        private readonly IClientRegistriesDelegate clientRegistriesDelegate;
        private readonly IPatientIdentityApi patientIdentityApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientRepository"/> class.
        /// </summary>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientIdentityApi">The patient identity api to use.</param>
        public PatientRepository(
            IClientRegistriesDelegate clientRegistriesDelegate,
            ICacheProvider cacheProvider,
            IConfiguration configuration,
            ILogger<PatientRepository> logger,
            IPatientIdentityApi patientIdentityApi)
        {
            this.clientRegistriesDelegate = clientRegistriesDelegate;
            this.cacheProvider = cacheProvider;
            this.cacheTtl = configuration.GetSection("PatientService").GetValue("CacheTTL", 0);
            this.logger = logger;
            this.patientIdentityApi = patientIdentityApi;
        }

        private static ActivitySource Source { get; } = new(nameof(PatientRepository));

        /// <inheritdoc/>
        public async Task<PatientQueryResult> Query(PatientQuery query, CancellationToken ct)
        {
            return query switch
            {
                PatientDetailsQuery q => await this.Handle(q, ct).ConfigureAwait(true),

                _ => throw new NotImplementedException($"{query.GetType().FullName}"),
            };
        }

        private static bool ShouldCheckCache(PatientDetailSource source)
        {
            return source switch
            {
                PatientDetailSource.AllCache => true,
                PatientDetailSource.EmpiCache => true,
                PatientDetailSource.PhsaCache => true,
                _ => false,
            };
        }

        private static bool ShouldCheckPhsa(PatientDetailSource source)
        {
            return source switch
            {
                PatientDetailSource.All => true,
                PatientDetailSource.AllCache => true,
                PatientDetailSource.Phsa => true,
                PatientDetailSource.PhsaCache => true,
                _ => false,
            };
        }

        private static bool ShouldCheckEmpi(PatientDetailSource source)
        {
            return source switch
            {
                PatientDetailSource.All => true,
                PatientDetailSource.AllCache => true,
                PatientDetailSource.Empi => true,
                PatientDetailSource.EmpiCache => true,
                _ => false,
            };
        }

        private async Task<PatientModel?> GetDemographicsAsync(OidType type, string identifier, PatientIdentifierType identifierType, bool disableIdValidation = false)
        {
            if (identifierType == PatientIdentifierType.Phn && !PhnValidator.IsValid(identifier))
            {
                this.logger.LogDebug("The PHN provided is invalid");
                throw new ProblemDetailsException(ExceptionUtility.CreateProblemDetails(ErrorMessages.PhnInvalid, HttpStatusCode.NotFound, nameof(PatientRepository)));
            }

            PatientModel? patient = await this.clientRegistriesDelegate.GetDemographicsAsync(type, identifier, disableIdValidation).ConfigureAwait(true);

            // Only cache if validation is enabled (as some clients could get invalid data) and when successful.
            if (!disableIdValidation && patient != null)
            {
                this.CachePatient(patient);
            }

            return patient;
        }

        private async Task<PatientQueryResult> Handle(PatientDetailsQuery query, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                throw new InvalidOperationException("cancellation was requested");
            }

            if (query.Hdid != null)
            {
                PatientModel? patient = ShouldCheckCache(query.Source) ? this.GetFromCache(query.Hdid, PatientIdentifierType.Hdid) : null;

                if (patient == null && ShouldCheckEmpi(query.Source))
                {
                    patient = await this.GetDemographicsAsync(OidType.Hdid, query.Hdid, PatientIdentifierType.Hdid).ConfigureAwait(true);
                }

                if (patient == null && ShouldCheckPhsa(query.Source))
                {
                    patient = await this.GetPatientIdentityAsync(query.Hdid).ConfigureAwait(true);
                }

                return new PatientQueryResult(new[] { patient! });
            }

            if (query.Phn != null)
            {
                PatientModel? patient = ShouldCheckCache(query.Source) ? this.GetFromCache(query.Phn, PatientIdentifierType.Phn) : null;

                if (patient == null)
                {
                    patient = await this.GetDemographicsAsync(OidType.Phn, query.Phn, PatientIdentifierType.Phn).ConfigureAwait(true);
                }

                return new PatientQueryResult(new[] { patient! });
            }

            throw new InvalidOperationException("Must specify either Hdid or Phn to query patient details");
        }

        private async Task<PatientModel> GetPatientIdentityAsync(string hdid)
        {
            PatientIdentityResult? patientIdentityResult = await this.patientIdentityApi.PatientLookupByHdidAsync(hdid).ConfigureAwait(true);

            // Map patientIdentityResult to patient model
            if (patientIdentityResult != null)
            {
                return new PatientModel();
            }

            throw new ProblemDetailsException(
                ExceptionUtility.CreateProblemDetails(ErrorMessages.ClientRegistryDoesNotReturnPerson, HttpStatusCode.NotFound, nameof(PatientRepository)));
        }

        /// <summary>
        /// Attempts to get the Patient model from the Generic Cache.
        /// </summary>
        /// <param name="identifier">The resource identifier used to determine the key to use.</param>
        /// <param name="identifierType">The type of patient identifier we are searching for.</param>
        /// <returns>The found Patient model or null.</returns>
        private PatientModel? GetFromCache(string identifier, PatientIdentifierType identifierType)
        {
            using Activity? activity = Source.StartActivity();
            PatientModel? retPatient = null;
            if (this.cacheTtl > 0)
            {
                switch (identifierType)
                {
                    case PatientIdentifierType.Hdid:
                        this.logger.LogDebug("Querying Patient Cache by HDID");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{identifier}");
                        break;

                    case PatientIdentifierType.Phn:
                        this.logger.LogDebug("Querying Patient Cache by PHN");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:PHN:{identifier}");
                        break;
                }

                string message = $"Patient with identifier {identifier} was {(retPatient == null ? "not" : string.Empty)} found in cache";
                this.logger.LogDebug("{Message}", message);
            }

            activity?.Stop();
            return retPatient;
        }

        /// <summary>
        /// Caches the Patient model if enabled.
        /// </summary>
        /// <param name="patientModel">The patient to cache.</param>
        private void CachePatient(PatientModel patientModel)
        {
            using Activity? activity = Source.StartActivity();
            string hdid = patientModel.Hdid;
            if (this.cacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache patient: {Hdid}", hdid);
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (!string.IsNullOrEmpty(patientModel.Hdid))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:HDID:{patientModel.Hdid}", patientModel, expiry);
                }

                if (!string.IsNullOrEmpty(patientModel.Phn))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:PHN:{patientModel.Phn}", patientModel, expiry);
                }
            }
            else
            {
                this.logger.LogDebug("Patient caching is disabled will not cache patient: {Hdid}", hdid);
            }

            activity?.Stop();
        }
    }
}
