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
namespace HealthGateway.Admin.Server.Delegates
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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.Immunization;
    using HealthGateway.Admin.Parsers.Immunization;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class RestImmunizationAdminDelegate : IImmunizationAdminDelegate
    {
        private const string ImmunizationConfigSectionKey = "Immunization";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly ImmunizationConfig immunizationConfig;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationAdminDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
        public RestImmunizationAdminDelegate(
            ILogger<RestImmunizationAdminDelegate> logger,
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

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestImmunizationAdminDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunizationEvent(string immunizationId)
        {
            using Activity? activity = Source.StartActivity("GetImmunizationEvent");
            RequestResult<ImmunizationEvent> retVal;
            RequestResult<PHSAResult<ImmunizationViewResponse>> immsResponse = await this.GetImmunization(immunizationId).ConfigureAwait(true);
            if (immsResponse.ResultStatus == ResultType.Success)
            {
                retVal = new ()
                {
                    ResultStatus = immsResponse.ResultStatus,
                    ResourcePayload = EventParser.FromPHSAModel(immsResponse.ResourcePayload!.Result),
                    PageIndex = immsResponse.PageIndex,
                    PageSize = immsResponse.PageSize,
                    TotalResultCount = immsResponse.TotalResultCount,
                };
            }
            else
            {
                retVal = new ()
                {
                    ResultStatus = immsResponse.ResultStatus,
                    ResultError = immsResponse.ResultError,
                };
            }

            return retVal;
        }

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
        public async Task<RequestResult<ImmunizationResult>> GetImmunizationEvents(PatientModel patient, int pageIndex = 0)
        {
            using Activity? activity = Source.StartActivity("GetImmunizationEvents");
            RequestResult<ImmunizationResult> retVal;
            RequestResult<PHSAResult<IList<ImmunizationViewResponse>>> immsResponse;
            int retries = 0;
            do
            {
                immsResponse = await this.GetImmunizations(patient, pageIndex).ConfigureAwait(true);
            }
            while (this.ShouldRetry(immsResponse) && retries++ < this.immunizationConfig.MaximumRetries);

            if (immsResponse.ResultStatus == ResultType.Success && immsResponse.ResourcePayload != null)
            {
                retVal = new ()
                {
                    ResultStatus = immsResponse.ResultStatus,
                    ResultError = immsResponse.ResultError,
                    PageIndex = immsResponse.PageIndex,
                    PageSize = immsResponse.PageSize,
                    TotalResultCount = immsResponse.ResourcePayload.Result?.Count ?? 0,
                    ResourcePayload = new ImmunizationResult(
                            LoadStateModel.FromPHSAModel(immsResponse.ResourcePayload.LoadState),
                            EventParser.FromPHSAModelList(immsResponse.ResourcePayload.Result)),
                };
            }
            else
            {
                retVal = new ()
                {
                    ResultStatus = immsResponse.ResultStatus,
                    ResultError = immsResponse.ResultError,
                };
            }

            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PHSAResult<IList<ImmunizationViewResponse>>>> GetImmunizations(PatientModel patient, int pageIndex = 0)
        {
            using Activity? activity = Source.StartActivity("GetImmunizations");
            this.logger.LogDebug($"Getting immunizations...");
            RequestResult<PHSAResult<IList<ImmunizationViewResponse>>> retVal;
            if (!string.IsNullOrEmpty(patient.PersonalHealthNumber) && patient.Birthdate != DateTime.MinValue)
            {
                Dictionary<string, string?> query = new ()
                {
                    ["phn"] = patient.PersonalHealthNumber,
                    ["dob"] = patient.Birthdate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    ["limit"] = this.immunizationConfig.FetchSize,
                };
                Uri endpoint = new Uri(QueryHelpers.AddQueryString(this.immunizationConfig.Endpoint, query));
                retVal = await this.ParsePHSAResult<IList<ImmunizationViewResponse>>(endpoint).ConfigureAwait(true);
                this.logger.LogDebug($"Finished getting Immunizations");
            }
            else
            {
                retVal = new ()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError()
                    {
                        ResultMessage = $"Patient PHN ({patient.PersonalHealthNumber}) or DOB ({patient.Birthdate}) Invalid",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    },
                };
            }

            return retVal;
        }

        /// <summary>
        /// Determines if the Immunization records are being refreshed and waits if true.
        /// </summary>
        /// <param name="result">The response from the Immunization delegate.</param>
        /// <returns>true if a retry should be attempted by the delegate.</returns>
        private bool ShouldRetry(RequestResult<PHSAResult<IList<ImmunizationViewResponse>>> result)
        {
            bool retry = result.ResultStatus == ResultType.Success &&
                         result.ResourcePayload != null &&
                         result.ResourcePayload.LoadState.RefreshInProgress;
            if (retry)
            {
                Thread.Sleep(this.immunizationConfig.RetryWait);
            }

            return retry;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Team Decision>")]
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
                        using Activity? activity = Source.StartActivity("Communicating with PHSA");
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
                                    retVal.ResultStatus = ResultType.Success;
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
                                retVal.ResultStatus = ResultType.Success;
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
                    catch (Exception e)
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
                ResultError = new ()
                {
                    ResultMessage = "Error retrieving bearer token",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                },
            };
        }
    }
}
