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
namespace HealthGateway.Immunization.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Models.PHSA;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestImmunizationDelegate : IImmunizationDelegate
    {
        private const string ImmunizationConfigSectionKey = "Immunization";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly ImmunizationConfig immunizationConfig;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public RestImmunizationDelegate(
            ILogger<RestImmunizationDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.httpContextAccessor = httpContextAccessor;
            this.immunizationConfig = new ImmunizationConfig();
            configuration.Bind(ImmunizationConfigSectionKey, this.immunizationConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestImmunizationDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PHSAResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId)
        {
            using Activity? activity = Source.StartActivity("GetImmunization");
            this.logger.LogDebug($"Getting immunization {immunizationId}...");

            string endpointString = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.immunizationConfig.Endpoint, immunizationId);
            RequestResult<PHSAResult<ImmunizationViewResponse>> retVal = await this.ParsePHSAResult<ImmunizationViewResponse>(new Uri(endpointString)).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting Immunization {immunizationId}");

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PHSAResult<ImmunizationResponse>>> GetImmunizations(int pageIndex = 0)
        {
            using Activity? activity = Source.StartActivity("GetImmunizations");
            this.logger.LogDebug($"Getting immunizations...");

            Dictionary<string, string?> query = new ()
            {
                ["limit"] = this.immunizationConfig.FetchSize,
            };
            Uri endpoint = new Uri(QueryHelpers.AddQueryString(this.immunizationConfig.Endpoint, query));
            RequestResult<PHSAResult<ImmunizationResponse>> retVal = await this.ParsePHSAResult<ImmunizationResponse>(endpoint).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting Immunizations");

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationCard>> GetImmunizationCard(string hdid, string immunizationDisease)
        {
            using Activity? activity = Source.StartActivity("GetImmunizationCard");
            this.logger.LogDebug($"Getting immunizations card for patient {hdid}");
            RequestResult<ImmunizationCard> retVal = new ()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? bearerToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (bearerToken != null)
                {
                    string endpointString = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", this.immunizationConfig.CardEndpoint, immunizationDisease);
                    this.logger.LogDebug($"Immunization card Endpoint string is {endpointString}");
                    Uri endpoint = new (endpointString);
                    using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                    HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    this.logger.LogTrace($"Response: {response}");
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            break;
                        case HttpStatusCode.NoContent:
                            retVal.ResultStatus = ResultType.Success;
                            break;
                        case HttpStatusCode.Forbidden:
                            retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                            this.logger.LogWarning($"{retVal.ResultError.ResultMessage}");
                            break;
                        default:
                            retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Immunizations Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                            this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                            break;
                    }
                }
            }

            return retVal;
        }

        private async Task<RequestResult<PHSAResult<T>>> ParsePHSAResult<T>(Uri endpoint)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? bearerToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (bearerToken != null)
                {
                    RequestResult<PHSAResult<T>> retVal = new ()
                    {
                        ResultStatus = ResultType.Error,
                        PageIndex = 0,
                    };

                    try
                    {
                        using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                        client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
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
                                PHSAResult<T>? phsaResult = JsonSerializer.Deserialize<PHSAResult<T>>(payload, options);
                                if (phsaResult != null && phsaResult.Result != null)
                                {
                                    retVal.ResultStatus = Common.Constants.ResultType.Success;
                                    retVal.ResourcePayload = phsaResult;
                                    retVal.TotalResultCount = 1;
                                    retVal.PageSize = int.Parse(this.immunizationConfig.FetchSize, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                }

                                break;
                            case HttpStatusCode.NoContent: // No Immunizations exits for this user
                                retVal.ResultStatus = Common.Constants.ResultType.Success;
                                retVal.ResourcePayload = new PHSAResult<T>();
                                retVal.TotalResultCount = 0;
                                retVal.PageSize = int.Parse(this.immunizationConfig.FetchSize, CultureInfo.InvariantCulture);
                                break;
                            case HttpStatusCode.Forbidden:
                                retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                break;
                            default:
                                retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Immunizations Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                                break;
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Immunization data: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        this.logger.LogError($"Unexpected exception retrieving Immunization data {e}");
                    }

                    return retVal;
                }
            }

            return new RequestResult<PHSAResult<T>>()
            {
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError()
                {
                    ResultMessage = "Error retrieving bearer token",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                },
            };
        }
    }
}
