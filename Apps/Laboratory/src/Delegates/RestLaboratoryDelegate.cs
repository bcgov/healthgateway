﻿//-------------------------------------------------------------------------
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
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Instrumentation;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve laboratory information.
    /// </summary>
    public class RestLaboratoryDelegate : ILaboratoryDelegate
    {
        private const string LabConfigSectionKey = "Laboratory";

        private readonly ILogger logger;
        private readonly ITraceService traceService;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;
        private readonly LaboratoryConfig labConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestLaboratoryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="traceService">Injected TraceService Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestLaboratoryDelegate(
            ILogger<RestLaboratoryDelegate> logger,
            ITraceService traceService,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.traceService = traceService;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
            this.labConfig = new LaboratoryConfig();
            this.configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryOrder>>> GetLaboratoryOrders(string bearerToken, int pageIndex = 0)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            RequestResult<IEnumerable<LaboratoryOrder>> retVal = new RequestResult<IEnumerable<LaboratoryOrder>>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
                PageIndex = pageIndex,
            };

            this.logger.LogDebug($"Getting laboratory orders...");
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
                this.logger.LogTrace($"Response: {response}");
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            IgnoreNullValues = true,
                            WriteIndented = true,
                        };
                        this.logger.LogTrace($"Response payload: {payload}");
                        PHSAResult<LaboratoryOrder> phsaResult = JsonSerializer.Deserialize<PHSAResult<LaboratoryOrder>>(payload, options);
                        if (phsaResult != null && phsaResult.Result != null)
                        {
                            retVal.ResultStatus = Common.Constants.ResultType.Success;
                            retVal.ResourcePayload = phsaResult.Result;
                            retVal.TotalResultCount = phsaResult.Result.Count;
#pragma warning disable CA1305 // Specify IFormatProvider
                            retVal.PageSize = int.Parse(this.labConfig.FetchSize);
#pragma warning restore CA1305 // Specify IFormatProvider
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
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
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Labs Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Lab Orders: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception in Get Lab Orders {e}");
            }

            this.logger.LogDebug($"Finished getting Laboratory Orders");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string bearerToken)
        {
            using ITracer tracer = this.traceService.TraceMethod(this.GetType().Name);
            RequestResult<LaboratoryReport> retVal = new RequestResult<LaboratoryReport>()
            {
                ResultStatus = Common.Constants.ResultType.Error,
            };

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
                            retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        }

                        break;
                    case HttpStatusCode.NoContent: // No Lab exits for this user
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.PageIndex = 0;
                        retVal.TotalResultCount = 0;
                        retVal.ResourcePayload = new LaboratoryReport();
                        break;
                    case HttpStatusCode.Forbidden:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Labs Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Lab Report: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception in Lab Report {e}");
            }

            this.logger.LogDebug($"Finished getting Laboratory Report");
            return retVal;
        }
    }
}
