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
namespace HealthGateway.Immunization.Services
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Utils;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Vaccine Status data service.
    /// </summary>
    public class VaccineStatusService : IVaccineStatusService
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private const string AuthConfigSectionName = "ClientAuthentication";
        private const string TokenCacheKey = "TokenCacheKey";

        private readonly IVaccineStatusDelegate vaccineStatusDelegate;
        private readonly IVaccineProofService vpService;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<VaccineStatusService> logger;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly PHSAConfig phsaConfig;
        private readonly Uri tokenUri;
        private readonly int tokenCacheMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="VaccineStatusService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="vaccineStatusDelegate">The injected vaccine status delegate.</param>
        /// <param name="vaccineProofService">The injected vaccine proof service.</param>
        /// <param name="memoryCache">The cache to use to reduce lookups.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            ILogger<VaccineStatusService> logger,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IVaccineProofService vaccineProofService,
            IMemoryCache memoryCache)
        {
            this.authDelegate = authDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;
            this.vpService = vaccineProofService;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenCacheMinutes = configSection.GetValue<int>("TokenCacheExpireMinutes", 20);
            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password

            this.phsaConfig = new();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);

            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetVaccineStatus(string phn, string dateOfBirth, string dateOfVaccine)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

            DateTime dob;
            try
            {
                dob = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing date of birth",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            DateTime dov;
            try
            {
                dov = DateTime.ParseExact(dateOfVaccine, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing date of vaccine",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            if (!PHNValidator.IsValid(phn))
            {
                retVal.ResultStatus = Common.Constants.ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing phn",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            VaccineStatusQuery query = new()
            {
                PersonalHealthNumber = phn,
                DateOfBirth = dob,
                DateOfVaccine = dov,
            };

            this.memoryCache.TryGetValue(TokenCacheKey, out string? accessToken);
            if (accessToken == null)
            {
                this.logger.LogInformation("Access token not found in cache");
                accessToken = this.authDelegate.AuthenticateAsUser(this.tokenUri, this.tokenRequest).AccessToken;
                if (!string.IsNullOrEmpty(accessToken))
                {
                    this.logger.LogInformation("Attempting to store Access token in cache");
                    MemoryCacheEntryOptions cacheEntryOptions = new();
                    cacheEntryOptions.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(this.tokenCacheMinutes));
                    this.memoryCache.Set(TokenCacheKey, accessToken, cacheEntryOptions);
                }
                else
                {
                    this.logger.LogCritical("The auth token is null or empty - unable to cache or proceed");
                    retVal.ResultError = new()
                    {
                        ResultMessage = "Error authenticating with KeyCloak",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    };
                    return retVal;
                }
            }

            RequestResult<PHSAResult<VaccineStatusResult>> result =
                await this.vaccineStatusDelegate.GetVaccineStatus(query, accessToken, true).ConfigureAwait(true);
            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload == null)
            {
                retVal.ResourcePayload = new();
                retVal.ResourcePayload.State = VaccineState.NotFound;
            }
            else
            {
                retVal.ResourcePayload = VaccineStatus.FromModel(payload, phn);
                retVal.ResourcePayload.State = retVal.ResourcePayload.State switch
                {
                    var state when state == VaccineState.Threshold || state == VaccineState.Blocked => VaccineState.NotFound,
                    _ => retVal.ResourcePayload.State,
                };

                if (retVal.ResourcePayload.State == VaccineState.DataMismatch)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
                }
            }

            if (result.ResourcePayload != null)
            {
                retVal.ResourcePayload.Loaded = !result.ResourcePayload.LoadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(result.ResourcePayload.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GetVaccineStatusPDF(string phn, string dateOfBirth, string dateOfVaccine, VaccineProofTemplate proofTemplate)
        {
            RequestResult<ReportModel> retVal;
            RequestResult<VaccineStatus> vsResult = await this.GetVaccineStatus(phn, dateOfBirth, dateOfVaccine).ConfigureAwait(true);
            if (vsResult.ResultStatus == ResultType.Success && vsResult.ResourcePayload != null && vsResult.ResourcePayload.Loaded)
            {
                retVal = await this.GetVaccineProof(phn, vsResult.ResourcePayload, proofTemplate).ConfigureAwait(true);
            }
            else
            {
                retVal = new()
                {
                    ResultStatus = vsResult.ResultStatus,
                    ResultError = vsResult.ResultError,
                };
            }

            return retVal;
        }

        private async Task<RequestResult<ReportModel>> GetVaccineProof(string phn, VaccineStatus vaccineStatus, VaccineProofTemplate proofTemplate)
        {
            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };
            VaccineState state = vaccineStatus.State;
            if (state == VaccineState.NotFound || state == VaccineState.DataMismatch || state == VaccineState.Threshold || state == VaccineState.Blocked)
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = "Vaccine status not found", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.PHSA) };
            }
            else
            {
                VaccinationStatus requestState = state switch
                {
                    VaccineState.AllDosesReceived => VaccinationStatus.Fully,
                    VaccineState.PartialDosesReceived => VaccinationStatus.Partially,
                    VaccineState.Exempt => VaccinationStatus.Exempt,
                    _ => VaccinationStatus.Unknown,
                };

                if (requestState != VaccinationStatus.Unknown)
                {
                    VaccineProofRequest request = new()
                    {
                        Status = requestState,
                        SmartHealthCardQr = vaccineStatus.QRCode.Data!,
                    };

                    RequestResult<ReportModel> assetResult = await this.vpService.GetVaccineProof(phn, request, proofTemplate).ConfigureAwait(true);
                    if (assetResult.ResultStatus == ResultType.Success && assetResult.ResourcePayload != null)
                    {
                        retVal.ResourcePayload = assetResult.ResourcePayload;
                        retVal.ResultStatus = ResultType.Success;
                    }
                    else
                    {
                        retVal.ResultError = assetResult.ResultError;
                    }
                }
                else
                {
                    retVal.ResultError = new RequestResultError() { ResultMessage = "Vaccine status is unknown", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.BCMP) };
                }
            }

            return retVal;
        }
    }
}
