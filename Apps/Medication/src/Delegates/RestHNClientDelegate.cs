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
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Medication.Models;
    using HealthGateway.Medication.Parsers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// BC HealthNet Client Delegate.
    /// </summary>
    public class RestHNClientDelegate : IHNClientDelegate
    {
        private readonly ILogger logger;
        private readonly IHNMessageParser<List<MedicationStatement>> medicationParser;
        private readonly IHNMessageParser<Pharmacy> pharmacyParser;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configService;
        private readonly IAuthenticationDelegate authDelegate;
        private readonly ISequenceDelegate sequenceDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestHNClientDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="medicationParser">The injected medication hn parser.</param>
        /// <param name="pharmacyParser">The injected pharmacy hn parser.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="authDelegate">The injected IAuthenticationDelegate for direct grant authentication.</param>
        /// <param name="sequenceDelegate">The injected sequence delegate.</param>
        public RestHNClientDelegate(
            ILogger<RestHNClientDelegate> logger,
            IHNMessageParser<List<MedicationStatement>> medicationParser,
            IHNMessageParser<Pharmacy> pharmacyParser,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IAuthenticationDelegate authDelegate,
            ISequenceDelegate sequenceDelegate)
        {
            this.logger = logger;
            this.medicationParser = medicationParser;
            this.pharmacyParser = pharmacyParser;
            this.httpClientService = httpClientService;
            this.configService = configuration;
            this.authDelegate = authDelegate;
            this.sequenceDelegate = sequenceDelegate;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<List<MedicationStatement>>> GetMedicationStatementsAsync(string phn, string protectiveWord, string userId, string ipAddress)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting medication statements... {phn!.Substring(0, 3)}");

            JWTModel jwtModel = this.authDelegate.AuthenticateAsSystem();
            HNMessage<List<MedicationStatement>> retVal;
            using (HttpClient client = this.httpClientService.CreateDefaultHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url") !);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

                long traceId = this.sequenceDelegate.GetNextValueForSequence(Sequence.PHARMANET_TRACE);
                HNMessage<string> requestMessage = this.medicationParser.CreateRequestMessage(new HNMessageRequest
                {
                    Phn = phn,
                    UserId = userId,
                    IpAddress = ipAddress,
                    TraceId = traceId,
                    ProtectiveWord = protectiveWord,
                });
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    HNMessage<string> responseMessage = JsonConvert.DeserializeObject<HNMessage<string>>(payload);
                    if (responseMessage != null && responseMessage.Message != null)
                    {
                        retVal = this.medicationParser.ParseResponseMessage(responseMessage.Message);
                    }
                    else
                    {
                        this.logger.LogError($"Parsed payload is null or not valid: {payload}");
                        retVal = new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Error, $"Unable to parse response");
                    }
                }
                else
                {
                    this.logger.LogError($"Error getting medication statements. {phn.Substring(0, 3)}, {payload}");
                    retVal = new HNMessage<List<MedicationStatement>>(Common.Constants.ResultType.Error, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting medication statements. {phn.Substring(0, 3)}, {JsonConvert.SerializeObject(retVal)}, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }

        /// <inheritdoc/>
        public async Task<HNMessage<Pharmacy>> GetPharmacyAsync(string pharmacyId, string userId, string ipAddress)
        {
            this.logger.LogTrace($"Getting pharmacy... {pharmacyId}");
            Stopwatch timer = new Stopwatch();
            timer.Start();

            HNMessage<Pharmacy> retVal;
            JWTModel jwtModel = this.authDelegate.AuthenticateAsSystem();
            using (HttpClient client = this.httpClientService.CreateDefaultHttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.BaseAddress = new Uri(this.configService.GetSection("HNClient")?.GetValue<string>("Url") !);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtModel.AccessToken);

                long traceId = this.sequenceDelegate.GetNextValueForSequence(Sequence.PHARMANET_TRACE);
                HNMessage<string> requestMessage = this.pharmacyParser.CreateRequestMessage(new HNMessageRequest
                {
                    PharmacyId = pharmacyId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    TraceId = traceId,
                });
                HttpResponseMessage response = await client.PostAsJsonAsync("v1/api/HNClient", requestMessage).ConfigureAwait(true);
                string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                if (response.IsSuccessStatusCode)
                {
                    HNMessage<string> responseMessage = JsonConvert.DeserializeObject<HNMessage<string>>(payload);
                    if (responseMessage != null && responseMessage.Message != null)
                    {
                        retVal = this.pharmacyParser.ParseResponseMessage(responseMessage.Message);
                    }
                    else
                    {
                        this.logger.LogError($"Parsed payload is null or not valid: {payload}");
                        retVal = new HNMessage<Pharmacy>(Common.Constants.ResultType.Error, $"Unable to parse response");
                    }
                }
                else
                {
                    this.logger.LogError($"Error getting pharmacy: {pharmacyId}, {payload}");
                    retVal = new HNMessage<Pharmacy>(Common.Constants.ResultType.Error, $"Unable to connect to HNClient: {response.StatusCode}");
                }
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting pharmacy. {JsonConvert.SerializeObject(retVal)}, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }
    }
}