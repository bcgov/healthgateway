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
    using System.Text.Json;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
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
        /// <param name="communicationDelegate">The communication delegate to interact with the DB.</param>
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
        }

        /// <inheritdoc />
        public RequestResult<Communication> Add(Communication communication)
        {
            this.logger.LogTrace($"Communication recieved:  {JsonSerializer.Serialize(communication)}");
            if (communication.CommunicationTypeCode == CommunicationType.Email)
            {
                if (communication.Text.Length == 0 || communication.Subject.Length == 0)
                {
                    throw new ArgumentException("One of: Email Subject, Email Content is invalid.");
                }

                communication.EffectiveDateTime = DateTime.Now;
                communication.ExpiryDateTime = DateTime.Now;
            }

            this.logger.LogTrace($"Adding communication... {JsonSerializer.Serialize(communication)}");

            if (communication.EffectiveDateTime < communication.ExpiryDateTime)
            {
                this.logger.LogTrace($"Adding communication... {JsonSerializer.Serialize(communication)}");
                DBResult<HealthGateway.Database.Models.Communication> dbResult = this.communicationDelegate.Add(communication.ToDbModel());
                return new RequestResult<Communication>()
                {
                    ResourcePayload = new Communication(dbResult.Payload),
                    ResultStatus = dbResult.Status == DBStatusCode.Created ? ResultType.Success : ResultType.Error,
                    ResultError = dbResult.Status == DBStatusCode.Created ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                };
            }
            else
            {
                return new RequestResult<Communication>()
                {
                    ResourcePayload = null,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Effective Date should be before Expiry Date.", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }
        }

        /// <inheritdoc />
        public RequestResult<Communication> Update(Communication communication)
        {
            if (communication.EffectiveDateTime < communication.ExpiryDateTime)
            {
                this.logger.LogTrace($"Updating communication... {JsonSerializer.Serialize(communication)}");

                DBResult<HealthGateway.Database.Models.Communication> dbResult = this.communicationDelegate.Update(communication.ToDbModel());
                return new RequestResult<Communication>()
                {
                    ResourcePayload = new Communication(dbResult.Payload),
                    ResultStatus = dbResult.Status == DBStatusCode.Updated ? ResultType.Success : ResultType.Error,
                    ResultError = dbResult.Status == DBStatusCode.Updated ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
                };
            }
            else
            {
                return new RequestResult<Communication>()
                {
                    ResourcePayload = null,
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError() { ResultMessage = "Effective Date should be before Expiry Date.", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }
        }

        /// <inheritdoc />
        public RequestResult<IEnumerable<Communication>> GetAll()
        {
            this.logger.LogTrace($"Getting communication entries...");
            DBResult<IEnumerable<HealthGateway.Database.Models.Communication>> dbResult = this.communicationDelegate.GetAll();
            RequestResult<IEnumerable<Communication>> requestResult = new RequestResult<IEnumerable<Communication>>()
            {
                ResourcePayload = Communication.FromDbModelIEnumerable(dbResult.Payload),
                ResultStatus = dbResult.Status == DBStatusCode.Read ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Read ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
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
                    ResultError = new RequestResultError() { ResultMessage = "Processed communication can't be deleted.", ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState) },
                };
            }

            DBResult<HealthGateway.Database.Models.Communication> dbResult = this.communicationDelegate.Delete(communication.ToDbModel());
            RequestResult<Communication> result = new RequestResult<Communication>()
            {
                ResourcePayload = new Communication(dbResult.Payload),
                ResultStatus = dbResult.Status == DBStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DBStatusCode.Deleted ? null : new RequestResultError() { ResultMessage = dbResult.Message, ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) },
            };
            return result;
        }
    }
}
