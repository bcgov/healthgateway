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
namespace HealthGateway.Common.Delegates.PHSA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestVaccineStatusDelegate : IVaccineStatusDelegate
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly PHSAConfig phsaConfig;

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestVaccineStatusDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="httpContextAccessor">The HTTP Context accessor.</param>
        public RestVaccineStatusDelegate(
            ILogger<RestVaccineStatusDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.httpContextAccessor = httpContextAccessor;
            this.phsaConfig = new PHSAConfig();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestVaccineStatusDelegate));

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public async Task<RequestResult<PHSAResult<VaccineStatusResult>>> GetVaccineStatus(VaccineStatusQuery query, string accessToken, bool isPublicEndpoint)
        {
            using Activity? activity = Source.StartActivity("GetVaccineStatus");
            this.logger.LogDebug($"Getting vaccine status {query.HdId} {query.PersonalHealthNumber} {query.DateOfBirth} {query.DateOfVaccine} {query.IncludeFederalVaccineProof}...");

            HttpContent? content = null;
            Uri endpoint = null!;
            string endpointString = this.phsaConfig.BaseUrl.ToString();
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            RequestResult<PHSAResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            try
            {
                string json = JsonSerializer.Serialize(query);
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                if (isPublicEndpoint)
                {
                    endpointString += this.phsaConfig.PublicVaccineStatusEndpoint;
                    endpoint = new Uri(endpointString);
                    content = new StringContent(json, null, MediaTypeNames.Application.Json);
                }
                else if (!string.IsNullOrEmpty(query.HdId))
                {
                    Dictionary<string, string?> queryDict = new()
                    {
                        ["subjectHdid"] = query.HdId,
                        ["federalPvc"] = query.IncludeFederalVaccineProof.ToString(),
                    };
                    endpointString += this.phsaConfig.VaccineStatusEndpoint;
                    endpoint = new Uri(QueryHelpers.AddQueryString(endpointString, queryDict));
                }
                else
                {
                    endpointString += this.phsaConfig.VaccineStatusEndpoint;
                    endpoint = new Uri(endpointString);
                    content = new StringContent(json, null, MediaTypeNames.Application.Json);
                }

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
                        PHSAResult<VaccineStatusResult>? phsaResult = JsonSerializer.Deserialize<PHSAResult<VaccineStatusResult>>(payload);
                        if (phsaResult != null && phsaResult.Result != null)
                        {
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = phsaResult;
                            retVal.TotalResultCount = 1;
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
                            ResultMessage = $"Unable to connect to Immunizations/VaccineStatus Endpoint, HTTP Error {response.StatusCode}",
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
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting vaccine status: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception retrieving vaccine status {e}");
            }
            finally
            {
                content?.Dispose();
            }

            this.logger.LogDebug($"Finished getting vaccine status {query.HdId} {query.PersonalHealthNumber} {query.DateOfBirth}");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PHSAResult<VaccineStatusResult>>> GetVaccineStatusWithRetries(VaccineStatusQuery query, string accessToken, bool isPublicEndpoint)
        {
            using Activity? activity = Source.StartActivity("RetryGetVaccineStatus");
            RequestResult<PHSAResult<VaccineStatusResult>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            RequestResult<PHSAResult<VaccineStatusResult>> response;
            int attemptCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.GetVaccineStatus(query, accessToken, isPublicEndpoint).ConfigureAwait(true);

                refreshInProgress = response.ResultStatus == ResultType.Success &&
                                    response.ResourcePayload != null &&
                                    response.ResourcePayload.LoadState.RefreshInProgress;

                attemptCount++;
                if (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries)
                {
                    this.logger.LogDebug($"Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.ResourcePayload!.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                this.logger.LogDebug($"Maximum retry attempts reached.");
                retVal.ResultError = new RequestResultError() { ResultMessage = "Refresh in progress", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
            }
            else if (response.ResultStatus == ResultType.Success)
            {
                retVal = response;
            }
            else
            {
                retVal.ResultError = response.ResultError;
            }

            return retVal;
        }

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public async Task<RequestResult<PHSAResult<RecordCard>>> GetRecordCard(RecordCardQuery query, string accessToken)
        {
            using Activity? activity = Source.StartActivity("GetRecordCard");
            this.logger.LogDebug($"Getting record card {query.PersonalHealthNumber.Substring(0, 5)} {query.DateOfBirth}...");
            string endpointString = $"{this.phsaConfig.BaseUrl}{this.phsaConfig.RecordCardEndpoint}";

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            RequestResult<PHSAResult<RecordCard>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            try
            {
                string json = JsonSerializer.Serialize(query);
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                using HttpContent content = new StringContent(json, null, MediaTypeNames.Application.Json);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                client.DefaultRequestHeaders.Add("X-Forward-For", ipAddress);

                HttpResponseMessage response = await client.PostAsync(new Uri(endpointString), content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                this.logger.LogTrace($"Response: {response}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        this.logger.LogTrace($"Response payload: {payload}");
                        PHSAResult<RecordCard>? phsaResult = JsonSerializer.Deserialize<PHSAResult<RecordCard>>(payload);
                        if (phsaResult != null && phsaResult.Result != null)
                        {
                            retVal.ResultStatus = ResultType.Success;
                            retVal.ResourcePayload = phsaResult;
                            retVal.TotalResultCount = 1;
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
                    case HttpStatusCode.NoContent: // No vaccine status exists for this patient
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = new PHSAResult<RecordCard>()
                        {
                            Result = new RecordCard(),
                        };
                        retVal.TotalResultCount = 0;
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
                            ResultMessage = $"Unable to connect to record card Endpoint, HTTP Error {response.StatusCode}",
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
                retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception getting record card: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
                this.logger.LogError($"Unexpected exception retrieving record card {e}");
            }

            this.logger.LogDebug($"Finished getting record card {query.PersonalHealthNumber.Substring(0, 5)} {query.DateOfBirth}");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PHSAResult<RecordCard>>> GetRecordCardWithRetries(RecordCardQuery query, string accessToken)
        {
            using Activity? activity = Source.StartActivity("RetryGetRecordCard");
            RequestResult<PHSAResult<RecordCard>> retVal = new()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            RequestResult<PHSAResult<RecordCard>> response;
            int attemptCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.GetRecordCard(query, accessToken).ConfigureAwait(true);

                refreshInProgress = response.ResultStatus == ResultType.Success &&
                                    response.ResourcePayload != null &&
                                    response.ResourcePayload.LoadState.RefreshInProgress;

                attemptCount++;
                if (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries)
                {
                    this.logger.LogDebug($"Refresh in progress, trying again....");
                    await Task.Delay(Math.Max(response.ResourcePayload!.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && attemptCount <= this.phsaConfig.MaxRetries);

            if (refreshInProgress)
            {
                this.logger.LogDebug($"Maximum retry attempts reached.");
                retVal.ResultError = new RequestResultError() { ResultMessage = "Refresh in progress", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA) };
            }
            else if (response.ResultStatus == ResultType.Success)
            {
                retVal = response;
            }
            else
            {
                retVal.ResultError = response.ResultError;
            }

            return retVal;
        }
    }
}
