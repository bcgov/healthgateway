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
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Parser;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private readonly ILogger<ImmunizationService> logger;
        private readonly Delegates.IImmunizationDelegate immunizationDelegate;
        private readonly IVaccineProofService vpService;
        private readonly IVaccineStatusDelegate vaccineDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly PHSAConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="logger">The injected logger to use.</param>
        /// <param name="immunizationDelegate">The factory to create immunization delegates.</param>
        /// <param name="vaccineProofService">The vaccine proof service to get the vaccine proof report.</param>
        /// <param name="vaccineDelegate">The injected vaccine status delegate.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        public ImmunizationService(
            IConfiguration configuration,
            ILogger<ImmunizationService> logger,
            Delegates.IImmunizationDelegate immunizationDelegate,
            IVaccineProofService vaccineProofService,
            IVaccineStatusDelegate vaccineDelegate,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.immunizationDelegate = immunizationDelegate;
            this.vaccineDelegate = vaccineDelegate;
            this.vpService = vaccineProofService;
            this.httpContextAccessor = httpContextAccessor;

            this.phsaConfig = new();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetCovidVaccineStatus(string hdid)
        {
            RequestResult<VaccineStatus> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            RequestResult<PHSAResult<VaccineStatusResult>> result = await this.vaccineDelegate.GetVaccineStatus(new VaccineStatusQuery() { HdId = hdid }, bearerToken, false).ConfigureAwait(true);
            VaccineStatusResult? payload = result.ResourcePayload?.Result;
            retVal.ResultStatus = result.ResultStatus;
            retVal.ResultError = result.ResultError;

            if (payload == null)
            {
                retVal.ResourcePayload = new VaccineStatus();
                retVal.ResourcePayload.State = VaccineState.NotFound;
            }
            else
            {
                retVal.ResourcePayload = VaccineStatus.FromModel(payload);
                retVal.ResourcePayload.State = retVal.ResourcePayload.State switch
                {
                    var state when
                        state == VaccineState.DataMismatch ||
                        state == VaccineState.Threshold ||
                        state == VaccineState.Blocked => VaccineState.NotFound,
                    _ => retVal.ResourcePayload.State,
                };
            }

            if (result.ResourcePayload != null)
            {
                retVal.ResourcePayload.Loaded = !result.ResourcePayload.LoadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(result.ResourcePayload.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidVaccineRecord>> GetCovidVaccineRecord(string hdid, VaccineProofTemplate proofTemplate)
        {
            RequestResult<CovidVaccineRecord> retVal;

            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);
            RequestResult<PHSAResult<VaccineStatusResult>> statusResult = await this.vaccineDelegate.GetVaccineStatus(new VaccineStatusQuery() { HdId = hdid }, bearerToken, false).ConfigureAwait(true);
            PHSALoadState? loadState = statusResult.ResourcePayload?.LoadState;
            VaccineStatusResult? vaccineStatusResult = statusResult.ResourcePayload?.Result;
            if (statusResult.ResultStatus == ResultType.Success && vaccineStatusResult != null && loadState != null && !loadState.RefreshInProgress)
            {
                retVal = await this.GetVaccineProof(hdid, vaccineStatusResult, proofTemplate).ConfigureAwait(true);
            }
            else
            {
                retVal = new();
                if (loadState != null)
                {
                    retVal.ResourcePayload = new CovidVaccineRecord();
                    retVal.ResultStatus = ResultType.ActionRequired;
                    retVal.ResultError = ErrorTranslator.ActionRequired("Vaccine status refresh in progress", ActionType.Refresh);
                    retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                    retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
                }
                else
                {
                    retVal.ResultError = statusResult.ResultError;
                }
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunization(string immunizationId)
        {
            RequestResult<PHSAResult<ImmunizationViewResponse>> delegateResult = await this.immunizationDelegate.GetImmunization(immunizationId).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = EventParser.FromPHSAModel(delegateResult.ResourcePayload!.Result),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationResult>> GetImmunizations(int pageIndex = 0)
        {
            RequestResult<PHSAResult<ImmunizationResponse>> delegateResult = await this.immunizationDelegate.GetImmunizations(pageIndex).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = new ImmunizationResult(
                        LoadStateModel.FromPHSAModel(delegateResult.ResourcePayload!.LoadState),
                        EventParser.FromPHSAModelList(delegateResult.ResourcePayload!.Result!.ImmunizationViews),
                        ImmunizationRecommendation.FromPHSAModelList(delegateResult.ResourcePayload.Result.Recommendations)),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        private async Task<RequestResult<CovidVaccineRecord>> GetVaccineProof(string hdid, VaccineStatusResult vaccineStatusResult, VaccineProofTemplate proofTemplate)
        {
            RequestResult<CovidVaccineRecord> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };
            VaccineState state = Enum.Parse<VaccineState>(vaccineStatusResult.StatusIndicator);
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
                        SmartHealthCardQr = vaccineStatusResult.QRCode.Data!,
                    };
                    RequestResult<ReportModel> assetResult = await this.vpService.GetVaccineProof(hdid, request, proofTemplate).ConfigureAwait(true);
                    if (assetResult.ResultStatus == ResultType.Success && assetResult.ResourcePayload != null)
                    {
                        EncodedMedia document = new()
                        {
                            Data = assetResult.ResourcePayload.Data,
                            Encoding = "base64",
                            Type = "application/pdf",
                        };
                        retVal.ResourcePayload = new CovidVaccineRecord()
                        {
                            Document = document,
                            Loaded = true,
                            QRCode = vaccineStatusResult.QRCode,
                        };
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
