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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
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
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
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
        /// <param name="memoryCache">The cache to use to reduce lookups.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        public VaccineStatusService(
            IConfiguration configuration,
            ILogger<VaccineStatusService> logger,
            IAuthenticationDelegate authDelegate,
            IVaccineStatusDelegate vaccineStatusDelegate,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor)
        {
            this.authDelegate = authDelegate;
            this.vaccineStatusDelegate = vaccineStatusDelegate;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenCacheMinutes = configSection.GetValue<int>("TokenCacheExpireMinutes", 20);
            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password

            this.phsaConfig = new();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);

            this.memoryCache = memoryCache;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatus(string phn, string dateOfBirth, string dateOfVaccine)
        {
            bool includeVaccineProof = false;
            return await this.GetPublicVaccineStatusWithOptionalProof(phn, dateOfBirth, dateOfVaccine, includeVaccineProof).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatus(string hdid)
        {
            bool includeVaccineProof = false;
            return await this.GetAuthenticatedVaccineStatusWithOptionalProof(hdid, includeVaccineProof).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetPublicVaccineProof(string phn, string dateOfBirth, string dateOfVaccine)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetPublicVaccineStatusWithOptionalProof(phn, dateOfBirth, dateOfVaccine, includeVaccineProof).ConfigureAwait(true);
            VaccineStatus payload = vaccineStatusResult.ResourcePayload!;

            RequestResult<VaccineProofDocument> retVal = new()
            {
                ResultStatus = vaccineStatusResult.ResultStatus,
                ResultError = vaccineStatusResult.ResultError,
                TotalResultCount = vaccineStatusResult.TotalResultCount,
                PageIndex = vaccineStatusResult.PageIndex,
                ResourcePayload = new()
                {
                    Loaded = payload.Loaded,
                    RetryIn = payload.RetryIn,
                    Document = payload.FederalVaccineProof ?? new(),
                    QRCode = payload.QRCode,
                },
            };

            if (vaccineStatusResult.ResultStatus == ResultType.Success)
            {
                if (payload.State == VaccineState.NotFound)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine Proof document is not available.", ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError() { ResultMessage = "Vaccine Proof document is not available.", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.PHSA) };
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineProofDocument>> GetAuthenticatedVaccineProof(string hdid)
        {
            bool includeVaccineProof = true;
            RequestResult<VaccineStatus> vaccineStatusResult = await this.GetAuthenticatedVaccineStatusWithOptionalProof(hdid, includeVaccineProof).ConfigureAwait(true);
            VaccineStatus payload = vaccineStatusResult.ResourcePayload!;

            RequestResult<VaccineProofDocument> retVal = new()
            {
                ResultStatus = vaccineStatusResult.ResultStatus,
                ResultError = vaccineStatusResult.ResultError,
                TotalResultCount = vaccineStatusResult.TotalResultCount,
                PageIndex = vaccineStatusResult.PageIndex,
                ResourcePayload = new()
                {
                    Loaded = payload.Loaded,
                    RetryIn = payload.RetryIn,
                    Document = payload.FederalVaccineProof ?? new(),
                    QRCode = payload.QRCode,
                },
            };

            if (vaccineStatusResult.ResultStatus == ResultType.Success)
            {
                if (payload.State == VaccineState.NotFound)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine Proof document is not available.", ActionType.Invalid);
                }
                else if (payload.Loaded && string.IsNullOrEmpty(payload.FederalVaccineProof?.Data))
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError() { ResultMessage = "Vaccine Proof document is not available.", ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.PHSA) };
                }
            }

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetPublicVaccineStatusWithOptionalProof(string phn, string dateOfBirth, string dateOfVaccine, bool includeVaccineProof)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            DateTime dob;
            try
            {
                dob = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = ResultType.Error;
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
                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing date of vaccine",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            if (!PhnValidator.IsValid(phn))
            {
                retVal.ResultStatus = ResultType.Error;
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
                IncludeFederalVaccineProof = includeVaccineProof,
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

            retVal = await this.GetVaccineStatusFromDelegate(query, accessToken, phn).ConfigureAwait(true);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetAuthenticatedVaccineStatusWithOptionalProof(string hdid, bool includeVaccineProof)
        {
            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            VaccineStatusQuery query = new()
            {
                HdId = hdid,
                IncludeFederalVaccineProof = includeVaccineProof,
            };

            RequestResult<VaccineStatus> retVal = await this.GetVaccineStatusFromDelegate(query, bearerToken).ConfigureAwait(true);

            return retVal;
        }

        private async Task<RequestResult<VaccineStatus>> GetVaccineStatusFromDelegate(VaccineStatusQuery query, string accessToken, string? phn = null)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new() { State = VaccineState.NotFound },
            };

            bool isPublicEndpoint = string.IsNullOrEmpty(query.HdId);
            RequestResult<PHSAResult<VaccineStatusResult>> result = await this.vaccineStatusDelegate.GetVaccineStatus(query, accessToken, isPublicEndpoint).ConfigureAwait(true);
            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            PHSALoadState? loadState = result.ResourcePayload?.LoadState;

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload != null)
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

            if (loadState != null)
            {
                retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                if (loadState.RefreshInProgress)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine status refresh in progress", ActionType.Refresh);
                    retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
                }
            }

            return retVal;
        }
    }
}
