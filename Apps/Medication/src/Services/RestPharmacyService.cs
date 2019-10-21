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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Medication.Delegates;
    using HealthGateway.Medication.Database;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Medication data service.
    /// </summary>
    public class RestPharmacyService : IPharmacyService
    {
        private readonly IConfiguration configService;
        private readonly IHNMessageParser<Pharmacy> pharmacyParser;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISequenceDelegate sequenceDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestPharmacyService"/> class.
        /// </summary>
        /// <param name="config">The injected configuration provider.</param>
        /// <param name="parser">The injected hn parser.</param>
        /// <param name="httpClientFactory">The injected http client factory.</param>
        /// <param name="sequenceDelegate">The injected sequence delegate.</param>
        public RestPharmacyService(IConfiguration config, IHNMessageParser<Pharmacy> parser, IHttpClientFactory httpClientFactory, ISequenceDelegate sequenceDelegate)
        {
            this.configService = config;
            this.pharmacyParser = parser;
            this.httpClientFactory = httpClientFactory;
            this.sequenceDelegate = sequenceDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId, string userId, string ipAddress)
        {
            using (HttpClient client = this.httpClientFactory.CreateClient("medicationService"))
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url"));
                long traceId = sequenceDelegate.NextValueForSequence(MedicationDBContext.PHARMANET_TRACE_SEQUENCE);
                HNMessage<string> requestMessage = this.pharmacyParser.CreateRequestMessage(pharmacyId, userId, ipAddress, traceId);
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    return JsonConvert.DeserializeObject<HNMessage<Pharmacy>>(payload);
                }
                else
                {
                    return new HNMessage<Pharmacy>(true, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }
        }
    }
}