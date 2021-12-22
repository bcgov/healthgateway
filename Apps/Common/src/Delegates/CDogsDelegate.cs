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
namespace HealthGateway.Common.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using HealthGateway.Common.Services;
    using HealthGateway.Common.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The CDogs report generator delegate.
    /// </summary>
    public class CDogsDelegate : ICDogsDelegate
    {
        private const string CDOGSConfigSectionKey = "CDOGS";
        private readonly ILogger<CDogsDelegate> logger;
        private readonly IHttpClientService httpClientService;
        private readonly Uri serviceEndpoint;
        private readonly CDogsConfig cdogsConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDogsDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpClientService">The injected http client service.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public CDogsDelegate(
            ILogger<CDogsDelegate> logger,
            IHttpClientService httpClientService,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.httpClientService = httpClientService;
            this.cdogsConfig = new CDogsConfig();
            configuration.Bind(CDOGSConfigSectionKey, this.cdogsConfig);
            if (this.cdogsConfig.DynamicServiceLookup)
            {
                string? serviceHost = Environment.GetEnvironmentVariable($"{this.cdogsConfig.ServiceName}{this.cdogsConfig.ServiceHostSuffix}");
                string? servicePort = Environment.GetEnvironmentVariable($"{this.cdogsConfig.ServiceName}{this.cdogsConfig.ServicePortSuffix}");
                Dictionary<string, string> replacementData = new Dictionary<string, string>()
                {
                    { "serviceHost", serviceHost! },
                    { "servicePort", servicePort! },
                };

                this.serviceEndpoint = new Uri(StringManipulator.Replace(this.cdogsConfig.BaseEndpoint, replacementData)!);
            }
            else
            {
                this.serviceEndpoint = new Uri(this.cdogsConfig.BaseEndpoint);
            }

            logger.LogInformation($"CDogs URL resolved as {this.serviceEndpoint}");
        }

        private static ActivitySource Source { get; } = new ActivitySource(nameof(CDogsDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GenerateReportAsync(CDogsRequestModel request)
        {
            using (Source.StartActivity("GenerateReportAsync"))
            {
                RequestResult<ReportModel> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                };
                using HttpClient client = this.httpClientService.CreateDefaultHttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                using StringContent httpContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json);
                try
                {
                    Uri endpoint = new Uri($"{this.serviceEndpoint}template/render");
                    HttpResponseMessage? response = await client.PostAsync(endpoint, httpContent).ConfigureAwait(true);
                    byte[] payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(true);
                    this.logger.LogTrace($"CDogs Response: {JsonSerializer.Serialize(response)}");

                    if (response.IsSuccessStatusCode)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = new ReportModel();
                        retVal.ResourcePayload.Data = Convert.ToBase64String(payload);
                        retVal.ResourcePayload.FileName = $"{request.Options.ReportName}.{request.Options.ConvertTo}";
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        retVal.ResultError = new RequestResultError() { ResultMessage = $"Unable to connect to CDogs API, HTTP Error {response.StatusCode}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs) };
                        this.logger.LogError($"Unable to connect to endpoint {endpoint}, HTTP Error {response.StatusCode}\n{payload}");
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    retVal.ResultError = new RequestResultError() { ResultMessage = $"Exception generating report: {e}", ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs) };
                    this.logger.LogError($"Unexpected exception in GenerateReport {e}");
                }

                return retVal;
            }
        }
    }
}
