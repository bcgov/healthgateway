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
    using System.Threading.Tasks;
    using HealthGateway.MedicationService.Models;
    using HealthGateway.MedicationService.Parsers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestMedicationService : IMedicationService
    {
        private readonly IConfiguration configService;
        private readonly IHNMessageParser<MedicationStatement> medicationParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        /// <param name="parser">The injected hn parser.</param>
        public RestMedicationService(IConfiguration config, IHNMessageParser<MedicationStatement> parser)
        {
            this.configService = config;
            this.medicationParser = parser;
        }

        /// <summary>
        /// Gets the patient phn.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <returns>The patient phn.</returns>
        public async Task<string> GetPatientPHN(string hdid)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                // Inject JWT
                // client.DefaultRequestHeaders.Add("Authorization", "Bearer ${TOKEN}")
                string hnClientUrl = this.configService.GetSection("PatientService").GetValue<string>("Url");
                HttpResponseMessage response = await client.GetAsync(new Uri($"{hnClientUrl}v1/api/Patient/{hdid}")).ConfigureAwait(true);
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

        /// <inheritdoc/>
        public async Task<List<MedicationStatement>> GetMedicationStatementsAsync(string id)
        {
            Task<IAuthModel> authenticating = this.authService.GetAuthTokens();  // @todo maybe cache this in future for efficiency
            IAuthModel jwtModel = authenticating.Result;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            // Add the JWT that this service obtained through authenticating with KeyCloak
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

            string hnClientUrl = this.configService.GetSection("HNClient").GetValue<string>("Url");

            var response = await client.GetAsync(new Uri(hnClientUrl)).ConfigureAwait(true);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<MedicationStatement>));

            List<MedicationStatement> medications;
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Unable to connect to HNClient: ${response.StatusCode}");
            }
            else
            {
                medications = serializer.ReadObject(await response.Content.ReadAsStreamAsync().ConfigureAwait(true)) as List<MedicationStatement>;
            }

            return medications;
        }
    }
}