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
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CommunicationService : ICommunicationService
    {
        private readonly ILogger logger;
        private readonly ICommunicationDelegate communicationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="communicationDelegate">Injected Note delegate.</param>
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Communication> GetActive()
        {
            DBResult<Communication> dbComm = this.communicationDelegate.GetActive();
            RequestResult<Communication> result = new RequestResult<Communication>()
            {
                ResourcePayload = dbComm.Payload,
                ResultStatus = dbComm.Status == Database.Constant.DBStatusCode.Read ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                ResultMessage = dbComm.Message,
            };
            return result;
        }
    }
}
