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
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Api;
    using HealthGateway.Immunization.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestImmunizationDelegate : IImmunizationDelegate
    {
        public const string PHSAConfigSectionKey = "PHSA";
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IImmunizationClient immunizationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="immunizationClient">The client to use for immunization api calls..</param>
        public RestImmunizationDelegate(
            ILogger<RestImmunizationDelegate> logger,
            IConfiguration configuration,
            IAuthenticationDelegate authenticationDelegate,
            IImmunizationClient immunizationClient)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.immunizationClient = immunizationClient;
            this.phsaConfig = new();
            configuration.Bind(PHSAConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestImmunizationDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationViewResponse>>> GetImmunization(string immunizationId)
        {
            using Activity? activity = Source.StartActivity("GetImmunization");
            this.logger.LogDebug("Getting immunization {ImmunizationId}", immunizationId);

            RequestResult<PhsaResult<ImmunizationViewResponse>> requestResult = InitializeResult<ImmunizationViewResponse>();
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                IApiResponse<PhsaResult<ImmunizationViewResponse>> response =
                    await this.immunizationClient.GetImmunization(immunizationId, accessToken).ConfigureAwait(true);
                this.ProcessResponse(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Error}", e.ToString());
                requestResult.ResultError = new RequestResultError()
                {
                    ResultMessage = $"Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationResponse>>> GetImmunizations(int pageIndex = 0)
        {
            using Activity? activity = Source.StartActivity("GetImmunizations");
            this.logger.LogDebug($"Getting immunizations...");

            RequestResult<PhsaResult<ImmunizationResponse>> requestResult = InitializeResult<ImmunizationResponse>();
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                IApiResponse<PhsaResult<ImmunizationResponse>> response =
                    await this.immunizationClient.GetImmunizations(this.phsaConfig.FetchSize, accessToken).ConfigureAwait(true);
                this.ProcessResponse(requestResult, response);
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Error}", e.ToString());
                requestResult.ResultError = new RequestResultError()
                {
                    ResultMessage = $"Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }

            this.logger.LogDebug($"Finished getting Immunizations");
            return requestResult;
        }

        private static RequestResult<PhsaResult<T>> InitializeResult<T>()
            where T : class
        {
            RequestResult<PhsaResult<T>> result = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
                ResourcePayload = new PhsaResult<T>(),
            };
            return result;
        }

        private void ProcessResponse<T>(RequestResult<PhsaResult<T>> requestResult, IApiResponse<PhsaResult<T>> response)
            where T : class
        {
            if (response.Error is null)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        requestResult.ResultStatus = ResultType.Success;
                        requestResult.ResourcePayload!.Result = response!.Content!.Result;
                        requestResult.TotalResultCount = 1;
                        break;
                    case HttpStatusCode.NoContent:
                        requestResult.ResultStatus = ResultType.Success;
                        requestResult.ResourcePayload = new PhsaResult<T>();
                        requestResult.TotalResultCount = 0;
                        requestResult.PageSize = int.Parse(this.phsaConfig.FetchSize, CultureInfo.InvariantCulture);
                        break;
                    case HttpStatusCode.Forbidden:
                        requestResult.ResultError = new RequestResultError()
                        {
                            ResultMessage =
                                $"DID Claim is missing or can not resolve PHN, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                        };
                        break;
                    default:
                        requestResult.ResultError = new RequestResultError()
                        {
                            ResultMessage =
                                $"Unable to connect to Immunizations Endpoint, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(
                                ErrorType.CommunicationExternal,
                                ServiceType.PHSA),
                        };
                        this.logger.LogError("Unexpected status code returned: {StatusCode}", response.StatusCode.ToString());
                        break;
                }
            }
            else
            {
                this.logger.LogError("Exception: {Error}", response.Error.ToString());
                this.logger.LogError("Http Payload: {Content}", response.Error.Content);
                requestResult.ResultError = new RequestResultError()
                {
                    ResultMessage = $"An unexpected error occurred while processing external call",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }
        }
    }
}
