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
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Models.Cacheable;
    using HealthGateway.Database.Wrapper;
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
        private const string ODRConfigSectionKey = "ODR";
        private const string ProtectiveWordCacheDomain = "ProtectiveWord";
        private const string ODRCacheTimeKey = "CacheTTL";

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;
        private readonly IGenericCacheDelegate genericCacheDelegate;
        private readonly IHashDelegate hashDelegate;
        private readonly ODRConfig odrConfig;
        private readonly string baseURL;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMedStatementDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        /// <param name="genericCacheDelegate">The delegate responsible for caching.</param>
        /// <param name="hashDelegate">The delegate responsible for hashing.</param>
        public RestMedStatementDelegate(
            ILogger<RestMedStatementDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration,
            IGenericCacheDelegate genericCacheDelegate,
            IHashDelegate hashDelegate)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
            this.genericCacheDelegate = genericCacheDelegate;
            this.hashDelegate = hashDelegate;
            this.odrConfig = new ODRConfig();
            this.configuration.Bind(ODRConfigSectionKey, this.odrConfig);
            if (this.odrConfig.DynamicServiceLookup)
            {
                string? serviceHost = Environment.GetEnvironmentVariable($"{this.odrConfig.ServiceName}{this.odrConfig.ServiceHostSuffix}");
                string? servicePort = Environment.GetEnvironmentVariable($"{this.odrConfig.ServiceName}{this.odrConfig.ServicePortSuffix}");
                Dictionary<string, string> replacementData = new Dictionary<string, string>()
                {
                    { "serviceHost", serviceHost! },
                    { "servicePort", servicePort! },
                };
                this.baseURL = Manipulator.Replace(this.odrConfig.Url, replacementData)!;
            }
            else
            {
                this.baseURL = this.odrConfig.Url;
            }
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
                client.BaseAddress = new Uri(this.baseURL);
                string patientProfileEndpoint = this.odrConfig.PatientProfileEndpoint;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                MedicationHistory request = new MedicationHistory()
                {
                    Id = System.Guid.NewGuid(),
                    RequestorHDID = hdid,
                    RequestorIP = ipAddress,
                    Query = query,
                };
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    IgnoreNullValues = true,
                    WriteIndented = true,
                };
                try
                {
                    string json = JsonSerializer.Serialize(request, options);
                    using HttpContent content = new StringContent(json);
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
                        retVal.ResultMessage = $"Invalid HTTP Response code of ${response.StatusCode} from ODR with reason ${response.ReasonPhrase}";
                        this.logger.LogError(retVal.ResultMessage);
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
            bool retVal = false;
            try
            {
                IHash? cacheHash = null;
                if (this.odrConfig.CacheTTL > 0)
                {
                    this.logger.LogDebug("Attempting to fetch Protective Word from Cache");
                    cacheHash = this.genericCacheDelegate.GetCacheObject<IHash>(hdid, ProtectiveWordCacheDomain);
                }
                if (cacheHash == null)
                {
                    this.logger.LogDebug("Unable to find Protective Word in Cache, fetching from source");
                    // The hash isn't in the cache, get Protective word hash from source
                    IHash? hash = Task.Run(async () => await this.GetProtectiveWord(phn, hdid, ipAddress)
                                                                               .ConfigureAwait(true)).Result;
                    if (this.odrConfig.CacheTTL > 0)
                    {
                        this.logger.LogDebug("Storing a copy of the Protective Word in the Cache");
                        this.genericCacheDelegate.CacheObject(hash, hdid, ProtectiveWordCacheDomain, this.odrConfig.CacheTTL);
                    }

                    retVal = this.hashDelegate.Compare(protectiveWord, hash);
                }
                else
                {
                    this.logger.LogDebug("Validating Cached Protective Word");
                    retVal = this.hashDelegate.Compare(protectiveWord, cacheHash);
                }
            }
#pragma warning disable CA1031 // We want to fail on any exception
            catch (Exception e)
#pragma warning restore CA1031
            {
                this.logger.LogError($"Error getting protected word {e.ToString()}");
            }

            return retVal;
        }

        /// <summary>
        /// Returns the hashed protective word.
        /// </summary>
        /// <param name="phn">The PHN to query.</param>
        /// <param name="hdid">The HDID of the user querying.</param>
        /// <param name="ipAddress">The IP of the user querying.</param>
        /// <returns>The hash of the protective word response or null if not set.</returns>
        private async Task<IHash> GetProtectiveWord(string phn, string hdid, string ipAddress)
        {
            IHash retVal;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this.logger.LogTrace($"Getting Protective word for {phn.Substring(0, 3)}");

            using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
            client.BaseAddress = new Uri(this.baseURL);
            string patientProfileEndpoint = this.odrConfig.ProtectiveWordEndpoint;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
            ProtectiveWord request = new ProtectiveWord()
            {
                Id = System.Guid.NewGuid(),
                RequestorHDID = hdid,
                RequestorIP = ipAddress,
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
            HttpResponseMessage response = await client.PostAsync(patientProfileEndpoint, content).ConfigureAwait(true);
            string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                ProtectiveWord protectiveWord = JsonSerializer.Deserialize<ProtectiveWord>(payload, options);
                if (protectiveWord != null && protectiveWord.QueryResponse != null)
                {
                    retVal = this.hashDelegate.Hash(protectiveWord.QueryResponse.Value);
                }
                else
                {
                    this.logger.LogError($"Response payload is not well-formed {payload}");
                    throw new HttpRequestException($"Response payload is not well-formed {payload}");
                }
            }
            else
            {
                this.logger.LogError($"Invalid HTTP Response code of ${response.StatusCode} from ODR with reason {response.ReasonPhrase}");
                throw new HttpRequestException($"Invalid HTTP Response code of ${response.StatusCode} from ODR with reason {response.ReasonPhrase}");
            }

            timer.Stop();
            this.logger.LogDebug($"Finished getting Protective Word {phn.Substring(0, 3)}, Time Elapsed: {timer.Elapsed}");
            return retVal;
        }
    }
}