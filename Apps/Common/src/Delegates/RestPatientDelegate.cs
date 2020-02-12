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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Text.Json;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation that uses HTTP to retrieve patient information.
    /// </summary>
    public class RestPatientDelegate : IPatientDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPatientDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestPatientDelegate(
            ILogger<RestPatientDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<string> GetPatientPHNAsync(string hdid, string authorization)
        {
            string retrievedPhn = string.Empty;
            Patient p = await GetPatientAsync(hdid, authorization).ConfigureAwait(true);
            if (p != null)
            {
                retrievedPhn = p.PersonalHealthNumber;
            }
            return retrievedPhn;
        }

        public async Task<Patient> GetPatientAsync(string hdid, string authorization)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Patient? patient = null;
            this.logger.LogTrace($"Getting patient... {hdid}");
            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            client.BaseAddress = new Uri(this.configuration.GetSection("PatientService").GetValue<string>("Url"));
            using HttpResponseMessage response = await client.GetAsync(new Uri($"v1/api/Patient/{hdid}", UriKind.Relative)).ConfigureAwait(true);
            string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                patient = JsonSerializer.Deserialize<Patient>(payload);
            }
            else
            {
                this.logger.LogError($"Error getting patient. {hdid}, {payload}");
                throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
            }

            timer.Stop();
            if (string.IsNullOrEmpty(patient.PersonalHealthNumber))
            {
                this.logger.LogDebug($"Finished getting patient. {hdid}, PHN not found, Time Elapsed: {timer.Elapsed}");
            }
            else
            {
                this.logger.LogDebug($"Finished getting patient. {hdid}, {patient.PersonalHealthNumber.Substring(0, 3)}, Time Elapsed: {timer.Elapsed}");
            }

            return patient;
        }
    }
}
