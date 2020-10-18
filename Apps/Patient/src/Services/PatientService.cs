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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Patient.Delegates;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Patient data service.
    /// </summary>
    public class PatientService : IPatientService
    {
        /// <summary>
        /// The resource identifier that represents an HDID.
        /// </summary>
        private const string HDIDIdentifier = "hdid";

        /// <summary>
        /// The resource identifier that represents a PHN.
        /// </summary>
        private const string PHNIdentifier = "phn";

        /// <summary>
        /// The generic cache domain to store patients against.
        /// </summary>
        private const string PatientCacheDomain = "Patient";

        /// <summary>
        /// The injected logger delegate.
        /// </summary>
        private readonly ILogger<PatientService> logger;
        private readonly IConfiguration configuration;
        private readonly IPatientDelegate patientDelegate;
        private readonly IGenericCacheDelegate cacheDelegate;
        private readonly int cacheTTL;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="logger">The service Logger.</param>
        /// <param name="configuration">The Configuration to use.</param>
        /// <param name="patientDelegate">The injected client registries delegate.</param>
        /// <param name="genericCacheDelegate">The delegate responsible for caching.</param>
        public PatientService(ILogger<PatientService> logger, IConfiguration configuration, IPatientDelegate patientDelegate, IGenericCacheDelegate genericCacheDelegate)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.patientDelegate = patientDelegate;
            this.cacheDelegate = genericCacheDelegate;
            this.cacheTTL = this.configuration.GetSection("PatientService").GetValue<int>("CacheTTL", 0);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> GetPatient(string hdid)
        {
            return await this.SearchPatientByIdentifier(new ResourceIdentifier(HDIDIdentifier, hdid)).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<RequestResult<PatientModel>> SearchPatientByIdentifier(ResourceIdentifier identifier)
        {
            RequestResult<PatientModel> requestResult = new RequestResult<PatientModel>();
            PatientModel? patient = this.GetFromCache(identifier);
            if (patient == null)
            {
                if (identifier.Key == HDIDIdentifier || identifier.Key == PHNIdentifier)
                {
                    if (identifier.Key == HDIDIdentifier)
                    {
                        this.logger.LogDebug("Performing Patient lookup by HDID");
                        requestResult = await this.patientDelegate.GetDemographicsByHDIDAsync(identifier.Value).ConfigureAwait(true);
                    }
                    else if (identifier.Key == PHNIdentifier)
                    {
                        this.logger.LogDebug("Performing Patient lookup by PHN");
                        requestResult = await this.patientDelegate.GetDemographicsByPHNAsync(identifier.Value).ConfigureAwait(true);
                    }

                    if (requestResult.ResultStatus == ResultType.Success && requestResult.ResourcePayload != null)
                    {
                        this.CachePatient(requestResult.ResourcePayload);
                    }
                }
                else
                {
                    this.logger.LogDebug($"Failed Patient lookup key {identifier.Key} is unknown");
                    requestResult.ResultStatus = ResultType.Error;
                    requestResult.ResultError = new RequestResultError() { ResultMessage = $"Identifier not recognized '{identifier.Key}'", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) };
                }
            }
            else
            {
                this.logger.LogDebug("Patient fetched from Cache");
                requestResult.ResourcePayload = patient;
                requestResult.ResultStatus = ResultType.Success;
            }

            return requestResult;
        }

        /// <summary>
        /// Attempts to get the Patient model from the Generic Cache.
        /// </summary>
        /// <param name="identifier">The resource identifier used to determine the key to use.</param>
        /// <returns>The found Patient model or null.</returns>
        private PatientModel? GetFromCache(ResourceIdentifier identifier)
        {
            PatientModel? retPatient = null;
            if (this.cacheTTL > 0)
            {
                bool hdidSearch = identifier.Key == HDIDIdentifier;
                this.logger.LogDebug($"Querying Patient Cache for {(hdidSearch ? "HDID" : "PHN")}: {identifier.Value}");
                if (hdidSearch)
                {
                    retPatient = this.cacheDelegate.GetCacheObject<PatientModel>(identifier.Value, PatientCacheDomain);
                }
                else
                {
                    retPatient = this.cacheDelegate.GetCacheObjectByJSONProperty<PatientModel>("personalhealthnumber", identifier.Value, PatientCacheDomain);
                }

                this.logger.LogDebug($"Patient with identifier {identifier.Value} was {(retPatient == null ? "not" : string.Empty)} found in cache");
            }

            return retPatient;
        }

        /// <summary>
        /// Caches the Patient model if enabled.
        /// </summary>
        /// <param name="patient">The patient to cache.</param>
        private void CachePatient(PatientModel patient)
        {
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
        }
    }
}
