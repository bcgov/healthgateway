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
    public class RestMedicationService : IMedicationService
    {
        private readonly IConfiguration configService;
        private readonly IHNMessageParser<MedicationStatement> medicationParser;
        private readonly IAuthService authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        /// <param name="parser">The injected hn parser.</param>
        /// <param name="authService">The injected authService for client credentials grant (system account).</param>
        public RestMedicationService(IConfiguration config, IHNMessageParser<MedicationStatement> parser, IAuthService authService)
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
        public async Task<List<MedicationStatement>> GetMedicationsAsync(string phn, string userId, string ipAddress)
        {
            JWTModel jwtModel = this.AuthenticateService();
            using (HttpClientHandler clientHandler = new HttpClientHandler())
            {
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                using (HttpClient client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

                    string hnClientUrl = this.configService.GetSection("HNClient").GetValue<string>("Url");
                    HNMessage responseMessage;

                    HNMessage requestMessage = this.medicationParser.CreateRequestMessage(phn, userId, ipAddress);
                    HttpResponseMessage response = await client.PostAsJsonAsync(new Uri($"{hnClientUrl}v1/api/HNClient"), requestMessage).ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        responseMessage = JsonConvert.DeserializeObject<HNMessage>(payload);
                    }
                    else
                    {
                        throw new HttpRequestException($"Unable to connect to HNClient: ${response.StatusCode}");
                    }

                    return this.medicationParser.ParseResponseMessage(responseMessage.Message);
                }
            }
        }

        /// <summary>
        /// Authenticates this service, using Client Credentials Grant.
        /// </summary>
        private JWTModel AuthenticateService()
        {
            JWTModel jwtModel;

            Task<IAuthModel> authenticating = this.authService.GetAuthTokens(); // @todo: maybe cache this in future for efficiency

            jwtModel = authenticating.Result as JWTModel;
            return jwtModel;
        }
    }
}