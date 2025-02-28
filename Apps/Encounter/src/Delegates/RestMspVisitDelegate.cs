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
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.ODR;
    using HealthGateway.Encounter.Api;
    using HealthGateway.Encounter.Models.ODR;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// ODR Implementation for Rest Medication Statements.
    /// </summary>
    public class RestMspVisitDelegate : IMspVisitDelegate
    {
        private readonly IMspVisitApi mspVisitApi;
        private readonly ILogger<RestMspVisitDelegate> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestMspVisitDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="mspVisitApi">The injected client to use for msp visit api calls.</param>
        public RestMspVisitDelegate(ILogger<RestMspVisitDelegate> logger, IMspVisitApi mspVisitApi)
        {
            this.logger = logger;
            this.mspVisitApi = mspVisitApi;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<MspVisitHistoryResponse>> GetMspVisitHistoryAsync(OdrHistoryQuery query, string hdid, string ipAddress, CancellationToken ct = default)
        {
            RequestResult<MspVisitHistoryResponse> retVal = new()
            {
                ResultStatus = ResultType.Error,
            };

            MspVisitHistory request = new()
            {
                Id = Guid.NewGuid(),
                RequestorHdid = hdid,
                RequestorIp = ipAddress,
                Query = query,
            };

            try
            {
                this.logger.LogDebug("Retrieving health visits");
                MspVisitHistory visitHistory = await this.mspVisitApi.GetMspVisitsAsync(request, ct);

                retVal.ResultStatus = ResultType.Success;
                retVal.ResourcePayload = visitHistory.Response;
                retVal.TotalResultCount = visitHistory.Response?.TotalRecords;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error retrieving health visits");
                HttpStatusCode? statusCode = (e as ApiException)?.StatusCode ?? ((HttpRequestException)e).StatusCode;
                retVal.ResultError = new()
                {
                    ResultMessage = $"Status: {statusCode}. Error while retrieving Msp Visits",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return retVal;
        }
    }
}
