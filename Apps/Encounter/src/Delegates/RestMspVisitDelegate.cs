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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMspVisitDelegate : IMspVisitDelegate
    {
        private const string OdrConfigSectionKey = "ODR";
        private readonly Uri baseUrl;
        private readonly IHttpClientService httpClientService;

        private readonly ILogger logger;
        private readonly OdrConfig odrConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMspVisitDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public RestMspVisitDelegate(
            ILogger<RestMspVisitDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.odrConfig = new OdrConfig();
            configuration.Bind(OdrConfigSectionKey, this.odrConfig);
            if (this.odrConfig.DynamicServiceLookup)
            {
                string? serviceHost = Environment.GetEnvironmentVariable($"{this.odrConfig.ServiceName}{this.odrConfig.ServiceHostSuffix}");
                string? servicePort = Environment.GetEnvironmentVariable($"{this.odrConfig.ServiceName}{this.odrConfig.ServicePortSuffix}");
                Dictionary<string, string> replacementData = new()
                {
                    { "serviceHost", serviceHost! },
                    { "servicePort", servicePort! },
                };
                this.baseUrl = new Uri(StringManipulator.Replace(this.odrConfig.BaseEndpoint, replacementData)!);
            }
            else
            {
                this.baseUrl = new Uri(this.odrConfig.BaseEndpoint);
            }

            logger.LogDebug("ODR Proxy URL resolved as {BaseUrl}", this.baseUrl);
        }

        private static ActivitySource Source { get; } = new(nameof(RestMspVisitDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<MspVisitHistoryResponse>> GetMspVisitHistoryAsync(OdrHistoryQuery query, string hdid, string ipAddress)
        {
            using (Source.StartActivity())
            {
                RequestResult<MspVisitHistoryResponse> retVal = new();
                this.logger.LogTrace("Getting MSP visits... {Phn}", query.Phn.Substring(0, 3));

                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                MspVisitHistory request = new()
                {
                    Id = Guid.NewGuid(),
                    RequestorHdid = hdid,
                    RequestorIp = ipAddress,
                    Query = query,
                };
                try
                {
                    string json = JsonSerializer.Serialize(request);
                    using HttpContent content = new StringContent(json, null, MediaTypeNames.Application.Json);
                    Uri endpoint = new(this.baseUrl, this.odrConfig.MspVisitsEndpoint);
                    HttpResponseMessage response = await client.PostAsync(endpoint, content).ConfigureAwait(true);
                    string payload = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    if (response.IsSuccessStatusCode)
                    {
                        MspVisitHistory? visitHistory = JsonSerializer.Deserialize<MspVisitHistory>(payload);
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = visitHistory?.Response;
                        retVal.TotalResultCount = visitHistory?.Response?.TotalRecords;
                    }
                    else
                    {
                        retVal.ResultStatus = ResultType.Error;
                        retVal.ResultError = new RequestResultError
                        {
                            ResultMessage = $"Invalid HTTP Response code of {response.StatusCode} from ODR with reason {response.ReasonPhrase}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.OdrRecords),
                        };
                        this.logger.LogError("MSP Visits endpoint error message... {Message}", retVal.ResultError.ResultMessage);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    retVal.ResultStatus = ResultType.Error;
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = e.ToString(),
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.OdrRecords),
                    };
                    this.logger.LogError("Unable to post message {Exception}", e);
                }

                this.logger.LogDebug("Finished getting MSP visits");

                return retVal;
            }
        }
    }
}
