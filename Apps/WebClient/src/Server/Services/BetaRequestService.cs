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
    using HealthGateway.Common.Services;
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
        private readonly IEmailQueueService emailQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="betaRequestDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        public BetaRequestService(ILogger<UserEmailService> logger, IBetaRequestDelegate betaRequestDelegate, IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.betaRequestDelegate = betaRequestDelegate;
            this.emailQueueService = emailQueueService;
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
        public RequestResult<BetaRequest> PutBetaRequest(BetaRequest betaRequest)
        {
            Contract.Requires(betaRequest != null);
            Contract.Requires(!string.IsNullOrEmpty(betaRequest.HdId));
            this.logger.LogTrace($"Creating a beta request... {JsonConvert.SerializeObject(betaRequest)}");

            // If there is a previous request, update it isntead of creating a new one
            BetaRequest previousRequest = this.betaRequestDelegate.GetBetaRequest(betaRequest.HdId).Payload;
            if (previousRequest != null)
            {
                RequestResult<BetaRequest> requestResult = new RequestResult<BetaRequest>();
                DBResult<BetaRequest> insertResult = this.betaRequestDelegate.UpdateBetaRequest(betaRequest);
                if (insertResult.Status == DBStatusCode.Updated)
                {
                    requestResult.ResourcePayload = insertResult.Payload;
                    requestResult.ResultStatus = ResultType.Success;
                    this.emailQueueService.QueueNewEmail(betaRequest.EmailAddress, EmailTemplateName.BETA_CONFIRMATION_TEMPLATE);
                    this.logger.LogDebug($"Finished updating beta request. {JsonConvert.SerializeObject(insertResult)}");
                }
                else
                {
                    requestResult.ResultMessage = insertResult.Message;
                    requestResult.ResultStatus = ResultType.Error;
                }

                return requestResult;
            }
            else
            {
                betaRequest.CreatedBy = betaRequest.HdId;
                betaRequest.UpdatedBy = betaRequest.HdId;
                DBResult<BetaRequest> insertResult = this.betaRequestDelegate.InsertBetaRequest(betaRequest);
                RequestResult<BetaRequest> requestResult = new RequestResult<BetaRequest>();
                if (insertResult.Status == DBStatusCode.Created)
                {
                    requestResult.ResourcePayload = insertResult.Payload;
                    requestResult.ResultStatus = ResultType.Success;
                    this.emailQueueService.QueueNewEmail(betaRequest.EmailAddress, EmailTemplateName.BETA_CONFIRMATION_TEMPLATE);
                    this.logger.LogDebug($"Finished creating beta request. {JsonConvert.SerializeObject(insertResult)}");
                }
                else
                {
                    requestResult.ResultMessage = insertResult.Message;
                    requestResult.ResultStatus = ResultType.Error;
                }
                return requestResult;
            }
        }
    }
}
