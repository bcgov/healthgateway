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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        /// <summary>
        /// The configuration section name for Laboratory.
        /// </summary>
        public const string LabConfigSectionKey = "Laboratory";

        private const string IsNullOrEmptyTokenErrorMessage = "The auth token is null or empty - unable to cache or proceed";

        private readonly ILaboratoryDelegate laboratoryDelegate;
        private readonly ILogger<LaboratoryService> logger;
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly LaboratoryConfig labConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        public LaboratoryService(
            IConfiguration configuration,
            ILogger<LaboratoryService> logger,
            ILaboratoryDelegateFactory laboratoryDelegateFactory,
            IAuthenticationDelegate authenticationDelegate)
        {
            this.logger = logger;
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
            this.authenticationDelegate = authenticationDelegate;

            this.labConfig = new();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Covid19OrderResult>> GetCovid19Orders(string hdid, int pageIndex = 0)
        {
            RequestResult<Covid19OrderResult> retVal = new()
            {
                ResourcePayload = new(),
                ResultStatus = ResultType.Error,
                ResultError = UnauthorizedResultError(),
            };

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            if (accessToken != null)
            {
                RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult = await this.laboratoryDelegate.GetCovid19Orders(accessToken, hdid, pageIndex).ConfigureAwait(true);

                retVal.ResultStatus = delegateResult.ResultStatus;
                retVal.ResultError = delegateResult.ResultError;
                retVal.PageIndex = delegateResult.PageIndex;
                retVal.PageSize = delegateResult.PageSize;
                retVal.TotalResultCount = delegateResult.TotalResultCount;

                IEnumerable<PhsaCovid19Order> payload =
                    delegateResult.ResourcePayload?.Result ?? Enumerable.Empty<PhsaCovid19Order>();
                if (delegateResult.ResultStatus == ResultType.Success)
                {
                    retVal.ResourcePayload.Covid19Orders = Covid19Order.FromPhsaModelCollection(payload);
                }

                PhsaLoadState? loadState = delegateResult.ResourcePayload?.LoadState;
                if (loadState != null)
                {
                    retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                    if (loadState.RefreshInProgress)
                    {
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired("Refresh in progress", ActionType.Refresh);
                        retVal.ResourcePayload.RetryIn = Math.Max(
                            loadState.BackOffMilliseconds,
                            this.labConfig.BackOffMilliseconds);
                    }
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryOrderResult>> GetLaboratoryOrders(string hdid)
        {
            RequestResult<LaboratoryOrderResult> retVal = new()
            {
                ResourcePayload = new(),
                ResultStatus = ResultType.Error,
                ResultError = UnauthorizedResultError(),
            };

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            if (accessToken != null)
            {
                RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult =
                    await this.laboratoryDelegate.GetLaboratorySummary(hdid, accessToken).ConfigureAwait(true);

                retVal.ResultStatus = delegateResult.ResultStatus;
                retVal.ResultError = delegateResult.ResultError;
                retVal.PageIndex = delegateResult.PageIndex;
                retVal.PageSize = delegateResult.PageSize;
                retVal.TotalResultCount = delegateResult.TotalResultCount;

                PhsaLaboratorySummary? payload = delegateResult.ResourcePayload?.Result;
                if (delegateResult.ResultStatus == ResultType.Success && payload != null)
                {
                    retVal.ResourcePayload.LaboratoryOrders =
                        LaboratoryOrder.FromPhsaModelCollection(payload.LabOrders);
                }

                PhsaLoadState? loadState = delegateResult.ResourcePayload?.LoadState;
                if (loadState != null)
                {
                    retVal.ResourcePayload.Queued = loadState.Queued;
                    retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                    if (loadState.RefreshInProgress)
                    {
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired("Refresh in progress", ActionType.Refresh);
                        retVal.ResourcePayload.RetryIn = Math.Max(
                            loadState.BackOffMilliseconds,
                            this.labConfig.BackOffMilliseconds);
                    }
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(string id, string hdid, bool isCovid19)
        {
            RequestResult<LaboratoryReport> retVal = new();

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            if (accessToken != null)
            {
                return await this.laboratoryDelegate.GetLabReport(id, hdid, accessToken, isCovid19)
                    .ConfigureAwait(true);
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
                retVal.ResultError = new RequestResultError
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
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error parsing collection date",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            if (!PhnValidator.IsValid(phn))
            {
                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = "Error parsing phn",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
                return retVal;
            }

            string? accessToken = this.authenticationDelegate.AccessTokenAsUser();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogCritical(IsNullOrEmptyTokenErrorMessage);
                retVal.ResultError = UnauthorizedResultError();
                return retVal;
            }

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> result = await this.laboratoryDelegate.GetPublicTestResults(accessToken, phn, dateOfBirth, collectionDate).ConfigureAwait(true);
            IEnumerable<CovidTestResult> payload = result.ResourcePayload?.Result ?? Enumerable.Empty<CovidTestResult>();
            PhsaLoadState? loadState = result.ResourcePayload?.LoadState;

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload.Any())
            {
                LabIndicatorType labIndicatorType = Enum.Parse<LabIndicatorType>(payload.Select(x => x.StatusIndicator).First());

                switch (labIndicatorType)
                {
                    case LabIndicatorType.Found:
                        retVal.ResourcePayload = new PublicCovidTestResponse(payload.Select(PublicCovidTestRecord.FromModel).ToList());
                        break;
                    case LabIndicatorType.DataMismatch:
                    case LabIndicatorType.NotFound:
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.DataMismatch, ActionType.DataMismatch);
                        break;
                    case LabIndicatorType.Threshold:
                    case LabIndicatorType.Blocked:
                        retVal.ResultStatus = ResultType.ActionRequired;
                        retVal.ResultError = ErrorTranslator.ActionRequired(ErrorMessages.RecordsNotAvailable, ActionType.Invalid);
                        break;
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
