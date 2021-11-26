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
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.Http;
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
        private readonly IHttpClientService httpClientService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly LaboratoryConfig labConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestLaboratoryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestLaboratoryDelegate(
            ILogger<RestLaboratoryDelegate> logger,
            IHttpClientService httpClientService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.httpContextAccessor = httpContextAccessor;
            this.labConfig = new LaboratoryConfig();
            configuration.Bind(LabConfigSectionKey, this.labConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestLaboratoryDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryOrder>>> GetLaboratoryOrders(string bearerToken, string hdid, int pageIndex = 0)
        {
            using (Source.StartActivity("GetLaboratoryOrders"))
            {
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
                Dictionary<string, string?>? query = new Dictionary<string, string?>
                {
                    ["limit"] = this.labConfig.FetchSize,
                    ["subjectHdid"] = hdid,
                };
                try
                {
                    string endpointString = $"{this.labConfig.BaseUrl}{this.labConfig.LabOrdersEndpoint}";
                    Uri endpoint = new(QueryHelpers.AddQueryString(endpointString, query));
                    HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    this.logger.LogTrace($"Response: {response}");
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            this.logger.LogTrace($"Response payload: {payload}");
                            PHSAResult<List<LaboratoryOrder>>? phsaResult = JsonSerializer.Deserialize<PHSAResult<List<LaboratoryOrder>>>(payload);
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
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, string bearerToken)
        {
            using (Source.StartActivity("GetLaboratoryOrders"))
            {
                RequestResult<LaboratoryReport> retVal = new RequestResult<LaboratoryReport>()
                {
                    ResultStatus = Common.Constants.ResultType.Error,
                };

                this.logger.LogTrace($"Getting laboratory report... {id}");
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                Dictionary<string, string?>? query = new Dictionary<string, string?>
                {
                    ["subjectHdid"] = hdid,
                };
                try
                {
                    string endpointString = $"{this.labConfig.BaseUrl}{this.labConfig.LabOrdersEndpoint}/{id}/LabReportDocument";
                    Uri endpoint = new(QueryHelpers.AddQueryString(endpointString, query));
                    HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    this.logger.LogTrace($"Get laboratory report response payload: {payload}");
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            JsonSerializerOptions? options = new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                WriteIndented = true,
                            };
                            LaboratoryReport? report = JsonSerializer.Deserialize<LaboratoryReport>(payload, options);
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
                        case HttpStatusCode.NoContent: // No report found.
                            retVal.ResultError = new RequestResultError() { ResultMessage = "Report not found.", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
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

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public async Task<RequestResult<PHSAResult<IEnumerable<PublicCovidTestResult>>>> GetPublicTestResults(string accessToken, string phn, DateOnly dateOfBirth, DateOnly collectionDate)
        {
            using Activity? activity = Source.StartActivity("GetPublicTestResults");
            this.logger.LogDebug($"Getting public COVID-19 test results {phn} {dateOfBirth} {collectionDate}...");

            HttpContent? content = null;
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            RequestResult<PHSAResult<IEnumerable<PublicCovidTestResult>>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            try
            {
                JsonSerializerOptions serializerOptions = new()
                {
                    Converters = { new DateOnlyJsonConverter() },
                };
                string json = JsonSerializer.Serialize(new { phn, dateOfBirth, collectionDate }, serializerOptions);
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();

                string endpointString = $"{this.labConfig.BaseUrl}{this.labConfig.PublicCovidTestsEndPoint}";
                Uri endpoint = new(endpointString);
                content = new StringContent(json, null, MediaTypeNames.Application.Json);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                client.DefaultRequestHeaders.Add("X-Forward-For", ipAddress);

                HttpResponseMessage response = await client.PostAsync(endpoint, content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Response: {response}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        this.logger.LogTrace($"Response payload: {payload}");
                        PHSAResult<IEnumerable<PublicCovidTestResult>>? phsaResult = JsonSerializer.Deserialize<PHSAResult<IEnumerable<PublicCovidTestResult>>>(payload);
                        if (phsaResult != null && phsaResult.Result != null)
                        {
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = phsaResult;
                            retVal.TotalResultCount = phsaResult.Result.Count();
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError()
                            {
                                ResultMessage = "Error with JSON data",
                                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                            };
                        }

                        break;
                    case HttpStatusCode.Forbidden:
                        retVal.ResultError = new RequestResultError()
                        {
                            ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError()
                        {
                            ResultMessage = $"Unable to connect to Laboratory PublicCovidTestsEndPoint, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        this.logger.LogError($"Unable to connect to endpoint {endpointString}, HTTP Error {response.StatusCode}\n{payload}");
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting public COVID-19 test results: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception retrieving public COVID-19 test results {e}");
            }
            finally
            {
                content?.Dispose();
            }

            this.logger.LogDebug($"Finished getting public COVID-19 test results {phn} {dateOfBirth} {collectionDate}...");
            return retVal;
        }
    }
}
