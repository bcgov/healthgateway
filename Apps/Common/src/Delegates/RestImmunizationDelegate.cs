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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Collections.Generic;
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
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization data.
    /// </summary>
    public class RestImmunizationDelegate : IImmunizationDelegate
    {
        private const string ServiceEndpointsSectionKey = "ServiceEndpoints";
        private const string EndpointKey = "Immunization";
        private const string ImmunizationServiceSectionKey = "ImmunizationService";
        private const string EndpointPathKey = "EndpointPath";
        private readonly string immunizationEndpoint;

        /// <summary>
        /// Gets or sets the http context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;

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

            string endpoint = configuration.GetSection(ServiceEndpointsSectionKey).GetValue<string>(EndpointKey);
            string path = configuration.GetSection(ImmunizationServiceSectionKey).GetValue<string>(EndpointPathKey);
            this.immunizationEndpoint = $"{endpoint}{path}";
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunization(string hdid, string immunizationId)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? bearerToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (bearerToken != null)
                {
                    RequestResult<ImmunizationEvent> retVal = new()
                    {
                        ResultStatus = ResultType.Error,
                        PageIndex = 0,
                    };

                    this.logger.LogTrace($"Getting immunization {immunizationId} from the Immunization Server for {hdid}...");
                    using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                    string endpointString = string.Format(CultureInfo.InvariantCulture, "{0}{1}", this.immunizationEndpoint, immunizationId);
                    Dictionary<string, string?> query = new()
                    {
                        ["hdid"] = hdid,
                    };
                    Uri endpoint = new Uri(QueryHelpers.AddQueryString(endpointString, query));
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(endpoint).ConfigureAwait(true);
                        string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        this.logger.LogTrace($"Response: {response}");
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                RequestResult<ImmunizationEvent>? requestResult = JsonSerializer.Deserialize<RequestResult<ImmunizationEvent>>(payload);
                                if (requestResult != null)
                                {
                                    retVal = requestResult;
                                }
                                else
                                {
                                    retVal.ResultError = new RequestResultError() { ResultMessage = "Error with JSON data", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                }

                                break;
                            case HttpStatusCode.NoContent:
                                retVal.ResultStatus = ResultType.Success;
                                retVal.ResourcePayload = new ImmunizationEvent();
                                retVal.TotalResultCount = 0;
                                break;
                            case HttpStatusCode.Forbidden:
                                this.logger.LogError($"Error Details: {payload}");
                                retVal.ResultError = new RequestResultError() { ResultMessage = $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                                break;
                            default:
                                retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to Immunization Service Endpoint, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.SMSInvalid, ServiceType.PHSA) };
                                this.logger.LogError($"Unable to connect to endpoint {endpointString}, HTTP Error {response.StatusCode}\n{payload}");
                                break;
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting Immunization: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                        this.logger.LogError($"Unexpected exception in GetImmunization {e}");
                    }

                    return retVal;
                }
            }

            return new RequestResult<ImmunizationEvent>()
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
