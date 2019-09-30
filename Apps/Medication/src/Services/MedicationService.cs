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
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using HealthGateway.MedicationService.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class MedicationService : IMedicationService
    {
        private readonly IConfiguration configService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        public MedicationService(IConfiguration config)
        {
            this.configService = config;
        }

        /// <inheritdoc/>
        public async Task<List<Prescription>> GetPrescriptionsAsync(string id)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Prescription>));
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                // client.DefaultRequestHeaders.Add("Authorization", "Bearer ${TOKEN}")
                string hnClientUrl = this.configService.GetSection("HNClient").GetValue<string>("Url");

                var response = await client.GetAsync(new Uri(hnClientUrl)).ConfigureAwait(true);
                List<Prescription> medications;
                if (response.IsSuccessStatusCode)
                {
                    medications = serializer.ReadObject(await response.Content.ReadAsStreamAsync().ConfigureAwait(true)) as List<Prescription>;
                }
                else
                {
                    throw new HttpRequestException($"Unable to connect to HNClient: ${response.StatusCode}");
                }

                return medications;
            }
        }
    }
}