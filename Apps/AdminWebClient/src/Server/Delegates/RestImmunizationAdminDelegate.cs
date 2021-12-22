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
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models.CovidSupport;
    using HealthGateway.Admin.Models.CovidSupport.PHSA;
    using HealthGateway.Admin.Parsers.Immunization;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class RestImmunizationAdminDelegate : IImmunizationAdminDelegate
    {
        private const string PHSAConfigSectionKey = "PHSA";
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly PHSAConfig phsaConfig;
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
            this.phsaConfig = new PHSAConfig();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestImmunizationAdminDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<VaccineDetails>> GetVaccineDetailsWithRetries(PatientModel patient, bool refresh)
        {
            using Activity? activity = Source.StartActivity("GetVaccineDetailsWithRetries");
            RequestResult<VaccineDetails> retVal;
            RequestResult<PHSAResult<VaccineDetailsResponse>> response;
            int retryCount = 0;
            bool refreshInProgress;
            do
            {
                response = await this.GetVaccineDetailsResponse(patient, refresh).ConfigureAwait(true);

                refreshInProgress = response.ResultStatus == ResultType.Success &&
                                    response.ResourcePayload != null &&
                                    response.ResourcePayload.LoadState.RefreshInProgress;

                if (refreshInProgress)
                {
                    this.logger.LogInformation("Waiting before we retry getting Vaccine Details");
                    await Task.Delay(Math.Max(response.ResourcePayload!.LoadState.BackOffMilliseconds, this.phsaConfig.BackOffMilliseconds)).ConfigureAwait(true);
                }
            }
            while (refreshInProgress && retryCount++ < this.phsaConfig.MaxRetries);

            if (response.ResultStatus == ResultType.Success && response.ResourcePayload != null)
            {
                VaccineDetails vaccineDetails = new(
                    VaccineDoseParser.FromPHSAModelList(response.ResourcePayload.Result?.Doses),
                    response.ResourcePayload.Result?.VaccineStatusResult)
                {
                    Blocked = response.ResourcePayload.Result?.Blocked ?? false,
                    ContainsInvalidDoses = response.ResourcePayload.Result?.ContainsInvalidDoses ?? false,
                };

                retVal = new()
                {
                    ResultStatus = response.ResultStatus,
                    ResultError = response.ResultError,
                    PageIndex = response.PageIndex,
                    PageSize = response.PageSize,
                    TotalResultCount = response.TotalResultCount,
                    ResourcePayload = vaccineDetails,
                };
            }
            else
            {
                retVal = new()
                {
                    ResultStatus = response.ResultStatus,
                    ResultError = response.ResultError,
                };
            }

            return retVal;
        }

        /// <summary>
        /// Gets the vaccine details response for the provided patient.
        /// The patient must have the PHN and DOB provided.
        /// </summary>
        /// <param name="patient">The patient to query for vaccine details.</param>
        /// <param name="refresh">Whether the call should force cached data to be refreshed.</param>
        /// <returns>The wrapped vaccine details response.</returns>
        private async Task<RequestResult<PHSAResult<VaccineDetailsResponse>>> GetVaccineDetailsResponse(PatientModel patient, bool refresh)
        {
            using Activity? activity = Source.StartActivity("GetVaccineDetails");
            this.logger.LogDebug($"Getting vaccine details...");
            RequestResult<PHSAResult<VaccineDetailsResponse>> retVal;
            if (!string.IsNullOrEmpty(patient.PersonalHealthNumber) && patient.Birthdate != DateTime.MinValue)
            {
                CovidImmunizationsRequest requestContent = new CovidImmunizationsRequest()
                {
                    PersonalHealthNumber = patient.PersonalHealthNumber,
                    IgnoreCache = refresh,
                };
                Uri endpoint = new($"{this.phsaConfig.BaseUrl}{this.phsaConfig.ImmunizationEndpoint}/VaccineValidationDetails");
                using StringContent httpContent = new(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");
                retVal = await this.CallEndpoint<VaccineDetailsResponse>(endpoint, httpContent).ConfigureAwait(true);
                this.logger.LogDebug($"Finished getting vaccine details");
            }
            else
            {
                retVal = new()
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Team Decision>")]
        private async Task<RequestResult<PHSAResult<T>>> CallEndpoint<T>(Uri endpoint, StringContent content)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? bearerToken = await httpContext.GetTokenAsync("access_token").ConfigureAwait(true);
                if (bearerToken != null)
                {
                    RequestResult<PHSAResult<T>> retVal = new()
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
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                        HttpResponseMessage response = await client.PostAsync(endpoint, content).ConfigureAwait(true);
                        string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        this.logger.LogTrace($"Response: {response}");

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                this.logger.LogTrace($"Response payload: {payload}");
                                PHSAResult<T>? phsaResult = JsonSerializer.Deserialize<PHSAResult<T>>(payload);
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
                                retVal.ResourcePayload = new PHSAResult<T>();
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
                ResultError = new()
                {
                    ResultMessage = "Error retrieving bearer token",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.InvalidState, ServiceType.Immunization),
                },
            };
        }
    }
}
