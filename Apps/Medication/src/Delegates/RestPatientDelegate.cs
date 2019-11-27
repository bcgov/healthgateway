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
namespace HealthGateway.Medication.Delegates
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation that uses HTTP to retrieve patient information.
    /// </summary>
    public class RestPatientDelegate : IPatientDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPatientDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestPatientDelegate(
            ILogger<RestPatientDelegate> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<string> GetPatientPHNAsync(string hdid, string authorization)
        {
            string retrievedPhn;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting patient phn... {hdid}");

            using (HttpClient client = this.httpClientFactory.CreateClient("patientService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", authorization);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configuration.GetSection("PatientService").GetValue<string>("Url"));

                using (HttpResponseMessage response = await client.GetAsync(new Uri($"v1/api/Patient/{hdid}", UriKind.Relative)).ConfigureAwait(true))
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        Patient responseMessage = JsonConvert.DeserializeObject<Patient>(payload);
                        retrievedPhn = responseMessage.PersonalHealthNumber;
                    }
                    else
                    {
                        this.logger.LogError($"Error getting patient phn. {hdid}, {payload}");
                        throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                    }
                }

                timer.Stop();
                if (string.IsNullOrEmpty(retrievedPhn))
                {
                    this.logger.LogDebug($"Finished getting patient phn. {hdid}, PHN not found, Time Elapsed: {timer.Elapsed}");
                }
                else
                {
                    this.logger.LogDebug($"Finished getting patient phn. {hdid}, {retrievedPhn.Substring(0, 3)}, Time Elapsed: {timer.Elapsed}");
                }

                return retrievedPhn;
            }
        }
    }
}
