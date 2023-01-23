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
    using System.Net;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
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
        public async Task<RequestResult<string>> GetPatientPhn(string hdid)
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
        public async Task<string> GetPatientHdid(string phn)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<PatientModel> patientResult = await this.GetPatient(phn, PatientIdentifierType.Phn).ConfigureAwait(true);
            if (patientResult.ResultStatus != ResultType.Success || patientResult.ResourcePayload == null)
            {
                throw new ProblemDetailsException(
                    ExceptionUtility.CreateProblemDetails(
                        patientResult.ResultError?.ResultMessage ?? "Unspecified error",
                        HttpStatusCode.InternalServerError,
                        nameof(PatientService)));
            }

            return patientResult.ResourcePayload.HdId;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PatientModel>> GetPatient(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.Hdid, bool disableIdValidation = false)
        {
            using Activity? activity = Source.StartActivity();

            RequestResult<PatientModel> patient = await this.GetPatientInternal(identifier, identifierType, disableIdValidation).ConfigureAwait(true);

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

        private async Task<RequestResult<PatientModel>> GetPatientInternal(string identifier, PatientIdentifierType identifierType = PatientIdentifierType.Hdid, bool disableIdValidation = false)
        {
            PatientModel? patient = this.GetFromCache(identifier, identifierType);
            if (patient == null)
            {
                RequestResult<PatientModel> patientResult = identifierType switch
                {
                    PatientIdentifierType.Hdid => await this.GetPatientByHdid(identifier, disableIdValidation).ConfigureAwait(true),
                    PatientIdentifierType.Phn => await this.GetPatientByPhn(identifier, disableIdValidation).ConfigureAwait(true),
                    _ => throw new NotImplementedException(),
                };
                if (patientResult.ResultStatus != ResultType.Success)
                {
                    this.logger.LogError("Failed to get patient information: {ErrorMessage}", patientResult.ResultError?.ResultMessage);
                    return patientResult;
                }

                patient = patientResult.ResourcePayload!;
            }

            this.CachePatient(patient);

            return RequestResultFactory.Success(patient);
        }

        private async Task<RequestResult<PatientModel>> GetPatientByPhn(string phn, bool disableIdValidation)
        {
            if (!PhnValidator.IsValid(phn))
            {
                return RequestResultFactory.ActionRequired<PatientModel>(ActionType.Validation, $"PHN' {phn}' is invalid");
            }

            return await this.patientDelegate.GetDemographicsByPhnAsync(phn, disableIdValidation).ConfigureAwait(true);
        }

        private async Task<RequestResult<PatientModel>> GetPatientByHdid(string hdid, bool disableIdValidation)
        {
            if (string.IsNullOrEmpty(hdid))
            {
                return RequestResultFactory.ActionRequired<PatientModel>(ActionType.Validation, $"HDID '{hdid}' is invalid");
            }

            return await this.patientDelegate.GetDemographicsByHdidAsync(hdid, disableIdValidation).ConfigureAwait(true);
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
            if (this.cacheTtl == 0)
            {
                return null;
            }

            PatientModel? patient = identifierType switch
            {
                PatientIdentifierType.Hdid => this.cacheProvider.GetItem<PatientModel>(GetPatientHdidCacheKey(identifier)),
                PatientIdentifierType.Phn => this.cacheProvider.GetItem<PatientModel>(GetPatientPhnCacheKey(identifier)),
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
        private void CachePatient(PatientModel patient)
        {
            using Activity? activity = Source.StartActivity();
            string hdid = patient.HdId;
            if (this.cacheTtl > 0)
            {
                this.logger.LogDebug("Attempting to cache patient: {Hdid}", hdid);
                TimeSpan expiry = TimeSpan.FromMinutes(this.cacheTtl);
                if (!patient.HdId.IsNullOrEmpty())
                {
                    this.cacheProvider.AddItem(GetPatientHdidCacheKey(patient.HdId), patient, expiry);
                }

                if (!patient.PersonalHealthNumber.IsNullOrEmpty())
                {
                    this.cacheProvider.AddItem(GetPatientPhnCacheKey(patient.PersonalHealthNumber), patient, expiry);
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
