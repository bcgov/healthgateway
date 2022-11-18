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
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.CDogs;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The CDOGS report generator delegate.
    /// </summary>
    public class CDogsDelegate : ICDogsDelegate
    {
        private readonly ICDogsApi cdogsApi;
        private readonly ILogger<CDogsDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDogsDelegate"/> class.
        /// </summary>
        /// <param name="logger">The injected Logger Provider.</param>
        /// <param name="cdogsApi">The injected Refit API.</param>
        public CDogsDelegate(
            ILogger<CDogsDelegate> logger,
            ICDogsApi cdogsApi)
        {
            this.logger = logger;
            this.cdogsApi = cdogsApi;
        }

        private static ActivitySource Source { get; } = new(nameof(CDogsDelegate));

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GenerateReportAsync(CDogsRequestModel request)
        {
            using (Source.StartActivity())
            {
                RequestResult<ReportModel> retVal = new()
                {
                    ResultStatus = ResultType.Error,
                };

                try
                {
                    HttpResponseMessage response = await this.cdogsApi.GenerateDocument(request).ConfigureAwait(true);
                    byte[] payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(true);
                    this.logger.LogTrace("CDogs Response status code: {ResponseStatusCode}", response.StatusCode);

                    if (response.IsSuccessStatusCode)
                    {
                        retVal.ResultStatus = ResultType.Success;
                        retVal.ResourcePayload = new ReportModel
                        {
                            Data = Convert.ToBase64String(payload),
                            FileName = $"{request.Options.ReportName}.{request.Options.ConvertTo}",
                        };
                        retVal.TotalResultCount = 1;
                    }
                    else
                    {
                        retVal.ResultError = new RequestResultError
                        {
                            ResultMessage = $"Unable to connect to CDogs API, HTTP Error {response.StatusCode}",
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                        };
                        this.logger.LogError("Unable to connect to CDogs API, HTTP Error {StatusCode}\n{Payload}", response.StatusCode, payload);
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = $"Exception generating report: {e}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                    };
                    this.logger.LogError("Unexpected exception in GenerateReport {Exception}", e);
                }

                return retVal;
            }
        }
    }
}
