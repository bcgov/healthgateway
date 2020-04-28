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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <inheritdoc />
    public class CommunicationService : ICommunicationService
    {
        private readonly ILogger logger;
        private readonly ICommunicationDelegate communicationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="communicationDelegate">The communication delegate to interact with the DB.</param>
        public CommunicationService(ILogger<UserFeedbackService> logger, ICommunicationDelegate communicationDelegate)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Communication> Add(Communication communication)
        {
            this.logger.LogTrace($"Adding communication... {JsonConvert.SerializeObject(communication)}");

            DBResult<Communication> dbResult = this.communicationDelegate.Add(communication);
            RequestResult<Communication> requestResult = new RequestResult<Communication>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResultMessage = dbResult.Message,
            };
            return requestResult;
        }
    }
}