//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Patient.Services
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Patient.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class PatientService : IPatientService
    {
        /// <summary>
        /// The generic cache domain to store patients against.
        /// </summary>
        private const string PatientCacheDomain = "Patient";
        private readonly ICacheProvider cacheProvider;
        private readonly int cacheTtl;

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientService> logger;
        private readonly IClientRegistriesDelegate clientRegistriesDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="clientRegistriesDelegate">The injected client registries delegate.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        public PatientService(ILogger<PatientService> logger, IConfiguration configuration, IClientRegistriesDelegate clientRegistriesDelegate, ICacheProvider cacheProvider)
        {
            this.logger = logger;
            this.clientRegistriesDelegate = clientRegistriesDelegate;
            this.cacheProvider = cacheProvider;
            this.cacheTtl = configuration.GetSection("PatientService").GetValue("CacheTTL", 0);
        }

        private static ActivitySource Source { get; } = new(nameof(PatientService));

        /// <inheritdoc/>
        public async Task<ApiResult<PatientModel>> GetPatient(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.Hdid, bool disableIdValidation = false)
        {
            using Activity? activity = Source.StartActivity();

            ApiResult<PatientModel> apiResult = new()
            {
                ResourcePayload = this.GetFromCache(identifier, identifierType),
            };

            if (apiResult.ResourcePayload == null)
            {
                OidType type = GetOidType(identifierType);
                this.logger.LogDebug("Starting GetPatient for identifier type: {IdentifierType}", identifierType);

                if (identifierType == PatientIdentifierType.Phn && !PhnValidator.IsValid(identifier))
                {
                    this.logger.LogDebug("The PHN provided is invalid");
                    throw new ApiException(ErrorMessages.PhnInvalid, "PatientService.GetPatient", HttpStatusCode.NotFound);
                }

                apiResult = await this.clientRegistriesDelegate.GetDemographicsAsync(type, identifier, disableIdValidation).ConfigureAwait(true);

                // Only cache if validation is enabled (as some clients could get invalid data) and when successful.
                if (!disableIdValidation && apiResult.ResourcePayload != null)
                {
                    this.CachePatient(apiResult.ResourcePayload);
                }
            }

            activity?.Stop();

            return apiResult;
        }

        private static OidType GetOidType(PatientIdentifierType type)
        {
            return type == PatientIdentifierType.Phn ? OidType.Phn : OidType.Hdid;
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
        /// <param name="patient">The patient to cache.</param>
        private void CachePatient(PatientModel patient)
        {
            using Activity? activity = Source.StartActivity();
            string hdid = patient.HdId;
            if (this.cacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache patient: {Hdid}", hdid);
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (!string.IsNullOrEmpty(patient.HdId))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:HDID:{patient.HdId}", patient, expiry);
                }

                if (!string.IsNullOrEmpty(patient.PersonalHealthNumber))
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:PHN:{patient.PersonalHealthNumber}", patient, expiry);
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
