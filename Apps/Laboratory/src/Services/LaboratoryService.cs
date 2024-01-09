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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ErrorHandling;
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

        private const string IsNullOrEmptyTokenErrorMessage = "The auth token is null or empty - unable to cache or proceed";

        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly ILaboratoryDelegate laboratoryDelegate;
        private readonly ILogger<LaboratoryService> logger;
        private readonly IMapper autoMapper;
        private readonly LaboratoryConfig labConfig;
        private readonly IPatientRepository patientRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="logger">The injected logger.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="patientRepository">The injected patient repository provider.</param>
        /// <param name="autoMapper">The injected automapper.</param>
        public LaboratoryService(
            IConfiguration configuration,
            ILogger<LaboratoryService> logger,
            ILaboratoryDelegateFactory laboratoryDelegateFactory,
            IAuthenticationDelegate authenticationDelegate,
            IPatientRepository patientRepository,
            IMapper autoMapper)
        {
            this.logger = logger;
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
            this.authenticationDelegate = authenticationDelegate;
            this.patientRepository = patientRepository;
            this.autoMapper = autoMapper;

            this.labConfig = new();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Covid19OrderResult>> GetCovid19OrdersAsync(string hdid, int pageIndex = 0, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.Covid19TestResult, ct))
            {
                return new RequestResult<Covid19OrderResult>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new Covid19OrderResult(),
                    TotalResultCount = 0,
                };
            }

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken == null)
            {
                this.logger.LogCritical(IsNullOrEmptyTokenErrorMessage);
                return RequestResultFactory.Error<Covid19OrderResult>(UnauthorizedResultError());
            }

            RequestResult<PhsaResult<List<PhsaCovid19Order>>> delegateResult = await this.laboratoryDelegate.GetCovid19OrdersAsync(accessToken, hdid, pageIndex, ct);

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
                    Loaded = true,
                },
                delegateResult.TotalResultCount,
                delegateResult.PageIndex,
                delegateResult.PageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryOrderResult>> GetLaboratoryOrdersAsync(string hdid, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.LabResult, ct))
            {
                return new RequestResult<LaboratoryOrderResult>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new LaboratoryOrderResult
                    {
                        Loaded = true,
                        Queued = false,
                        LaboratoryOrders = Enumerable.Empty<LaboratoryOrder>(),
                    },
                };
            }

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogCritical(IsNullOrEmptyTokenErrorMessage);
                return RequestResultFactory.Error<LaboratoryOrderResult>(UnauthorizedResultError());
            }

            RequestResult<PhsaResult<PhsaLaboratorySummary>> delegateResult = await this.laboratoryDelegate.GetLaboratorySummaryAsync(hdid, accessToken, ct);

            PhsaLoadState? loadState = delegateResult.ResourcePayload?.LoadState;
            if (loadState != null && loadState.RefreshInProgress)
            {
                return RequestResultFactory.ActionRequired(
                    new LaboratoryOrderResult
                    {
                        RetryIn = Math.Max(loadState.BackOffMilliseconds, this.labConfig.BackOffMilliseconds),
                        Loaded = !loadState.RefreshInProgress,
                        Queued = loadState.Queued,
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
                    Loaded = !(loadState?.RefreshInProgress ?? false),
                    Queued = loadState?.Queued ?? false,
                },
                delegateResult.TotalResultCount,
                delegateResult.PageIndex,
                delegateResult.PageSize);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReportAsync(string id, string hdid, bool isCovid19, CancellationToken ct = default)
        {
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (string.IsNullOrEmpty(accessToken))
            {
                this.logger.LogCritical(IsNullOrEmptyTokenErrorMessage);
                return RequestResultFactory.Error<LaboratoryReport>(UnauthorizedResultError());
            }

            return await this.laboratoryDelegate.GetLabReportAsync(id, hdid, accessToken, isCovid19, ct);
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
