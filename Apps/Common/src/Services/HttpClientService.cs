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

namespace HealthGateway.Common.Services
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// A custom http client factory service.
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly IConfiguration configService;

        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configService">The injected configuration provider.</param>
        public HttpClientService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configService)
        {
            this.httpClientFactory = httpClientFactory;
            this.configService = configService;
        }

        /// <inheritdoc/>
        public HttpClient CreateDefaultHttpClient()
        {
            HttpClient retVal = this.httpClientFactory.CreateClient();
            return this.SetTimeout(retVal);
        }

        /// <summary>
        /// Initalizes the request timeout for the <see cref="HttpClientService"/> class.
        /// </summary>
        /// <param name="client">The injected http client factory.</param>
        /// <returns>The HttpClient.</returns>
        public HttpClient SetTimeout(HttpClient client)
        {
            string? timeout = this.configService.GetSection("HttpClient").GetValue<string>("Timeout");

            if (!string.IsNullOrEmpty(timeout))
            {
                // Timeout should be in timespan format: 00:01:00
                client.Timeout = TimeSpan.Parse(timeout, CultureInfo.CurrentCulture);
            }

            return client;
        }
    }
}
