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
namespace HealthGateway.Admin.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
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
#pragma warning disable SA1310 // Disable _ in variable name
        private const string HOST_TEMPLATE_VARIABLE = "host";
        private const string INVITE_KEY_TEMPLATE_VARIABLE = "inviteKey";
        private const string EMAIL_TO_TEMPLATE_VARIABLE = "emailTo";
#pragma warning restore SA1310 // Restore warnings

        /// <summary>
        /// Initializes a new instance of the <see cref="BetaRequestService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="betaRequestDelegate">The email delegate to interact with the DB.</param>
        /// <param name="emailQueueService">The email service to queue emails.</param>
        public BetaRequestService(ILogger<BetaRequestService> logger, IBetaRequestDelegate betaRequestDelegate, IEmailQueueService emailQueueService)
        {
            this.logger = logger;
            this.betaRequestDelegate = betaRequestDelegate;
            this.emailQueueService = emailQueueService;
        }

        /// <inheritdoc />
        public RequestResult<List<UserBetaRequest>> GetPendingBetaRequests()
        {
            this.logger.LogTrace($"Retrieving pending beta requests");
            DBResult<List<BetaRequest>> pendingBetaRequests = this.betaRequestDelegate.GetPendingBetaRequest();
            this.logger.LogDebug($"Finished retrieving pending requets: {JsonConvert.SerializeObject(pendingBetaRequests)}");
            List<UserBetaRequest> betaRequests = UserBetaRequest.CreateListFromDbModel(pendingBetaRequests.Payload);
            return new RequestResult<List<UserBetaRequest>>()
            {
                ResourcePayload = betaRequests,
                ResultStatus = ResultType.Success,
                TotalResultCount = betaRequests.Count
            };
        }

        /// <inheritdoc />
        public RequestResult<List<string>> SendInvites(List<string> betaRequestIds, string hostUrl)
        {
            this.logger.LogTrace($"Sending invites to beta requests... {JsonConvert.SerializeObject(betaRequestIds)}");

            // Get the requets that still need to be invited
            List<BetaRequest> pendingRequests = this.betaRequestDelegate.GetPendingBetaRequest().Payload;

            List<BetaRequest> requestsToInvite = pendingRequests.Where(b => betaRequestIds.Contains(b.HdId)).ToList();

            RequestResult<List<string>> requestResult = new RequestResult<List<string>>();
            requestResult.ResourcePayload = new List<string>();
            foreach (BetaRequest betaRequest in requestsToInvite)
            {
                EmailInvite invite = new EmailInvite();
                invite.InviteKey = Guid.NewGuid();
                invite.HdId = betaRequest.HdId;
                invite.ExpireDate = DateTime.MaxValue;

                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                keyValues.Add(HOST_TEMPLATE_VARIABLE, hostUrl);
                keyValues.Add(INVITE_KEY_TEMPLATE_VARIABLE, invite.InviteKey.ToString());
                keyValues.Add(EMAIL_TO_TEMPLATE_VARIABLE, betaRequest.EmailAddress);
                invite.Email = this.emailQueueService.ProcessTemplate(betaRequest.EmailAddress, this.emailQueueService.GetEmailTemplate(EmailTemplateName.INVITE_TEMPLATE), keyValues);
                this.emailQueueService.QueueNewInviteEmail(invite);

                requestResult.ResourcePayload.Add(betaRequest.HdId);
                requestResult.ResultStatus = ResultType.Success;
            }

            this.logger.LogDebug("Finished sending beta requests invites.");
            this.logger.LogDebug($"Requets to invite: {JsonConvert.SerializeObject(betaRequestIds)}");
            this.logger.LogDebug($"Invited: {JsonConvert.SerializeObject(requestResult.ResourcePayload)}");
            requestResult.ResultStatus = ResultType.Success;

            return requestResult;
        }
    }
}