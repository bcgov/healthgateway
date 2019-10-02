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
namespace HealthGateway.Medication.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Runtime.Serialization.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authentication;
    using HealthGateway.Common.Authentication.Models;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class MedicationService : IMedicationService
    {
        private readonly IConfiguration configService;
        private readonly IAuthService authService;
        private readonly IHNMessageParser<MedicationStatement> medicationParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MedicationService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        /// <param name="parser">The injected hn parser.</param>
        /// <param name="authService">The injected authService for client credentials grant (system account).</param>
        public MedicationService(IConfiguration config, IHNMessageParser<MedicationStatement> parser, IAuthService authService)
        {
            this.configService = config;
            this.medicationParser = parser;
            this.authService = authService;
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
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                try
                {
                    string patienServiceUrl = this.configService.GetSection("PatientService").GetValue<string>("Url");
                    using (HttpResponseMessage response = await client.GetAsync(new Uri($"{patienServiceUrl}v1/api/Patient/{hdid}")).ConfigureAwait(true))
                    {
                        response.EnsureSuccessStatusCode();
                        string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                        Patient responseMessage = JsonConvert.DeserializeObject<Patient>(payload);
                        return responseMessage.PersonalHealthNumber;
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Message :{0} ", e.Message);
                    return null;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<List<MedicationStatement>> GetMedicationsAsync(string phn, string userId, string ipAddress)
        {
            using (HttpClient client = new HttpClient())
            {
                using (Task<IAuthModel> authenticating = this.authService.GetAuthTokens()) // @todo: maybe cache this in future for efficiency
                {
                    IAuthModel jwtModel = authenticating.Result;

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

                    // Add the JWT that this service obtained through authenticating with KeyCloak
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);
                }

                string hnClientUrl = this.configService.GetSection("HNClient").GetValue<string>("Url");

                using (var httpResponse = await client.GetAsync(new Uri(hnClientUrl)).ConfigureAwait(false))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<MedicationStatement>));

                    return serializer.ReadObject(await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(true)) as List<MedicationStatement>;
                }
            }
        }
    }
}