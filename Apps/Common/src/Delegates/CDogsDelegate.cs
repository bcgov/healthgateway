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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
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

        private static ActivitySource ActivitySource { get; } = new(typeof(CDogsDelegate).FullName);

        /// <inheritdoc/>
        public async Task<RequestResult<ReportModel>> GenerateReportAsync(CDogsRequestModel request, CancellationToken ct = default)
        {
            using Activity? activity = ActivitySource.StartActivity();

            RequestResult<ReportModel> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Generating report using CDOGS");
                HttpResponseMessage response = await this.cdogsApi.GenerateDocumentAsync(request, ct);
                byte[] payload = await response.Content.ReadAsByteArrayAsync(ct);
                activity?.AddBaggage("ResponseStatusCode", response.StatusCode.ToString());

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
                    this.logger.LogWarning("Error generating report using CDOGS: {Payload}", payload);
                    retVal.ResultError = new RequestResultError
                    {
                        ResultMessage = $"Unable to connect to CDogs API, HTTP Error {response.StatusCode}",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                    };
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.logger.LogError(e, "Error generating report using CDOGS");
                retVal.ResultError = new RequestResultError
                {
                    ResultMessage = $"Exception generating report: {e}",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.CDogs),
                };
            }

            return retVal;
        }
    }
}
