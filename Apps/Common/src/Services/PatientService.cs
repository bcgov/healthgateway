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
    using System.Diagnostics;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
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
        private readonly IGenericCacheDelegate cacheDelegate;
        private readonly int cacheTTL;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientDelegate">The injected client registries delegate.</param>
        /// <param name="genericCacheDelegate">The delegate responsible for caching.</param>
        public PatientService(ILogger<PatientService> logger, IConfiguration configuration, IClientRegistriesDelegate patientDelegate, IGenericCacheDelegate genericCacheDelegate)
        {
            this.logger = logger;
            this.patientDelegate = patientDelegate;
            this.cacheDelegate = genericCacheDelegate;
            this.cacheTTL = configuration.GetSection("PatientService").GetValue<int>("CacheTTL", 0);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(PatientService));

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<RequestResult<string>> GetPatientPHN(string hdid)
        {
            using Activity? activity = Source.StartActivity("GetPatientPHN");
            RequestResult<string> retVal = new RequestResult<string>()
            {
                ResultError = new RequestResultError() { ResultMessage = "Error during PHN retrieval", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ClientRegistries) },
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
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetPatient(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.HDID, bool disableIdValidation = false)
        {
            using Activity? activity = Source.StartActivity("GetPatient");
            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>();
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
                            requestResult.ResultError = new RequestResultError() { ResultMessage = $"Internal Error: PatientIdentifier is invalid '{identifier}'", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                            this.logger.LogDebug($"The PHN provided is invalid: {identifier}");
                        }

                        break;
                    default:
                        this.logger.LogDebug($"Failed Patient lookup unknown PatientIdentifierType");
                        requestResult.ResultStatus = ResultType.Error;
                        requestResult.ResultError = new RequestResultError() { ResultMessage = $"Internal Error: PatientIdentifierType is unknown '{identifierType.ToString()}'", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
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
            using Activity? activity = Source.StartActivity("GetFromCache");
            PatientModel? retPatient = null;
            if (this.cacheTTL > 0)
            {
                switch (identifierType)
                {
                    case PatientIdentifierType.HDID:
                        this.logger.LogDebug($"Querying Patient Cache by HDID");
                        retPatient = this.cacheDelegate.GetCacheObject<PatientModel>(identifier, PatientCacheDomain);
                        break;
                    case PatientIdentifierType.PHN:
                        this.logger.LogDebug($"Querying Patient Cache by PHN");
                        retPatient = this.cacheDelegate.GetCacheObjectByJSONProperty<PatientModel>("personalhealthnumber", identifier, PatientCacheDomain);
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
            using Activity? activity = Source.StartActivity("CachePatient");
            string hdid = patient.HdId;
            if (this.cacheTTL > 0)
            {
                this.logger.LogDebug($"Attempting to cache patient: {hdid}");
                DBResult<GenericCache> dbResult = this.cacheDelegate.CacheObject(patient, hdid, PatientCacheDomain, this.cacheTTL, true);
                if (dbResult.Status == Database.Constants.DBStatusCode.Created)
                {
                    this.logger.LogDebug($"Created cache for patient: {hdid}");
                }
                else
                {
                    this.logger.LogWarning($"Unable to cache patient: {hdid}, Status: {dbResult.Status.ToString()}, Message = {dbResult.Message}");
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
