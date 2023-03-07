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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace HealthGateway.PatientDataAccess.Phsa
{
    internal class PhsaTokenProvider : IPhsaTokenProvider
    {
        private readonly ITokenApi tokenApi;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDistributedCache cache;
        private readonly PhsaClientConfiguration config;

        private string? UserId => httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        private async Task<string?> GetUserAccessToken() => await httpContextAccessor.HttpContext!.GetTokenAsync("access_token");

        public PhsaTokenProvider(ITokenApi tokenApi, IHttpContextAccessor httpContextAccessor, IDistributedCache cache, PhsaClientConfiguration config)
        {
            this.tokenApi = tokenApi;
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
            this.config = config;
        }

        public async Task<string> GetAsync(CancellationToken ct)
        {
            var userId = UserId;
            if (string.IsNullOrEmpty(userId)) throw new InvalidOperationException("No user was not found");

            var token = await SwapToken(userId, ct);

            return token;
        }

        private async Task<string> SwapToken(string userId, CancellationToken ct)
        {
            var cacheKey = $"phsa-token:{userId}";
            var phsaToken = await cache.GetAsync<TokenSwapResponse>(cacheKey, ct);
            if (phsaToken == null)
            {
                var userAccessToken = await GetUserAccessToken();
                if (string.IsNullOrEmpty(userAccessToken)) throw new InvalidOperationException($"User {userId} doesn't have an access token");

                var formData = FormParameters(userAccessToken);

                using FormUrlEncodedContent content = new(formData);

                content.Headers.Clear();
                content.Headers.Add(@"Content-Type", @"application/x-www-form-urlencoded");
                phsaToken = await tokenApi.SwapTokenAsync(content, ct);
                await cache.SetAsync(cacheKey, phsaToken, TimeSpan.FromMinutes(phsaToken.ExpiresIn), ct);
            }

            return phsaToken.AccessToken;
        }

        private IEnumerable<KeyValuePair<string, string>> FormParameters(string accessToken) =>
            new Dictionary<string, string>
            {
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["grant_type"] = config.GrantType,
                ["scope"] = config.Scope,
                ["token"] = accessToken,
            };
    }
}
