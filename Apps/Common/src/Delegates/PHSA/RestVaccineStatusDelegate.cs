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
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Constants.PHSA;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Http;
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
        /// <param name="httpContextAccessor">The Http Context accessor.</param>
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
        public async Task<RequestResult<PHSAResult<VaccineStatusResult>>> GetVaccineStatus(VaccineStatusQuery query, string accessToken)
        {
            using Activity? activity = Source.StartActivity("GetVaccineStatus");
            this.logger.LogDebug($"Getting vaccine status {query.PersonalHealthNumber.Substring(0, 5)} {query.DateOfBirth}...");
            string endpointString = $"{this.phsaConfig.BaseUrl}{this.phsaConfig.VaccineStatusEndpoint}";

            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            string? ipAddress = httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();

            RequestResult<PHSAResult<VaccineStatusResult>> retVal = new ()
            {
                ResultStatus = ResultType.Error,
                PageIndex = 0,
            };

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                    WriteIndented = true,
                };
                string json = JsonSerializer.Serialize(query, jsonOptions);
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
                        PHSAResult<VaccineStatusResult>? phsaResult = JsonSerializer.Deserialize<PHSAResult<VaccineStatusResult>>(payload, jsonOptions);
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
                        retVal.ResourcePayload = new PHSAResult<VaccineStatusResult>()
                        {
                            Result = new VaccineStatusResult()
                            {
                                StatusIndicator = VaccineState.NotFound.ToString(),
                            },
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

            this.logger.LogDebug($"Finished getting vaccine status {query.PersonalHealthNumber.Substring(0, 5)} {query.DateOfBirth}");
            return retVal;
        }
    }
}
