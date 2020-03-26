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
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.Medication.Constants;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMedStatementDelegate : IMedStatementDelegate
    {
        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestMedStatementDelegate(
            ILogger<RestMedStatementDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configService = configuration;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<MedicationHistoryResponse>> GetMedicationStatementsAsync(MedicationHistoryQuery query, string protectiveWord, string hdid, string ipAddress)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null");
            }
            else if (query.PHN == null)
            {
                throw new ArgumentNullException(nameof(query), "Query PHN cannot be null");
            }

            HNMessage<MedicationHistoryResponse> retVal = new HNMessage<MedicationHistoryResponse>();
            if (this.ValidateProtectiveWord(query.PHN, protectiveWord, hdid, ipAddress))
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                this.logger.LogTrace($"Getting medication statements... {query.PHN.Substring(0, 3)}");

                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.BaseAddress = new Uri(this.configService.GetSection("ODR").GetValue<string>("Url"));
                string patientProfileEndpoint = this.configService.GetSection("ODR").GetValue<string>("PatientProfileEndpoint");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                MedicationHistory request = new MedicationHistory()
                {
                    Query = query,
                };
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                    WriteIndented = true,
                };
                string json = JsonSerializer.Serialize(request, options);
                using HttpContent content = new StringContent(json);
                try
                {
                    HttpResponseMessage response = await client.PostAsync(patientProfileEndpoint, content).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        MedicationHistory medicationHistory = JsonSerializer.Deserialize<MedicationHistory>(payload, options);
                        retVal.Message = medicationHistory.Response!;
                    }
                    else
                    {
                        retVal.Result = Common.Constants.ResultType.Error;
                        retVal.ResultMessage = $"Invalid HTTP Response code of ${response.StatusCode} from ODR with readon ${response.ReasonPhrase}";
                        this.logger.LogError($"Error getting medication statements. {query.PHN.Substring(0, 3)}, {payload}");
                    }
                }
                catch (Exception e)
                {
                    retVal.Result = Common.Constants.ResultType.Error;
                    retVal.ResultMessage = e.ToString();
                    this.logger.LogError($"Unable to post message {e.ToString()}");
                }

                timer.Stop();
                this.logger.LogDebug($"Finished getting medication statements, Time Elapsed: {timer.Elapsed}");
            }
            else
            {
                retVal.Result = Common.Constants.ResultType.Protected;
                retVal.ResultMessage = ErrorMessages.ProtectiveWordErrorMessage;
            }
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<string?> GetProtectiveWord(string phn, string hdid, string ipAddress)
        {
            string? retVal = null;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting Protective word for {phn.Substring(0, 3)}");

            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.BaseAddress = new Uri(this.configService.GetSection("ODR").GetValue<string>("Url"));
            string patientProfileEndpoint = this.configService.GetSection("ODR").GetValue<string>("ProtectiveWordEndpoint");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            ProtectiveWord request = new ProtectiveWord()
            {
                QueryResponse = new ProtectiveWordQueryResponse()
                {
                    PHN = phn,
                    Operator = Constants.ProtectiveWordOperator.Get,
                },
            };
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            string json = JsonSerializer.Serialize(request, options);
            using HttpContent content = new StringContent(json);
            try
            {
                HttpResponseMessage response = await client.PostAsync(patientProfileEndpoint, content).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    ProtectiveWord protectiveWord = JsonSerializer.Deserialize<ProtectiveWord>(payload, options);
                    if (protectiveWord.QueryResponse != null && !string.IsNullOrEmpty(protectiveWord.QueryResponse.Value))
                    {
                        retVal = protectiveWord.QueryResponse.Value;
                    }
                }
                else
                {
                    this.logger.LogError($"Invalid HTTP Response code of ${response.StatusCode} from ODR with reason: ${response.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                this.logger.LogError($"Unable to post message {e.ToString()}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Protective Word {phn.Substring(0, 3)}, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }

        /// <inheritdoc/>
        public Task<bool> SetProtectiveWord(string phn, string newProtectiveWord, string protectiveWord, string hdid, string ipAddress)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> DeleteProtectiveWord(string phn, string protectiveWord, string hdid, string ipAddress)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool ValidateProtectiveWord(string phn, string protectiveWord, string hdid, string ipAddress)
        {
            string? pnetProtectedWord = Task.Run(async () => await this.GetProtectiveWord(phn, hdid, ipAddress)
                                                                       .ConfigureAwait(true)).Result;
            return pnetProtectedWord == null || pnetProtectedWord == protectiveWord;
        }
    }
}