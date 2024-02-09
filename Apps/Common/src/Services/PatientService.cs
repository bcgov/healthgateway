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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

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

        private readonly IClientRegistriesDelegate patientDelegate;

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
        public async Task<RequestResult<string>> GetPatientPhnAsync(string hdid, CancellationToken ct = default)
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
            RequestResult<PatientModel> patientResult = await this.GetPatientAsync(hdid, ct: ct);
            retVal.ResultError = patientResult.ResultError;
            retVal.ResultStatus = patientResult.ResultStatus;
            if (patientResult.ResultStatus == ResultType.Success && patientResult.ResourcePayload != null)
            {
                retVal.ResourcePayload = patientResult.ResourcePayload.PersonalHealthNumber;
            }

            activity?.Stop();
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<string> GetPatientHdidAsync(string phn, CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<PatientModel> patientResult = await this.GetPatientAsync(phn, PatientIdentifierType.Phn, ct: ct);
            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                throw new NotFoundException(patientResult.ResultError?.ResultMessage ?? "Unspecified error");
            }

            return patientResult.ResourcePayload.HdId;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PatientModel>> GetPatientAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();

            RequestResult<PatientModel> patient = await this.GetPatientInternalAsync(identifier, identifierType, disableIdValidation, ct);

            activity?.Stop();
            return patient;
        }

        private static string GetPatientPhnCacheKey(string phn)
        {
            return $"{PatientCacheDomain}:PHN:{phn}";
        }

        private static string GetPatientHdidCacheKey(string hdid)
        {
            return $"{PatientCacheDomain}:HDID:{hdid}";
        }

        private async Task<RequestResult<PatientModel>> GetPatientInternalAsync(
            string identifier,
            PatientIdentifierType identifierType = PatientIdentifierType.Hdid,
            bool disableIdValidation = false,
            CancellationToken ct = default)
        {
            PatientModel? patient = await this.GetFromCacheAsync(identifier, identifierType, ct);
            if (patient == null)
            {
                RequestResult<PatientModel> patientResult = identifierType switch
                {
                    PatientIdentifierType.Hdid => await this.GetPatientByHdidAsync(identifier, disableIdValidation, ct),
                    PatientIdentifierType.Phn => await this.GetPatientByPhnAsync(identifier, disableIdValidation, ct),
                    _ => throw new NotImplementedException(),
                };
                if (patientResult.ResultStatus != ResultType.Success)
                {
                    this.logger.LogError("Failed to get patient information: {ErrorMessage}", patientResult.ResultError?.ResultMessage);
                    return patientResult;
                }

                patient = patientResult.ResourcePayload!;
            }

            await this.CachePatientAsync(patient, ct);

            return RequestResultFactory.Success(patient);
        }

        private async Task<RequestResult<PatientModel>> GetPatientByPhnAsync(string phn, bool disableIdValidation, CancellationToken ct)
        {
            if (!PhnValidator.IsValid(phn))
            {
                return RequestResultFactory.ActionRequired<PatientModel>(ActionType.Validation, $"PHN' {phn}' is invalid");
            }

            return await this.patientDelegate.GetDemographicsByPhnAsync(phn, disableIdValidation, ct);
        }

        private async Task<RequestResult<PatientModel>> GetPatientByHdidAsync(string hdid, bool disableIdValidation, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(hdid))
            {
                return RequestResultFactory.ActionRequired<PatientModel>(ActionType.Validation, $"HDID '{hdid}' is invalid");
            }

            return await this.patientDelegate.GetDemographicsByHdidAsync(hdid, disableIdValidation, ct);
        }

        /// <summary>
        /// Attempts to get the Patient model from the Generic Cache.
        /// </summary>
        /// <param name="identifier">The resource identifier used to determine the key to use.</param>
        /// <param name="identifierType">The type of patient identifier we are searching for.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The found Patient model or null.</returns>
        private async Task<PatientModel?> GetFromCacheAsync(string identifier, PatientIdentifierType identifierType, CancellationToken ct)
        {
            using Activity? activity = Source.StartActivity();
            if (this.cacheTtl == 0)
            {
                return null;
            }

            PatientModel? patient = identifierType switch
            {
                PatientIdentifierType.Hdid => await this.cacheProvider.GetItemAsync<PatientModel>(GetPatientHdidCacheKey(identifier), ct),
                PatientIdentifierType.Phn => await this.cacheProvider.GetItemAsync<PatientModel>(GetPatientPhnCacheKey(identifier), ct),
                _ => throw new NotImplementedException(),
            };

            if (patient == null)
            {
                this.logger.LogDebug("Patient with identifier {Identifier} was NOT found in cache", identifier);
                return null;
            }

            this.logger.LogDebug("Patient with identifier {Identifier} was found in cache", identifier);
            activity?.Stop();
            return patient;
        }

        /// <summary>
        /// Caches the Patient model if enabled.
        /// </summary>
        /// <param name="patient">The patient to cache.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        private async Task CachePatientAsync(PatientModel patient, CancellationToken ct)
        {
            using Activity? activity = Source.StartActivity();
            string hdid = patient.HdId;
            if (this.cacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache patient: {Hdid}", hdid);
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (!patient.HdId.IsNullOrEmpty())
                {
                    await this.cacheProvider.AddItemAsync(GetPatientHdidCacheKey(patient.HdId), patient, expiry, ct);
                }

                if (!patient.PersonalHealthNumber.IsNullOrEmpty())
                {
                    await this.cacheProvider.AddItemAsync(GetPatientPhnCacheKey(patient.PersonalHealthNumber), patient, expiry, ct);
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
