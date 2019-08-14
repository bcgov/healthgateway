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

namespace HealthGateway.WebClient.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

#pragma warning disable CA1303 // disable literal strings check

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public class ImmsService : IImmsService
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClient;
        private readonly IAuthService authService;
        private readonly string baseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmsService"/> class.
        /// </summary>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClient">Injected HttpClientFactory Provider.</param>
        /// <param name="authService">Injected IAuthService Provider.</param>
        public ImmsService(ILogger<AuthService> logger, IConfiguration configuration, IHttpClientFactory httpClient, IAuthService authService)
        {
            // configuration cannot be null since it is used directly on the constructor.
            Contract.Requires(configuration != null);

            this.logger = logger;
            this.httpClient = httpClient;
            this.authService = authService;
            this.baseUrl = $"{configuration["ImmsServiceUrl"]}/imms";
        }

        /// <summary>
        /// Returns the list of immunization records.
        /// </summary>
        /// <returns>A list of immunization records.</returns>
        public async Task<IEnumerable<ImmsData>> GetItems()
        {
            this.logger.LogDebug("Retrieving Immunization items");

            using (HttpClient client = this.httpClient.CreateClient())
            {
                AuthData authData = this.authService.GetAuthenticationData();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authData.Token);
                using (HttpResponseMessage response = await client.GetAsync(new Uri($"{this.baseUrl}/items")).ConfigureAwait(true))
                {
                    return await response.Content.ReadAsAsync<IEnumerable<ImmsData>>().ConfigureAwait(true);
                }
            }
        }
    }
}
