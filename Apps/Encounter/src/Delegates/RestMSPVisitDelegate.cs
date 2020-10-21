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
namespace HealthGateway.Encounter.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMSPVisitDelegate : IMSPVisitDelegate
    {
        private const string ODRConfigSectionKey = "ODR";

        private readonly ILogger logger;
        private readonly IHttpClientService httpClientService;
        private readonly IConfiguration configuration;
        private readonly ODRConfig odrConfig;
        private readonly Uri baseURL;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMSPVisitDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestMSPVisitDelegate(
            ILogger<RestMSPVisitDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.configuration = configuration;
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
                this.baseURL = new Uri(StringManipulator.Replace(this.odrConfig.BaseEndpoint, replacementData) !);
            }
            else
            {
                this.baseURL = new Uri(this.odrConfig.BaseEndpoint);
            }

            logger.LogInformation($"ODR Proxy URL resolved as {this.baseURL.ToString()}");
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(RestMSPVisitDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<MSPVisitHistoryResponse>> GetMSPVisitHistoryAsync(ODRHistoryQuery query, string hdid, string ipAddress)
        {
            using (Source.StartActivity("GetMSPVisitHistoryAsync"))
            {
                RequestResult<MSPVisitHistoryResponse> retVal = new RequestResult<MSPVisitHistoryResponse>();
                this.logger.LogTrace($"Getting MSP visits... {query.PHN.Substring(0, 3)}");

                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                MSPVisitHistory request = new MSPVisitHistory()
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
                    Uri endpoint = new Uri(this.baseURL, this.odrConfig.MSPVisitsEndpoint);
                    HttpResponseMessage response = await client.PostAsync(endpoint, content).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        MSPVisitHistory visitHistory = JsonSerializer.Deserialize<MSPVisitHistory>(payload, options);
                        retVal.ResultStatus = Common.Constants.ResultType.Success;
                        retVal.ResourcePayload = visitHistory.Response;
                        retVal.TotalResultCount = visitHistory.Response?.TotalRecords;
                    }
                    else
                    {
                        retVal.ResultStatus = Common.Constants.ResultType.Error;
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Invalid HTTP Response code of ${response.StatusCode} from ODR with reason ${response.ReasonPhrase}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ODRRecords) };
                        this.logger.LogError(retVal.ResultError.ResultMessage);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    retVal.ResultStatus = Common.Constants.ResultType.Error;
                    retVal.ResultError = new RequestResultError() { ResultMessage = e.ToString(), ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.ODRRecords) };
                    this.logger.LogError($"Unable to post message {e.ToString()}");
                }

                this.logger.LogDebug($"Finished getting MSP visits");

                return retVal;
            }
        }
    }
}
