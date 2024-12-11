// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.Utils.Phsa
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;

    /// <summary>
    /// Delegating access handler that swaps the user token for a PHSA token and injects it.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IAccessTokenService tokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthHeaderHandler"/> class.
        /// </summary>
        /// <param name="tokenService">The Token Swap service to use.</param>
        public AuthHeaderHandler(IAccessTokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestResult<TokenSwapResponse> phsaTokenResponse = await this.tokenService.GetPhsaAccessTokenAsync(cancellationToken);
            if (phsaTokenResponse.ResultStatus == ResultType.Success)
            {
                string? token = phsaTokenResponse.ResourcePayload?.AccessToken;
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            throw new HttpRequestException(phsaTokenResponse.ResultError?.ResultMessage);
        }
    }
}
