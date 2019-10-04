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
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configService;
        private readonly IHNMessageParser<MedicationStatement> medicationParser;
        private readonly IAuthService authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedicationService"/> class.
        /// </summary>
        /// <param name="parser">The injected hn parser.</param>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authService">The injected authService for client credentials grant (system account).</param>
        public RestMedicationService(IHNMessageParser<MedicationStatement> parser, IHttpClientFactory httpClientFactory, IConfiguration configuration, IAuthService authService)
        {
            this.medicationParser = parser;
            this.httpClientFactory = httpClientFactory;
            this.configService = configuration;
            this.authService = authService;
        }

        /// <inheritdoc/>
        public async Task<List<MedicationStatement>> GetMedicationsAsync(string phn, string userId, string ipAddress)
        {
            JWTModel jwtModel = this.AuthenticateService();
            using (HttpClient client = this.httpClientFactory.CreateClient("medicationService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);
                HNMessage responseMessage;

                HNMessage requestMessage = this.medicationParser.CreateRequestMessage(phn, userId, ipAddress);
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
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

        /// <summary>
        /// Authenticates this service, using Client Credentials Grant.
        /// </summary>
        private JWTModel AuthenticateService()
        {
            JWTModel jwtModel;

            Task<IAuthModel> authenticating = this.authService.ClientCredentialsAuth(); // @todo: maybe cache this in future for efficiency

            jwtModel = authenticating.Result as JWTModel;
            return jwtModel;
        }
    }
}