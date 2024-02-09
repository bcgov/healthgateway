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
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// Implementation that uses HTTP to swap access tokens.
    /// </summary>
    public class RestTokenSwapDelegate : ITokenSwapDelegate
    {
        /// <summary>
        /// Configuration section key for PHSA values.
        /// </summary>
        private readonly ILogger logger;
        private readonly PhsaConfigV2 phsaConfigV2;
        private readonly ITokenSwapApi tokenSwapApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestTokenSwapDelegate"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="tokenSwapApi">The refit api responsible for swapping tokens with PHSA.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="configurationSectionKey">The configuration section to use when binding values.</param>
        public RestTokenSwapDelegate(
            ILogger<RestTokenSwapDelegate> logger,
            ITokenSwapApi tokenSwapApi,
            IConfiguration configuration,
            string? configurationSectionKey = PhsaConfigV2.ConfigurationSectionKey)
        {
            this.logger = logger;
            this.tokenSwapApi = tokenSwapApi;
            this.phsaConfigV2 = new PhsaConfigV2();
            configuration.Bind(configurationSectionKey, this.phsaConfigV2); // Initializes ClientId, ClientSecret, GrantType and Scope.
        }

        private static ActivitySource Source { get; } = new(nameof(RestTokenSwapDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<TokenSwapResponse>> SwapTokenAsync(string accessToken, CancellationToken ct = default)
        {
            using Activity? activity = Source.StartActivity();
            RequestResult<TokenSwapResponse> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                PageSize = 0,
            };

            try
            {
                Dictionary<string, string> formData = this.FormParameters(accessToken);
                using FormUrlEncodedContent content = new(formData);
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                TokenSwapResponse response = await this.tokenSwapApi.SwapTokenAsync(content, ct);
                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload = response;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogCritical("TokenSwap API Exception {Exception}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with Token Swap API",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <summary>
        /// Gets the form parameters to swap tokens.
        /// </summary>
        private Dictionary<string, string> FormParameters(string accessToken)
        {
            return new Dictionary<string, string>
            {
                ["client_id"] = this.phsaConfigV2.ClientId,
                ["client_secret"] = this.phsaConfigV2.ClientSecret,
                ["grant_type"] = this.phsaConfigV2.GrantType,
                ["scope"] = this.phsaConfigV2.Scope,
                ["token"] = accessToken,
            };
        }
    }
}
