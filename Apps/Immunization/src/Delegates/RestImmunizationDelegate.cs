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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
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
        private readonly IAuthenticationDelegate authenticationDelegate;
        private readonly IImmunizationApi immunizationApi;
        private readonly ILogger<RestImmunizationDelegate> logger;
        private readonly PhsaConfig phsaConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestImmunizationDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <param name="authenticationDelegate">The injected authentication delegate to fetch tokens.</param>
        /// <param name="immunizationApi">The injected Refit API for immunizations.</param>
        public RestImmunizationDelegate(
            ILogger<RestImmunizationDelegate> logger,
            IConfiguration configuration,
            IAuthenticationDelegate authenticationDelegate,
            IImmunizationApi immunizationApi)
        {
            this.logger = logger;
            this.authenticationDelegate = authenticationDelegate;
            this.immunizationApi = immunizationApi;
            this.phsaConfig = configuration.GetSection(PhsaConfig.ConfigurationSectionKey).Get<PhsaConfig>() ?? new();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<PhsaResult<ImmunizationResponse>>> GetImmunizationsAsync(string hdid, CancellationToken ct = default)
        {
            RequestResult<PhsaResult<ImmunizationResponse>> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
                ResourcePayload = new PhsaResult<ImmunizationResponse>(),
            };

            string? accessToken = await this.authenticationDelegate.FetchAuthenticatedUserTokenAsync(ct);

            try
            {
                this.logger.LogDebug("Retrieving immunizations");
                PhsaResult<ImmunizationResponse> response = await this.immunizationApi.GetImmunizationsAsync(hdid, this.phsaConfig.FetchSize, accessToken, ct);

                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload = response;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving immunizations");
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with Get Immunization Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }
    }
}
