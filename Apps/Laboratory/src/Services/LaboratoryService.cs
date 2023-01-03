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
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Factories;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        /// <summary>
        /// The configuration section name for Laboratory.
        /// </summary>
        public const string LabConfigSectionKey = "Laboratory";

        private const string AuthConfigSectionName = "PublicAuthentication";

        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILaboratoryDelegate laboratoryDelegate;
        private readonly ILogger<LaboratoryService> logger;
        private readonly IMapper autoMapper;
        private readonly LaboratoryConfig labConfig;
        private readonly ClientCredentialsTokenRequest tokenRequest;
        private readonly Uri tokenUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="autoMapper">The injected automapper.</param>
        public LaboratoryService(
            IConfiguration configuration,
            ILogger<LaboratoryService> logger,
            ILaboratoryDelegateFactory laboratoryDelegateFactory,
            IAuthenticationDelegate authenticationDelegate,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
            this.authenticationDelegate = authenticationDelegate;
            this.autoMapper = autoMapper;

            (this.tokenUri, this.tokenRequest) = this.authenticationDelegate.GetClientCredentialsAuth(AuthConfigSectionName);

            this.labConfig = new();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Covid19OrderResult>> GetCovid19Orders(string hdid, int pageIndex = 0)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                return RequestResultFactory.Error<Covid19OrderResult>(UnauthorizedResultError());
            }

            RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult = await this.laboratoryDelegate.GetCovid19Orders(accessToken, hdid, pageIndex).ConfigureAwait(true);

            PhsaLoadState? loadState = delegateResult.ResourcePayload?.LoadState;
            if (loadState != null && loadState.RefreshInProgress)
            {
                return RequestResultFactory.ActionRequired(
                    new Covid19OrderResult
                    {
                        RetryIn = Math.Max(loadState.BackOffMilliseconds, this.labConfig.BackOffMilliseconds),
                        Loaded = !loadState.RefreshInProgress,
                    },
                    ActionType.Refresh,
                    "Refresh in progress");
            }

            if (delegateResult.ResultStatus != ResultType.Success)
            {
                return RequestResultFactory.Error<Covid19OrderResult>(delegateResult.ResultError);
            }

            return RequestResultFactory.Success(
                new Covid19OrderResult
                {
                    Covid19Orders = this.autoMapper.Map<IEnumerable<PhsaCovid19Order>, IEnumerable<Covid19Order>>(delegateResult.ResourcePayload?.Result),
                },
                delegateResult.TotalResultCount,
                delegateResult.PageIndex,
                delegateResult.PageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryOrderResult>> GetLaboratoryOrders(string hdid)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                return RequestResultFactory.Error<LaboratoryOrderResult>(UnauthorizedResultError());
            }

            RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult = await this.laboratoryDelegate.GetLaboratorySummary(hdid, accessToken).ConfigureAwait(true);

            PhsaLoadState? loadState = delegateResult.ResourcePayload?.LoadState;
            if (loadState != null && loadState.RefreshInProgress)
            {
                return RequestResultFactory.ActionRequired(
                    new LaboratoryOrderResult
                    {
                        RetryIn = Math.Max(loadState.BackOffMilliseconds, this.labConfig.BackOffMilliseconds),
                        Loaded = !loadState.RefreshInProgress,
                    },
                    ActionType.Refresh,
                    "Refresh in progress");
            }

            if (delegateResult.ResultStatus != ResultType.Success)
            {
                return RequestResultFactory.Error<LaboratoryOrderResult>(delegateResult.ResultError);
            }

            return RequestResultFactory.Success(
                new LaboratoryOrderResult
                {
                    LaboratoryOrders = this.autoMapper.Map<IEnumerable<PhsaLaboratoryOrder>, IEnumerable<LaboratoryOrder>>(delegateResult.ResourcePayload?.Result?.LabOrders),
                },
                delegateResult.TotalResultCount,
                delegateResult.PageIndex,
                delegateResult.PageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(string id, string hdid, bool isCovid19)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                return RequestResultFactory.Error<LaboratoryReport>(UnauthorizedResultError());
            }

            return await this.laboratoryDelegate.GetLabReport(id, hdid, accessToken, isCovid19).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PublicCovidTestResponse>> GetPublicCovidTestsAsync(string phn, string dateOfBirthString, string collectionDateString)
        {
            if (!DateFormatter.TryParse(dateOfBirthString, "yyyy-MM-dd", out DateTime dateOfBirth))
            {
                return RequestResultFactory.Error<PublicCovidTestResponse>(ErrorType.InvalidState, "Error parsing date of birth");
            }

            if (!DateFormatter.TryParse(collectionDateString, "yyyy-MM-dd", out DateTime collectionDate))
            {
                return RequestResultFactory.Error<PublicCovidTestResponse>(ErrorType.InvalidState, "Error parsing collection date");
            }

            if (!PhnValidator.IsValid(phn))
            {
                return RequestResultFactory.Error<PublicCovidTestResponse>(ErrorType.InvalidState, "Error parsing phn");
            }

            string? accessToken = this.authenticationDelegate.AuthenticateAsSystem(this.tokenUri, this.tokenRequest).AccessToken;
            if (accessToken == null)
            {
                return RequestResultFactory.Error<PublicCovidTestResponse>(UnauthorizedResultError());
            }

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> result =
                await this.laboratoryDelegate.GetPublicTestResults(accessToken, phn, DateOnly.FromDateTime(dateOfBirth), DateOnly.FromDateTime(collectionDate)).ConfigureAwait(true);

            PhsaLoadState? loadState = result.ResourcePayload?.LoadState;
            if (loadState != null && loadState.RefreshInProgress)
            {
                return RequestResultFactory.ActionRequired(
                    new PublicCovidTestResponse
                    {
                        RetryIn = Math.Max(loadState.BackOffMilliseconds, this.labConfig.BackOffMilliseconds),
                        Loaded = !loadState.RefreshInProgress,
                    },
                    ActionType.Refresh,
                    "Refresh in progress");
            }

            LabIndicatorType labIndicatorType = Enum.Parse<LabIndicatorType>(result.ResourcePayload?.Result.FirstOrDefault()?.StatusIndicator);

            IEnumerable<PublicCovidTestRecord>? records = this.autoMapper.Map<IEnumerable<PublicCovidTestRecord>>(result.ResourcePayload?.Result ?? Array.Empty<CovidTestResult>());
            RequestResult<PublicCovidTestResponse> retVal = labIndicatorType switch
            {
                LabIndicatorType.Found => RequestResultFactory.Success(CreatePublicCovidTestRecordPayload(records, true), result.TotalResultCount, result.PageIndex, result.PageSize),
                LabIndicatorType.DataMismatch or LabIndicatorType.NotFound => RequestResultFactory.ActionRequired(
                    CreatePublicCovidTestRecordPayload(),
                    ActionType.DataMismatch,
                    ErrorMessages.DataMismatch),
                LabIndicatorType.Threshold or LabIndicatorType.Blocked => RequestResultFactory.ActionRequired(
                    CreatePublicCovidTestRecordPayload(),
                    ActionType.Invalid,
                    ErrorMessages.RecordsNotAvailable),
                _ => RequestResultFactory.Error<PublicCovidTestResponse>(result.ResultError),
            };

            return retVal;
        }

        private static PublicCovidTestResponse CreatePublicCovidTestRecordPayload(IEnumerable<PublicCovidTestRecord>? records = null, bool loaded = false, int retryIn = 0)
        {
            return new()
            {
                Records = records ?? Array.Empty<PublicCovidTestRecord>(),
                Loaded = loaded,
                RetryIn = retryIn,
            };
        }

        private static RequestResultError UnauthorizedResultError()
        {
            return new()
            {
                ResultMessage = "Error authenticating with KeyCloak",
                ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
            };
        }
    }
}
