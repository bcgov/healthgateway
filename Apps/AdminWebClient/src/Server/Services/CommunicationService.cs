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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
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
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Communication> Add(Communication communication)
        {
            this.logger.LogTrace($"Communication recieved:  {JsonConvert.SerializeObject(communication)}");
            if (communication.CommunicationTypeCode == "Email")
            {
                if (communication.Text.Length == 0 || communication.Subject.Length == 0)
                {
                    throw new ArgumentException("One of: Email Subject, Email Content is invalid.");
                }

                communication.EffectiveDateTime = DateTime.Now;
                communication.ExpiryDateTime = DateTime.Now;
            }

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

        /// <inheritdoc />
        public RequestResult<Communication> Update(Communication communication)
        {
            if (ValidateDates(communication.EffectiveDateTime, communication.ExpiryDateTime))
            {
                this.logger.LogTrace($"Updating communication... {JsonConvert.SerializeObject(communication)}");

                DBResult<Communication> dbResult = this.communicationDelegate.Update(communication);
                return new RequestResult<Communication>()
                {
                    ResourcePayload = dbResult.Payload,
                    ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                    ResultMessage = dbResult.Message,
                };
            }
            else
            {
                return new RequestResult<Communication>()
                {
                    ResourcePayload = null,
                    ResultStatus = ResultType.Error,
                    ResultMessage = "Effective Date should be before Expiry Date.",
                };
            }
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<Communication>> GetAll()
        {
            this.logger.LogTrace($"Getting communication entries...");
            DBResult<IEnumerable<Communication>> dBResult = this.communicationDelegate.GetAll();
            RequestResult<IEnumerable<Communication>> requestResult = new RequestResult<IEnumerable<Communication>>()
            {
                ResourcePayload = dBResult.Payload,
                ResultStatus = dBResult.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultMessage = dBResult.Message,
            };
            return requestResult;
        }

        /// <inheritdoc />
        public RequestResult<Communication> Delete(Communication communication)
        {
            if (communication.CommunicationStatusCode == CommunicationStatus.Processed)
            {
                this.logger.LogError($"Processed communication can't be deleted.");
                return new RequestResult<Communication>()
                {
                    ResultStatus = ResultType.Error,
                    ResultMessage = "Processed communication can't be deleted.",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                };
            }

            DBResult<Communication> dbResult = this.communicationDelegate.Delete(communication);
            RequestResult<Communication> result = new RequestResult<Communication>()
            {
                ResourcePayload = dbResult.Payload,
                ResultStatus = dbResult.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultMessage = dbResult.Message,
                ErrorCode = dbResult.Status == DBStatusCode.Deleted ? string.Empty : ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
            };
            return result;
        }

        private static bool ValidateDates(DateTime effectiveDate, DateTime expiryDate)
        {
            if (effectiveDate > expiryDate)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
