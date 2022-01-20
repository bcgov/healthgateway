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
namespace HealthGateway.Laboratory.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using HealthGateway.Laboratory.Parsers;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        private const string LabConfigSectionKey = "Laboratory";
        private const string AuthConfigSectionName = "ClientAuthentication";
        private const string TokenCacheKey = "TokenCacheKey";
        private const string IsNullOrEmptyTokenErrorMessage = "The auth token is null or empty - unable to cache or proceed";

        private readonly ILaboratoryDelegate laboratoryDelegate;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<LaboratoryService> logger;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly LaboratoryConfig labConfig;
        private readonly Uri tokenUri;
        private readonly int tokenCacheMinutes;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        /// <param name="authDelegate">The OAuth2 authentication service.</param>
        /// <param name="memoryCache">The cache to use to reduce lookups.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public LaboratoryService(
            IConfiguration configuration,
            ILogger<LaboratoryService> logger,
            ILaboratoryDelegateFactory laboratoryDelegateFactory,
            IAuthenticationDelegate authDelegate,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor)
        {
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
            this.authDelegate = authDelegate;

            IConfigurationSection? configSection = configuration?.GetSection(AuthConfigSectionName);
            this.tokenUri = configSection.GetValue<Uri>(@"TokenUri");
            this.tokenCacheMinutes = configSection.GetValue<int>("TokenCacheExpireMinutes", 20);
            this.tokenRequest = new ClientCredentialsTokenRequest();
            configSection.Bind(this.tokenRequest); // Client ID, Client Secret, Audience, Username, Password

            this.labConfig = new();
            configuration.Bind(LabConfigSectionKey, this.labConfig);

            this.memoryCache = memoryCache;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<Covid19Model>>> GetCovid19Orders(string hdid, int pageIndex = 0)
        {
            RequestResult<IEnumerable<Covid19Model>> retVal = new();

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    RequestResult<IEnumerable<PhsaCovid19Order>> delegateResult = await this.laboratoryDelegate.GetCovid19Orders(accessToken, hdid, pageIndex).ConfigureAwait(true);
                    if (delegateResult.ResultStatus == ResultType.Success)
                    {
                        retVal.ResultStatus = delegateResult.ResultStatus;
                        retVal.ResourcePayload = Covid19Model.FromPHSAModelList(delegateResult.ResourcePayload);
                        retVal.PageIndex = delegateResult.PageIndex;
                        retVal.PageSize = delegateResult.PageSize;
                        retVal.TotalResultCount = delegateResult.TotalResultCount;
                        return retVal;
                    }
                    else
                    {
                        retVal.ResultStatus = delegateResult.ResultStatus;
                        retVal.ResultError = delegateResult.ResultError;
                        return retVal;
                    }
                }
            }

            retVal.ResultError = UnauthorizedResultError();
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid)
        {
            RequestResult<LaboratoryReport> retVal = new();

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    retVal = await this.laboratoryDelegate.GetLabReport(id, hdid, accessToken).ConfigureAwait(true);
                }
            }

            retVal.ResultError = UnauthorizedResultError();
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<AuthenticatedRapidTestResponse>> CreateRapidTestAsync(string hdid, AuthenticatedRapidTestRequest rapidTestRequest)
        {
            RequestResult<AuthenticatedRapidTestResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new AuthenticatedRapidTestResponse(),
            };

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? accessToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);

                if (accessToken != null)
                {
                    RequestResult<RapidTestResponse> result = await this.laboratoryDelegate.SubmitRapidTestAsync(hdid, accessToken, rapidTestRequest).ConfigureAwait(true);
                    RapidTestResponse payload = result.ResourcePayload ?? new RapidTestResponse();

                    retVal.ResultStatus = result.ResultStatus;
                    retVal.ResultError = result.ResultError;
                    retVal.ResourcePayload = new AuthenticatedRapidTestResponse() { Phn = payload.Phn, Records = PhsaModelParser.FromPhsaModelList(payload.RapidTestResults) };
                }
            }

            retVal.ResultError = UnauthorizedResultError();
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PublicCovidTestResponse>> GetPublicCovidTestsAsync(string phn, string dateOfBirthString, string collectionDateString)
        {
            RequestResult<PublicCovidTestResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new PublicCovidTestResponse(),
            };

            DateOnly dateOfBirth;
            try
            {
                dateOfBirth = DateOnly.ParseExact(dateOfBirthString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
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

            DateOnly collectionDate;
            try
            {
                collectionDate = DateOnly.ParseExact(collectionDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (Exception e) when (e is FormatException || e is ArgumentNullException)
            {
                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = new RequestResultError()
                {
                    ResultMessage = "Error parsing collection date",
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

            string? accessToken = this.RetrieveAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogCritical(IsNullOrEmptyTokenErrorMessage);
                retVal.ResultError = UnauthorizedResultError();
                return retVal;
            }

            RequestResult<PHSAResult<IEnumerable<CovidTestResult>>> result = await this.laboratoryDelegate.GetPublicTestResults(accessToken, phn, dateOfBirth, collectionDate).ConfigureAwait(true);
            IEnumerable<CovidTestResult> payload = result.ResourcePayload?.Result ?? Enumerable.Empty<CovidTestResult>();
            PHSALoadState? loadState = result.ResourcePayload?.LoadState;

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload.Any())
            {
                LabIndicatorType labIndicatorType = Enum.Parse<LabIndicatorType>(payload.Select(x => x.StatusIndicator).First());

                if (labIndicatorType == LabIndicatorType.Found)
                {
                    retVal.ResourcePayload = new PublicCovidTestResponse(payload.Select(PublicCovidTestRecord.FromModel).ToList());
                }

                if (labIndicatorType == LabIndicatorType.DataMismatch || labIndicatorType == LabIndicatorType.NotFound)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
                }

                if (labIndicatorType == LabIndicatorType.Threshold || labIndicatorType == LabIndicatorType.Blocked)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.RecordsNotAvailable, ActionType.Invalid);
                }
            }

            if (loadState != null)
            {
                retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                if (loadState.RefreshInProgress)
                {
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Refresh in progress", ActionType.Refresh);
                    retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.labConfig.BackOffMilliseconds);
                }
            }

            return retVal;
        }

        private static RequestResultError? UnauthorizedResultError()
        {
            return new()
            {
                ResultMessage = "Error authenticating with KeyCloak",
                ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
            };
        }

        private string? RetrieveAccessToken()
        {
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
            }

            return accessToken;
        }
    }
}
