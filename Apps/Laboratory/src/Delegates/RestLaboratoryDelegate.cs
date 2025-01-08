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
namespace HealthGateway.Laboratory.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve laboratory information.
    /// </summary>
    public class RestLaboratoryDelegate : ILaboratoryDelegate
    {
        private readonly ILaboratoryApi laboratoryApi;
        private readonly LaboratoryConfig labConfig;
        private readonly ILogger<RestLaboratoryDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestLaboratoryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="laboratoryApi">The client to use for laboratory api calls.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestLaboratoryDelegate(
            ILogger<RestLaboratoryDelegate> logger,
            ILaboratoryApi laboratoryApi,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.laboratoryApi = laboratoryApi;
            this.labConfig = configuration.GetSection(LaboratoryConfig.ConfigSectionKey).Get<LaboratoryConfig>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<List<PhsaCovid19Order>>>> GetCovid19OrdersAsync(string bearerToken, string hdid, int pageIndex = 0, CancellationToken ct = default)
        {
            RequestResult<PhsaResult<List<PhsaCovid19Order>>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = pageIndex,
                PageSize = int.Parse(this.labConfig.FetchSize, CultureInfo.InvariantCulture),
            };

            try
            {
                this.logger.LogDebug("Retrieving COVID-19 test results");
                PhsaResult<List<PhsaCovid19Order>> response = await this.laboratoryApi.GetCovid19OrdersAsync(hdid, this.labConfig.FetchSize, bearerToken, ct);

                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                retVal.ResourcePayload = response ?? new() { Result = [] };
                retVal.TotalResultCount = retVal.ResourcePayload.Result!.Count;
                retVal.ResultStatus = ResultType.Success;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving COVID-19 test results");
                HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                retVal.ResultError = new()
                {
                    ResultMessage = $"Status: {statusCode}. Error while retrieving Covid19 Orders",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReportAsync(string id, string hdid, string bearerToken, bool isCovid19, CancellationToken ct = default)
        {
            RequestResult<LaboratoryReport> retVal = new()
            {
                ResultStatus = ResultType.Error,
                TotalResultCount = 0,
            };

            try
            {
                this.logger.LogDebug("Retrieving {ReportType} laboratory report", isCovid19 ? "COVID-19" : string.Empty);

                retVal.ResourcePayload = isCovid19
                    ? await this.laboratoryApi.GetLaboratoryReportAsync(id, hdid, bearerToken, ct)
                    : await this.laboratoryApi.GetPlisLaboratoryReportAsync(id, hdid, bearerToken, ct);

                if (retVal.ResourcePayload == null)
                {
                    retVal.ResultError = new()
                    {
                        ResultMessage = "Status: 204. Error retrieving Laboratory Report.",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                }
                else
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving laboratory report");
                HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                retVal.ResultError = new()
                {
                    ResultMessage = $"Status: {statusCode}. Error retrieving Laboratory Report",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<PhsaLaboratorySummary>>> GetLaboratorySummaryAsync(string hdid, string bearerToken, CancellationToken ct = default)
        {
            RequestResult<PhsaResult<PhsaLaboratorySummary>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = int.Parse(this.labConfig.FetchSize, CultureInfo.InvariantCulture),
            };

            try
            {
                this.logger.LogDebug("Retrieving laboratory orders");
                PhsaResult<PhsaLaboratorySummary> response = await this.laboratoryApi.GetPlisLaboratorySummaryAsync(hdid, bearerToken, ct);

                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                retVal.ResourcePayload = response ?? new() { Result = new() };
                retVal.TotalResultCount = retVal.ResourcePayload.Result!.LabOrderCount;
                retVal.ResultStatus = ResultType.Success;
                return retVal;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving laboratory orders");
                HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                retVal.ResultError = new()
                {
                    ResultMessage = $"Status: {statusCode}. Error while retrieving Laboratory Summary",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }
    }
}
