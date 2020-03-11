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
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMedStatementDelegate : IRestMedStatementDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configService;
        private readonly ISequenceDelegate sequenceDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="sequenceDelegate">The injected sequence delegate.</param>
        public RestMedStatementDelegate(
            ILogger<RestMedStatementDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            ISequenceDelegate sequenceDelegate)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configService = configuration;
            this.sequenceDelegate = sequenceDelegate;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MedicationHistoryResponse>> GetMedicationStatementsAsync(string phn, string protectiveWord, string userId, string ipAddress)
        {
            Contract.Requires(phn != null);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting medication statements... {phn.Substring(0, 3)}");

            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.BaseAddress = new Uri(this.configService.GetSection("ODR")?.GetValue<string>("Url"));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            MedicationHistory request = new MedicationHistory();
            MedicationHistoryQuery query = new MedicationHistoryQuery()
            {
                PHN = phn,
            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            request.Query = query;
            string json = System.Text.Json.JsonSerializer.Serialize(request, options);
            HttpContent content = new StringContent(json);
            try
            {
                HttpResponseMessage response = await client.PostAsync(string.Empty, content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    // HNMessage<string> responseMessage = JsonConvert.DeserializeObject<HNMessage<string>>(payload);
                    // retVal = this.medicationParser.ParseResponseMessage(responseMessage.Message);
                }
                else
                {
                    // this.logger.LogError($"Error getting medication statements. {phn.Substring(0, 3)}, {payload}");
                    // retVal = new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Error, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Unable to post message {e.ToString()}");
            }

            timer.Stop();
            // this.logger.LogDebug($"Finished getting medication statements. {phn.Substring(0, 3)}, {JsonConvert.SerializeObject(retVal)}, Time Elapsed: {timer.Elapsed}");
            return new List<MedicationHistoryResponse>();
        }
    }
}