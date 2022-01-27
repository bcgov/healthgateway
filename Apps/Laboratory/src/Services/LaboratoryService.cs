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
    using HealthGateway.Laboratory.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        /// <summary>
        /// The configuration section name for Laboratory.
        /// </summary>
        public const string LabConfigSectionKey = "Laboratory";

        private readonly ILaboratoryDelegate laboratoryDelegate;
        private readonly ITokenCacheService tokenCacheService;
        private readonly ILogger<LaboratoryService> logger;
        private readonly LaboratoryConfig labConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        /// <param name="tokenCacheService">The cache to use to reduce lookups.</param>
        public LaboratoryService(
            IConfiguration configuration,
            ILogger<LaboratoryService> logger,
            ILaboratoryDelegateFactory laboratoryDelegateFactory,
            ITokenCacheService tokenCacheService)
        {
            this.logger = logger;
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
            this.tokenCacheService = tokenCacheService;

            this.labConfig = new();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryModel>>> GetLaboratoryOrders(string bearerToken, string hdid, int pageIndex = 0)
        {
            RequestResult<IEnumerable<LaboratoryOrder>> delegateResult = await this.laboratoryDelegate.GetLaboratoryOrders(bearerToken, hdid, pageIndex).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<IEnumerable<LaboratoryModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = LaboratoryModel.FromPHSAModelList(delegateResult.ResourcePayload),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<IEnumerable<LaboratoryModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, string bearerToken)
        {
            return await this.laboratoryDelegate.GetLabReport(id, hdid, bearerToken).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<AuthenticatedRapidTestResponse>> CreateRapidTestAsync(string hdid, string bearerToken, AuthenticatedRapidTestRequest rapidTestRequest)
        {
            RequestResult<AuthenticatedRapidTestResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
                ResourcePayload = new AuthenticatedRapidTestResponse(),
            };

            RequestResult<RapidTestResponse> result = await this.laboratoryDelegate.SubmitRapidTestAsync(hdid, bearerToken, rapidTestRequest).ConfigureAwait(true);
            RapidTestResponse payload = result.ResourcePayload ?? new RapidTestResponse();

            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;
            retVal.ResourcePayload = new AuthenticatedRapidTestResponse() { Phn = payload.Phn, Records = PhsaModelParser.FromPhsaModelList(payload.RapidTestResults) };

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

            string? accessToken = this.tokenCacheService.RetrieveAccessToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogCritical("The auth token is null or empty - unable to cache or proceed");
                retVal.ResultError = new()
                {
                    ResultMessage = "Error authenticating with KeyCloak",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
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
    }
}
