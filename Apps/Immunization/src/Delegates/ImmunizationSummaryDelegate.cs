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
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using Hl7.Fhir.Model;
    using Hl7.Fhir.Serialization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Implementation that uses HTTP to retrieve immunization information.
    /// </summary>
    public class ImmunizationSummaryDelegate : IImmunizationSummaryDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationSummaryDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public ImmunizationSummaryDelegate(
            ILogger<ImmunizationSummaryDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetImmunizationSummary(string phn)
        {
            Bundle responseMessage = new Bundle();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting immunization summary... {phn}");

            using (HttpClient client = this.httpClientService.CreateDefaultHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();

                // client.DefaultRequestHeaders.Add("Authorization", authorization);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configuration.GetSection("Panorama").GetValue<string>("Url"));

                using (StringContent httpContent = new StringContent("{'phn':'9735353315','dob':'19670602'}", Encoding.UTF8, MediaTypeNames.Application.Json))
                {
                    httpContent.Headers.Add("X-API-Key", "C10A9A2A8D804E9DA7978BD7CAC1A013");
                    httpContent.Headers.Add("X-PHN", "{ 'phn':'9735353315','dob':'19670602' }");

                    using (HttpResponseMessage response = await client.PostAsync(new Uri("api/ImmsSummary", UriKind.Relative), httpContent).ConfigureAwait(true))
                    {
                        string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        this.logger.LogDebug($"FHIR Payload: {payload}");
                        if (response.IsSuccessStatusCode)
                        {
                            FhirJsonParser parser = new FhirJsonParser();
                            try
                            {
                                responseMessage = parser.Parse<Bundle>(payload);
                                foreach (var e in responseMessage.Entry)
                                {
                                    Console.WriteLine(e);
                                }

                                this.logger.LogDebug($"FHIR Parsed.");
                            }
                            catch (FormatException e)
                            {
                                this.logger.LogError($"FHIR Failed to be parsed. {e.ToString()}");
                            }
                        }
                        else
                        {
                            this.logger.LogError($"Error getting immunization summary. {phn}, {payload}");
                            throw new HttpRequestException($"Unable to connect to Panorama: ${response.StatusCode}");
                        }
                    }
                }

                timer.Stop();
                this.logger.LogDebug($"Finished getting immunization summary. {phn}, Time Elapsed: {timer.Elapsed}");

                return responseMessage;
            }
        }
    }
}
