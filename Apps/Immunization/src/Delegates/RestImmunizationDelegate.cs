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
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Api;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class RestImmunizationDelegate : IImmunizationDelegate
    {
        /// <summary>
        /// Configuration section key for PHSA values.
        /// </summary>
        public const string PhsaConfigSectionKey = "PHSA";
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IImmunizationApi immunizationApi;
        private readonly ILogger logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authenticationDelegate">The auth delegate to fetch tokens.</param>
        /// <param name="immunizationApi">The client to use for immunization api calls..</param>
        public RestImmunizationDelegate(
            ILogger<RestImmunizationDelegate> logger,
            IConfiguration configuration,
            IAuthenticationDelegate authenticationDelegate,
            IImmunizationApi immunizationApi)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.immunizationApi = immunizationApi;
            this.phsaConfig = new();
            configuration.Bind(PhsaConfigSectionKey, this.phsaConfig);
        }

        private static ActivitySource Source { get; } = new(nameof(RestImmunizationDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationViewResponse>>> GetImmunizationAsync(string immunizationId)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting immunization {ImmunizationId}", immunizationId);

            RequestResult<PhsaResult<ImmunizationViewResponse>> requestResult = InitializeResult<ImmunizationViewResponse>();
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                PhsaResult<ImmunizationViewResponse> response =
                    await this.immunizationApi.GetImmunizationAsync(immunizationId, accessToken).ConfigureAwait(true);
                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload!.Result = response.Result;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical("Get Immunization unexpected Exception {Error}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with Get Immunization Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationResponse>>> GetImmunizationsAsync(string hdid)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Getting immunizations for hdid: {Hdid}", hdid);

            RequestResult<PhsaResult<ImmunizationResponse>> requestResult = InitializeResult<ImmunizationResponse>();
            string? accessToken = this.authenticationDelegate.FetchAuthenticatedUserToken();

            try
            {
                PhsaResult<ImmunizationResponse> response =
                    await this.immunizationApi.GetImmunizationsAsync(hdid, this.phsaConfig.FetchSize, accessToken).ConfigureAwait(true);
                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload!.Result = response.Result;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical("Get Immunizations unexpected Exception {Error}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with Get Immunization Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            this.logger.LogDebug("Finished getting Immunizations");
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
    }
}
