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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
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
        private const string LabConfigSectionKey = "Laboratory";
        private readonly ILaboratoryApi laboratoryApi;
        private readonly LaboratoryConfig labConfig;

        private readonly ILogger logger;

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
            this.labConfig = new LaboratoryConfig();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestLaboratoryDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<List<PhsaCovid19Order>>>> GetCovid19Orders(string bearerToken, string hdid, int pageIndex = 0)
        {
            using (Source.StartActivity())
            {
                this.logger.LogDebug("Getting covid19 orders...");

                RequestResult<PhsaResult<List<PhsaCovid19Order>>> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                    PageIndex = pageIndex,
                    PageSize = int.Parse(this.labConfig.FetchSize, CultureInfo.InvariantCulture),
                };

                Dictionary<string, string?> query = new()
                {
                    ["limit"] = this.labConfig.FetchSize,
                    ["subjectHdid"] = hdid,
                };

                try
                {
                    PhsaResult<List<PhsaCovid19Order>> response =
                        await this.laboratoryApi.GetCovid19OrdersAsync(query, bearerToken).ConfigureAwait(true);
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = response;
                    retVal.TotalResultCount = retVal.ResourcePayload.Result!.Count;
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    if (e is ApiException { StatusCode: HttpStatusCode.NoContent })
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = new() { Result = new() };
                        retVal.TotalResultCount = 0;
                    }
                    else
                    {
                        this.logger.LogError("Error while retrieving Covid19 Orders... {Error}", e);
                        HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                        retVal.ResultError = new()
                        {
                            ResultMessage = $"Status: {statusCode}. Error while retrieving Covid19 Orders",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                }

                this.logger.LogDebug("Finished getting Covid19 Orders");
                return retVal;
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(string id, string hdid, string bearerToken, bool isCovid19)
        {
            using (Source.StartActivity())
            {
                this.logger.LogTrace("Getting laboratory report... {Id}", id);

                RequestResult<LaboratoryReport> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                    TotalResultCount = 0,
                };

                Dictionary<string, string?> query = new()
                {
                    ["subjectHdid"] = hdid,
                };

                try
                {
                    if (isCovid19)
                    {
                        retVal.ResourcePayload = await this.laboratoryApi.GetLaboratoryReportAsync(id, query, bearerToken).ConfigureAwait(true);
                    }
                    else
                    {
                        retVal.ResourcePayload = await this.laboratoryApi.GetPlisLaboratoryReportAsync(id, query, bearerToken).ConfigureAwait(true);
                    }

                    retVal.ResultStatus = ResultType.Success;
                    retVal.TotalResultCount = 1;
                }
                catch (Exception e) when (e is ApiException or HttpRequestException)
                {
                    this.logger.LogError("Error retrieving Laboratory Report...{Error}", e);
                    HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                    retVal.ResultError = new()
                    {
                        ResultMessage = statusCode == HttpStatusCode.NoContent ? "Laboratory Report not found" : $"Status: {statusCode}. Error retrieving Laboratory Report",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                }

                this.logger.LogDebug("Finished getting Laboratory Report");
                return retVal;
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<PhsaLaboratorySummary>>> GetLaboratorySummary(string hdid, string bearerToken)
        {
            using Activity? activity = Source.StartActivity();

            this.logger.LogDebug("Getting laboratory summary...");

            RequestResult<PhsaResult<PhsaLaboratorySummary>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = int.Parse(this.labConfig.FetchSize, CultureInfo.InvariantCulture),
            };

            Dictionary<string, string?> query = new()
            {
                ["subjectHdid"] = hdid,
            };

            try
            {
                PhsaResult<PhsaLaboratorySummary> response =
                    await this.laboratoryApi.GetPlisLaboratorySummaryAsync(query, bearerToken).ConfigureAwait(true);
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = response;
                retVal.TotalResultCount = retVal.ResourcePayload.Result!.LabOrderCount;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                if (e is ApiException { StatusCode: HttpStatusCode.NoContent })
                {
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = new() { Result = new() };
                    retVal.TotalResultCount = 0;
                }
                else
                {
                    this.logger.LogError("Error while retrieving Laboratory Summary... {Error}", e);
                    HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                    retVal.ResultError = new()
                    {
                        ResultMessage = $"Status: {statusCode}. Error while retrieving Laboratory Summary",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                }
            }

            this.logger.LogDebug("Finished getting laboratory summary");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<IEnumerable<CovidTestResult>>>> GetPublicTestResults(string accessToken, string phn, DateOnly dateOfBirth, DateOnly collectionDate)
        {
            using Activity? activity = Source.StartActivity();

            this.logger.LogDebug("Getting public COVID-19 Test Results");

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            Dictionary<string, string?> query = new()
            {
                ["phn"] = phn,
                ["dateOfBirth"] = dateOfBirth.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture),
                ["collectionDate"] = collectionDate.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture),
            };

            try
            {
                PhsaResult<IEnumerable<CovidTestResult>> response =
                    await this.laboratoryApi.GetPublicCovidLabSummaryAsync(query, accessToken).ConfigureAwait(true);
                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = response;
                retVal.TotalResultCount = retVal.ResourcePayload.Result!.Count();
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogError("Error while retrieving Covid19 Test Results... {Error}", e);
                HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;

                retVal.ResultError = new()
                {
                    ResultMessage = $"Status: {statusCode}. Error while retrieving Covid19 Test Results",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting public COVID-19 Test results");
            return retVal;
        }
    }
}
