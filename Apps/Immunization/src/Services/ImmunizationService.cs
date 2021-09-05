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
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Delegates.PHSA;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Models.PHSA;
    using HealthGateway.Immunization.Parser;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private const string CovidDisease = "COVID19";
        private const string PHSAConfigSectionKey = "PHSA";
        private readonly Delegates.IImmunizationDelegate immunizationDelegate;
        private readonly IReportDelegate reportDelegate;
        private readonly IVaccineStatusDelegate vaccineDelegate;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly PHSAConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="immunizationDelegate">The factory to create immunization delegates.</param>
        /// <param name="reportDelegate">The injected report delegate.</param>
        /// <param name="vaccineDelegate">The injected vaccine status delegate.</param>
        /// <param name="httpContextAccessor">The injected http context accessor.</param>
        public ImmunizationService(
            IConfiguration configuration,
            Delegates.IImmunizationDelegate immunizationDelegate,
            IReportDelegate reportDelegate,
            IVaccineStatusDelegate vaccineDelegate,
            IHttpContextAccessor httpContextAccessor)
        {
            this.immunizationDelegate = immunizationDelegate;
            this.reportDelegate = reportDelegate;
            this.vaccineDelegate = vaccineDelegate;
            this.httpContextAccessor = httpContextAccessor;

            this.phsaConfig = new PHSAConfig();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineStatus>> GetCovidVaccineStatus(string hdid)
        {
            RequestResult<VaccineStatus> retVal = new ()
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
            }

            if (result.ResourcePayload != null)
            {
                retVal.ResourcePayload.Loaded = !result.ResourcePayload.LoadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(result.ResourcePayload.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<CovidVaccineRecord>> GetCovidVaccineRecord(string hdid)
        {
            RequestResult<CovidVaccineRecord> retVal = new ()
            {
                ResultStatus = ResultType.Error,
            };

            // Gets the current user access token and pass it along to PHSA
            string? bearerToken = await this.httpContextAccessor.HttpContext!.GetTokenAsync("access_token").ConfigureAwait(true);

            RequestResult<PHSAResult<ImmunizationCard>> recordCardResult = await this.immunizationDelegate.GetVaccineHistory(hdid, CovidDisease).ConfigureAwait(true);
            RequestResult<PHSAResult<VaccineStatusResult>> statusResult = await this.vaccineDelegate.GetVaccineStatus(new VaccineStatusQuery() { HdId = hdid }, bearerToken, false).ConfigureAwait(true);
            PHSALoadState? loadState = recordCardResult.ResourcePayload?.LoadState;
            ImmunizationCard? recordCardPayload = recordCardResult.ResourcePayload?.Result;
            VaccineStatusResult? vaccineStatusResult = statusResult.ResourcePayload?.Result;
            if ((recordCardResult.ResultStatus == ResultType.Success && recordCardPayload != null) &&
                (statusResult.ResultStatus == ResultType.Success && vaccineStatusResult != null))
            {
                RequestResult<ReportModel> reportResult = this.reportDelegate.GetVaccineStatusAndRecordPDF(
                    VaccineStatus.FromModel(vaccineStatusResult, null),
                    null,
                    recordCardPayload.PaperRecord.Data);
                retVal.ResourcePayload = new ()
                {
                    Document = new EncodedMedia()
                    {
                        Data = reportResult.ResourcePayload!.Data,
                        Encoding = "base64",
                        Type = MediaTypeNames.Application.Pdf,
                    },
                    QRCode = recordCardPayload.QRCode,
                };
                retVal.ResultStatus = ResultType.Success;
            }
            else
            {
                retVal.ResultStatus = ResultType.Error;
                retVal.ResultError = recordCardResult.ResultError ?? statusResult.ResultError;
                retVal.ResourcePayload = new CovidVaccineRecord();
            }

            if (loadState != null)
            {
                retVal.ResourcePayload.Loaded = !loadState.RefreshInProgress;
                retVal.ResourcePayload.RetryIn = Math.Max(loadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds);
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
    }
}
