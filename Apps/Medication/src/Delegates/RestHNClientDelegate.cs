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
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Common.Authentication;
    using HealthGateway.Common.Authentication.Models;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// BC HealthNet Client Delegate.
    /// </summary>
    public class RestHNClientDelegate : IHNClientDelegate
    {
        private readonly IHNMessageParser<List<MedicationStatement>> medicationParser;
        private readonly IHNMessageParser<Pharmacy> pharmacyParser;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configService;
        private readonly IAuthService authService;
        private readonly ISequenceDelegate sequenceDelegate;
        private readonly IDrugLookupDelegate drugLookupDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestHNClientDelegate"/> class.
        /// </summary>
        /// <param name="medicationParser">The injected medication hn parser.</param>
        /// <param name="pharmacyParser">The injected pharmacy hn parser.</param>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authService">The injected authService for client credentials grant (system account).</param>
        /// <param name="sequenceDelegate">The injected sequence delegate.</param>
        public RestHNClientDelegate(
            IHNMessageParser<List<MedicationStatement>> medicationParser,
            IHNMessageParser<Pharmacy> pharmacyParser,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IAuthService authService,
            ISequenceDelegate sequenceDelegate)
        {
            this.medicationParser = medicationParser;
            this.pharmacyParser = pharmacyParser;
            this.httpClientFactory = httpClientFactory;
            this.configService = configuration;
            this.authService = authService;
            this.sequenceDelegate = sequenceDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<List<MedicationStatement>>> GetMedicationStatementsAsync(string phn, string protectiveWord, string userId, string ipAddress)
        {
            JWTModel jwtModel = this.authService.AuthenticateService();
            using (HttpClient client = this.httpClientFactory.CreateClient("medicationService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

                long traceId = this.sequenceDelegate.NextValueForSequence(Sequence.PHARMANET_TRACE);
                HNMessage<string> requestMessage = this.medicationParser.CreateRequestMessage(phn, userId, ipAddress, traceId, protectiveWord);
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    HNMessage<string> responseMessage = JsonConvert.DeserializeObject<HNMessage<string>>(payload);
                    return this.medicationParser.ParseResponseMessage(responseMessage.Message);
                }
                else
                {
                    return new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Error, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }
        }

        /// <inheritdoc/>
        public async Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId, string userId, string ipAddress)
        {
            JWTModel jwtModel = this.authService.AuthenticateService();
            using (HttpClient client = this.httpClientFactory.CreateClient("medicationService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url"));
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

                long traceId = this.sequenceDelegate.NextValueForSequence(Sequence.PHARMANET_TRACE);
                HNMessage<string> requestMessage = this.pharmacyParser.CreateRequestMessage(pharmacyId, userId, ipAddress, traceId, null);
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    HNMessage<string> responseMessage = JsonConvert.DeserializeObject<HNMessage<string>>(payload);
                    return this.pharmacyParser.ParseResponseMessage(responseMessage.Message);
                }
                else
                {
                    return new HNMessage<Pharmacy>(Common.Constants.ResultType.Error, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }
        }
    }
}