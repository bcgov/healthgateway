// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.WebClient.Services
{
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class BetaRequestService : IBetaRequestService
    {
        private readonly ILogger logger;
        private readonly IBetaRequestDelegate betaRequestDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="betaRequestDelegate">The email delegate to interact with the DB.</param>
        public BetaRequestService(ILogger<UserEmailService> logger, IBetaRequestDelegate betaRequestDelegate)
        {
            this.logger = logger;
            this.betaRequestDelegate = betaRequestDelegate;
        }

        /// <inheritdoc />
        public BetaRequest GetBetaRequest(string hdid)
        {
            this.logger.LogTrace($"Retrieving Beta queued email for {hdid}");
            DBResult<BetaRequest> queuedEmail = this.betaRequestDelegate.GetBetaRequest(hdid);
            this.logger.LogDebug($"Finished retrieving email: {JsonConvert.SerializeObject(queuedEmail)}");
            return queuedEmail.Payload;
        }

        /// <inheritdoc />
        public RequestResult<BetaRequest> CreateBetaRequest(BetaRequest betaRequest)
        {
            Contract.Requires(betaRequest != null);
            this.logger.LogTrace($"Creating a beta request... {JsonConvert.SerializeObject(betaRequest)}");

            RequestResult<BetaRequest> requestResult = new RequestResult<BetaRequest>();
            string hdid = betaRequest.hdid;
            betaRequest.CreatedBy = hdid;
            betaRequest.UpdatedBy = hdid;

            DBResult<BetaRequest> insertResult = this.betaRequestDelegate.InsertBetaRequest(betaRequest);
            if (insertResult.Status == DBStatusCode.Created)
            {
                requestResult.ResourcePayload = insertResult.Payload;
                requestResult.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug($"Finished creating user profile. {JsonConvert.SerializeObject(insertResult)}");
            return requestResult;
        }
    }
}
