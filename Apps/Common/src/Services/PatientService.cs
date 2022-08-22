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
namespace HealthGateway.Common.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
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

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientService> logger;
        private readonly IClientRegistriesDelegate patientDelegate;
        private readonly ICacheProvider cacheProvider;
        private readonly int cacheTtl;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientDelegate">The injected client registries delegate.</param>
        /// <param name="cacheProvider">The provider responsible for caching.</param>
        public PatientService(ILogger<PatientService> logger, IConfiguration configuration, IClientRegistriesDelegate patientDelegate, ICacheProvider cacheProvider)
        {
            this.logger = logger;
            this.patientDelegate = patientDelegate;
            this.cacheProvider = cacheProvider;
            this.cacheTtl = configuration.GetSection("PatientService").GetValue("CacheTTL", 0);
        }

        private static ActivitySource Source { get; } = new(nameof(PatientService));

        /// <inheritdoc/>
        public async Task<RequestResult<string>> GetPatientPHN(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<string> retVal = new()
            {
                ResultError = new RequestResultError
                {
                    ResultMessage = "Error during PHN retrieval",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries),
                },
                ResultStatus = ResultType.Error,
            };
            RequestResult<PatientModel> patientResult = await this.GetPatient(hdid).ConfigureAwait(true);
            if (patientResult != null)
            {
                retVal.ResultError = patientResult.ResultError;
                retVal.ResultStatus = patientResult.ResultStatus;
                if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
                {
                    retVal.ResourcePayload = patientResult.ResourcePayload.PersonalHealthNumber;
                }
            }

            activity?.Stop();
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PatientModel>> GetPatient(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.HDID, bool disableIdValidation = false)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<PatientModel> requestResult = new();
            PatientModel? patient = this.GetFromCache(identifier, identifierType);
            if (patient == null)
            {
                switch (identifierType)
                {
                    case PatientIdentifierType.HDID:
                        this.logger.LogDebug("Performing Patient lookup by HDID");
                        requestResult = await this.patientDelegate.GetDemographicsByHDIDAsync(identifier, disableIdValidation).ConfigureAwait(true);
                        break;
                    case PatientIdentifierType.PHN:
                        this.logger.LogDebug("Performing Patient lookup by PHN");
                        if (PhnValidator.IsValid(identifier))
                        {
                            requestResult = await this.patientDelegate.GetDemographicsByPHNAsync(identifier, disableIdValidation).ConfigureAwait(true);
                        }
                        else
                        {
                            requestResult.ResultStatus = ResultType.ActionRequired;
                            requestResult.ResultError = new()
                            {
                                ResultMessage = $"Internal Error: PatientIdentifier is invalid '{identifier}'",
                                ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                            };
                            this.logger.LogDebug($"The PHN provided is invalid: {identifier}");
                        }

                        break;
                    default:
                        this.logger.LogDebug("Failed Patient lookup unknown PatientIdentifierType");
                        requestResult.ResultStatus = ResultType.Error;
                        requestResult.ResultError = new()
                        {
                            ResultMessage = $"Internal Error: PatientIdentifierType is unknown '{identifierType.ToString()}'",
                            ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                        };
                        break;
                }

                // Only cache if validation is enabled (as some clients could get invalid data) and when successful.
                if (!disableIdValidation && requestResult.ResultStatus == ResultType.Success && requestResult.ResourcePayload != null)
                {
                    this.CachePatient(requestResult.ResourcePayload);
                }
            }
            else
            {
                this.logger.LogDebug("Patient fetched from Cache");
                requestResult.ResourcePayload = patient;
                requestResult.ResultStatus = ResultType.Success;
            }

            activity?.Stop();
            return requestResult;
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
                    case PatientIdentifierType.HDID:
                        this.logger.LogDebug("Querying Patient Cache by HDID");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:HDID:{identifier}");
                        break;
                    case PatientIdentifierType.PHN:
                        this.logger.LogDebug("Querying Patient Cache by PHN");
                        retPatient = this.cacheProvider.GetItem<PatientModel>($"{PatientCacheDomain}:PHN:{identifier}");
                        break;
                }

                this.logger.LogDebug($"Patient with identifier {identifier} was {(retPatient == null ? "not" : string.Empty)} found in cache");
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
                this.logger.LogDebug($"Attempting to cache patient: {hdid}");
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (patient.HdId != null)
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:HDID:{patient.HdId}", patient, expiry);
                }

                if (patient.PersonalHealthNumber != null)
                {
                    this.cacheProvider.AddItem($"{PatientCacheDomain}:PHN:{patient.PersonalHealthNumber}", patient, expiry);
                }
            }
            else
            {
                this.logger.LogDebug($"Patient caching is disabled will not cache patient: {hdid}");
            }

            activity?.Stop();
        }
    }
}
