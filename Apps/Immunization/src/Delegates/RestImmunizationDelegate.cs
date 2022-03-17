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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
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
        private const string PHSAConfigSectionKey = "PHSA";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly PhsaConfig phsaConfig;

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
            this.phsaConfig = new();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestImmunizationDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId)
        {
            using Activity? activity = Source.StartActivity("GetImmunization");
            this.logger.LogDebug($"Getting immunization {immunizationId}...");

            string endpointString = $"{this.phsaConfig.BaseUrl}{this.phsaConfig.ImmunizationEndpoint}/{immunizationId}";
            RequestResult<PhsaResult<ImmunizationViewResponse>> retVal = await this.ParsePHSAResult<ImmunizationViewResponse>(new Uri(endpointString)).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting Immunization {immunizationId}");

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationResponse>>> GetImmunizations(int pageIndex = 0)
        {
            using Activity? activity = Source.StartActivity("GetImmunizations");
            this.logger.LogDebug($"Getting immunizations...");

            Dictionary<string, string?> query = new()
            {
                ["limit"] = this.phsaConfig.FetchSize,
            };
            string endpointString = $"{this.phsaConfig.BaseUrl}{this.phsaConfig.ImmunizationEndpoint}";
            Uri endpoint = new Uri(QueryHelpers.AddQueryString(endpointString, query));
            RequestResult<PhsaResult<ImmunizationResponse>> retVal = await this.ParsePHSAResult<ImmunizationResponse>(endpoint).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting Immunizations");

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationCard>>> GetVaccineHistory(string hdid, string immunizationDisease)
        {
            using Activity? activity = Source.StartActivity("GetVaccineHistory");
            this.logger.LogDebug($"Getting vaccine history...");

            Dictionary<string, string?> query = new()
            {
                ["limit"] = this.phsaConfig.FetchSize,
            };
            string endpointString = $"{this.phsaConfig.BaseUrl}{this.phsaConfig.ImmunizationEndpoint}/RecordCards/{immunizationDisease}";
            Uri endpoint = new Uri(QueryHelpers.AddQueryString(endpointString, query));
            RequestResult<PhsaResult<ImmunizationCard>> retVal = await this.ParsePHSAResult<ImmunizationCard>(endpoint).ConfigureAwait(true);
            this.logger.LogDebug($"Finished getting vaccine history.");

            return retVal;
        }

        private async Task<RequestResult<PhsaResult<T>>> ParsePHSAResult<T>(Uri endpoint)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? bearerToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (bearerToken != null)
                {
                    RequestResult<PhsaResult<T>> retVal = new()
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
                                this.logger.LogTrace($"Response payload: {payload}");
                                PhsaResult<T>? phsaResult = JsonSerializer.Deserialize<PhsaResult<T>>(payload);
                                if (phsaResult != null && phsaResult.Result != null)
                                {
                                    retVal.ResultStatus = ResultType.Success;
                                    retVal.ResourcePayload = phsaResult;
                                    retVal.TotalResultCount = 1;
                                    retVal.PageSize = int.Parse(this.phsaConfig.FetchSize, CultureInfo.InvariantCulture);
                                }
                                else
                                {
                                    retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                }

                                break;
                            case HttpStatusCode.NoContent: // No Immunizations exits for this user
                                retVal.ResultStatus = ResultType.Success;
                                retVal.ResourcePayload = new PhsaResult<T>();
                                retVal.TotalResultCount = 0;
                                retVal.PageSize = int.Parse(this.phsaConfig.FetchSize, CultureInfo.InvariantCulture);
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

            return new RequestResult<PhsaResult<T>>()
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
