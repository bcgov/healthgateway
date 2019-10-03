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
namespace HealthGateway.MedicationService.Services
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.MedicationService.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class RestPatientService : IPatientService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPatientService"/> class.
        /// </summary>
        /// <param name="clientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestPatientService(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = clientFactory;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<string> GetPatientPHNAsync(string hdid)
        {
            using (HttpClient client = this.httpClientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configuration.GetSection("PatientService").GetValue<string>("Url"));

                HttpResponseMessage response = await client.GetAsync($"Patient/{hdid}").ConfigureAwait(true);
                Patient responseMessage;
                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    responseMessage = JsonConvert.DeserializeObject<Patient>(payload);
                }
                else
                {
                    throw new HttpRequestException($"Unable to connect to PatientService: ${response.StatusCode}");
                }

                return responseMessage.PersonalHealthNumber;
            }
        }
    }
}
