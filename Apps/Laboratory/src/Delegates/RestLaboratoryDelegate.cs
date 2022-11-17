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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Laboratory.Api;
    using HealthGateway.Laboratory.Models;
    using HealthGateway.Laboratory.Models.PHSA;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve laboratory information.
    /// </summary>
    public class RestLaboratoryDelegate : ILaboratoryDelegate
    {
        private const string LabConfigSectionKey = "Laboratory";
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IHttpClientService httpClientService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILaboratoryApi laboratoryApi;
        private readonly LaboratoryConfig labConfig;

        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestLaboratoryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        /// <param name="laboratoryApi">The client to use for laboratory api calls.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestLaboratoryDelegate(
            ILogger<RestLaboratoryDelegate> logger,
            IHttpClientService httpClientService,
            IHttpContextAccessor httpContextAccessor,
            ILaboratoryApi laboratoryApi,
            IAuthenticationDelegate authenticationDelegate,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.httpContextAccessor = httpContextAccessor;
            this.laboratoryApi = laboratoryApi;
            this.authenticationDelegate = authenticationDelegate;
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

                string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
                if (accessToken != null)
                {
                    try
                    {
                        PhsaResult<List<PhsaCovid19Order>> response =
                            await this.laboratoryApi.GetCovid19OrdersAsync(query, accessToken).ConfigureAwait(true);
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = response;
                        retVal.TotalResultCount = retVal.ResourcePayload.Result!.Count;
                    }
                    catch (ApiException e)
                    {
                        if (e.StatusCode == HttpStatusCode.NoContent)
                        {
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = new() { Result = new() };
                            retVal.TotalResultCount = 0;
                        }
                        else
                        {
                            this.logger.LogCritical("Get Covid19 Orders - Api Exception {Error}", e.ToString());
                            retVal.ResultError = new()
                            {
                                ResultMessage = "Get Covid19 Orders - Error with Api Exception",
                                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                            };
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        this.logger.LogCritical("Get Covid19 Orders - HTTP Request Exception {Error}", e.ToString());
                        retVal.ResultError = new()
                        {
                            ResultMessage = "Get Covid19 Orders - Error with HTTP Request",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                }
                else
                {
                    retVal.ResultError = new()
                    {
                        ResultMessage = "Get Covid19 Orders - Unable to acquire authentication token",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
                    };
                    this.logger.LogError("Unable to acquire authentication token");
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

                string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
                if (accessToken != null)
                {
                    try
                    {
                        if (isCovid19)
                        {
                            retVal.ResourcePayload = await this.laboratoryApi.GetLaboratoryReportAsync(id, query, accessToken).ConfigureAwait(true);
                        }
                        else
                        {
                            retVal.ResourcePayload = await this.laboratoryApi.GetPlisLaboratoryReportAsync(id, query, accessToken).ConfigureAwait(true);
                        }

                        retVal.ResultStatus = ResultType.Success;
                        retVal.TotalResultCount = 1;
                    }
                    catch (ApiException e)
                    {
                        this.logger.LogCritical("Get Laboratory Report - Api Exception {Error}", e.ToString());
                        retVal.ResultError = new()
                        {
                            ResultMessage = e.StatusCode == HttpStatusCode.NoContent ? "Report not found" : "Get Laboratory Report - Error with Api Exception",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                    catch (HttpRequestException e)
                    {
                        this.logger.LogCritical("Get Laboratory Report - HTTP Request Exception {Error}", e.ToString());
                        retVal.ResultError = new()
                        {
                            ResultMessage = "Get Laboratory Report - Error with HTTP Request",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                }
                else
                {
                    retVal.ResultError = new()
                    {
                        ResultMessage = "Get Laboratory Report - Unable to acquire authentication token",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
                    };
                    this.logger.LogError("Unable to acquire authentication token");
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

            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();
            if (accessToken != null)
            {
                try
                {
                    PhsaResult<PhsaLaboratorySummary> response =
                        await this.laboratoryApi.GetPlisLaboratorySummaryAsync(query, accessToken).ConfigureAwait(true);
                    retVal.ResultStatus = ResultType.Success;
                    retVal.ResourcePayload = response;
                    retVal.TotalResultCount = retVal.ResourcePayload.Result!.LabOrderCount;
                }
                catch (ApiException e)
                {
                    this.logger.LogCritical("Get Laboratory Summary - Api Exception {Error}", e.ToString());
                    if (e.StatusCode == HttpStatusCode.NoContent)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = new() { Result = new() };
                        retVal.TotalResultCount = 0;
                    }
                    else
                    {
                        retVal.ResultError = new()
                        {
                            ResultMessage = "Get Laboratory Summary - Error with Api Exception",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                    }
                }
                catch (HttpRequestException e)
                {
                    this.logger.LogCritical("Get Laboratory Summary - HTTP Request Exception {Error}", e.ToString());
                    retVal.ResultError = new()
                    {
                        ResultMessage = "Get Laboratory Summary - Error with HTTP Request",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                    };
                }
            }
            else
            {
                retVal.ResultError = new()
                {
                    ResultMessage = "Get Laboratory Summary - Unable to acquire authentication token",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Keycloak),
                };
                this.logger.LogError("Unable to acquire authentication token");
            }

            this.logger.LogDebug("Finished getting laboratory summary");
            return retVal;
        }

        /// <inheritdoc/>
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public async Task<RequestResult<PhsaResult<IEnumerable<CovidTestResult>>>> GetPublicTestResults(string accessToken, string phn, DateOnly dateOfBirth, DateOnly collectionDate)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting public COVID-19 test results {Phn} {DateOfBirth} {CollectionDate}...", phn, dateOfBirth, collectionDate);

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            RequestResult<PhsaResult<IEnumerable<CovidTestResult>>> retVal = new()
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
                using HttpContent content = new StringContent(json, null, MediaTypeNames.Application.Json);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                client.DefaultRequestHeaders.Add("X-Forward-For", ipAddress);

                HttpResponseMessage response = await client.PostAsync(endpoint, content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace("Response: {Response}", response);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        this.logger.LogTrace("Response payload: {Payload}", payload);
                        PhsaResult<IEnumerable<CovidTestResult>>? phsaResult = JsonSerializer.Deserialize<PhsaResult<IEnumerable<CovidTestResult>>>(payload);
                        if (phsaResult != null && phsaResult.Result != null)
                        {
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = phsaResult;
                            retVal.TotalResultCount = phsaResult.Result.Count();
                        }
                        else
                        {
                            retVal.ResultError = new RequestResultError
                            {
                                ResultMessage = "Error with JSON data",
                                ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                            };
                        }

                        break;
                    case HttpStatusCode.Forbidden:
                        retVal.ResultError = new RequestResultError
                        {
                            ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                        break;
                    default:
                        retVal.ResultError = new RequestResultError
                        {
                            ResultMessage = $"Unable to connect to Laboratory PublicCovidTestsEndPoint, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                        };
                        this.logger.LogError("Unable to connect to endpoint {EndpointString}, HTTP Error {StatusCode}\n{Payload}", endpointString, response.StatusCode.ToString(), payload);
                        break;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = $"Exception getting public COVID-19 test results: {e}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
                this.logger.LogError("Unexpected exception retrieving public COVID-19 test results {Exception}", e.ToString());
            }

            this.logger.LogDebug("Finished getting public COVID-19 test results {Phn} {DateOfBirth} {CollectionDate}...", phn, dateOfBirth, collectionDate);
            return retVal;
        }
    }
}
