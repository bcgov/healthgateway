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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Implementation that uses HTTP to retrieve laboratory information.
    /// </summary>
    public class RestLaboratoryDelegate : ILaboratoryDelegate
    {
        private const string LabConfigSectionKey = "Laboratory";

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;
        private readonly LaboratoryConfig labConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestLaboratoryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestLaboratoryDelegate(
            ILogger<RestLaboratoryDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
            this.labConfig = new LaboratoryConfig();
            this.configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryOrder>>> GetLaboratoryOrders(string bearerToken, int pageIndex = 0)
        {
            RequestResult<IEnumerable<LaboratoryOrder>> retVal = new RequestResult<IEnumerable<LaboratoryOrder>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                PageIndex = pageIndex,
            };
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting laboratory orders...");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            var query = new Dictionary<string, string>
            {
                ["limit"] = this.labConfig.FetchSize,
            };
            try
            {
                Uri endpoint = new Uri(QueryHelpers.AddQueryString(this.labConfig.Endpoint, query));
                HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            IgnoreNullValues = true,
                            WriteIndented = true,
                        };
                        IEnumerable<LaboratoryOrder> labReports = JsonSerializer.Deserialize<List<LaboratoryOrder>>(payload, options);
                        if (labReports != null)
                        {
                            retVal.ResultStatus = Common.Constants.ResultType.Success;
                            retVal.ResourcePayload = labReports;
                            retVal.TotalResultCount = labReports.Count();
                            #pragma warning disable CA1305 // Specify IFormatProvider
                            retVal.PageSize = int.Parse(this.labConfig.FetchSize);
                            #pragma warning restore CA1305 // Specify IFormatProvider
                        }
                        else
                        {
                            retVal.ResultMessage = "Error with JSON data";
                        }

                        break;
                    case HttpStatusCode.NoContent: // No Lab exits for this user
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = new List<LaboratoryOrder>();
                        retVal.TotalResultCount = 0;
                        #pragma warning disable CA1305 // Specify IFormatProvider
                        retVal.PageSize = int.Parse(this.labConfig.FetchSize);
                        #pragma warning restore CA1305 // Specify IFormatProvider
                        break;
                    case HttpStatusCode.Forbidden:
                        retVal.ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}";
                        break;
                    default:
                        retVal.ResultMessage = $"Unable to connect to Labs Endpoint, HTTP Error {response.StatusCode}";
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            #pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultMessage = $"Exception getting Lab Orders: {e}";
                this.logger.LogError($"Unexpected exception in Get Lab Orders {e}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Laboratory Orders, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string bearerToken)
        {
            RequestResult<LaboratoryReport> retVal = new RequestResult<LaboratoryReport>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting laboratory report...");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            try
            {
                Uri endpoint = new Uri($"{this.labConfig.Endpoint}/{id}/LabReportDocument");
                HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            IgnoreNullValues = true,
                            WriteIndented = true,
                        };
                        LaboratoryReport report = JsonSerializer.Deserialize<LaboratoryReport>(payload, options);
                        if (report != null)
                        {
                            retVal.ResultStatus = Common.Constants.ResultType.Success;
                            retVal.ResourcePayload = report;
                            retVal.TotalResultCount = 1;
                        }
                        else
                        {
                            retVal.ResultMessage = "Error with JSON data";
                        }

                        break;
                    case HttpStatusCode.NoContent: // No Lab exits for this user
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.ResultMessage = $"No Lab Report exists for id: {id}";
                        retVal.PageIndex = 0;
                        retVal.TotalResultCount = 0;
                        retVal.ResourcePayload = new LaboratoryReport();
                        break;
                    case HttpStatusCode.Forbidden:
                        retVal.ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}";
                        break;
                    default:
                        retVal.ResultMessage = $"Unable to connect to Labs Endpoint, HTTP Error {response.StatusCode}";
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultMessage = $"Exception getting Lab Report: {e}";
                this.logger.LogError($"Unexpected exception in Lab Report {e}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Laboratory Report, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }
    }
}
