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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">Injected Logger Provider.</param>
    /// <param name="communicationDelegate">The communication delegate to interact with the DB.</param>
    /// <param name="mappingService">The injected mapping service.</param>
    public class CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate, IAdminServerMappingService mappingService) : ICommunicationService
    {
        /// <inheritdoc/>
        public async Task<RequestResult<Communication>> AddAsync(Communication communication, CancellationToken ct = default)
        {
            logger.LogTrace("Communication received: {Id)}", communication.Id.ToString());

            if (communication.EffectiveDateTime < communication.ExpiryDateTime)
            {
                logger.LogTrace("Adding communication... {Id)}", communication.Id.ToString());
                DbResult<Database.Models.Communication> dbResult = await communicationDelegate.AddAsync(mappingService.MapToDatabaseCommunication(communication), ct: ct);
                return new RequestResult<Communication>
                {
                    ResourcePayload = mappingService.MapToCommonCommunication(dbResult.Payload),
                    ResultStatus = dbResult.Status == DbStatusCode.Created ? ResultType.Success : ResultType.Error,
                    ResultError = dbResult.Status == DbStatusCode.Created
                        ? null
                        : new RequestResultError
                        {
                            ResultMessage = dbResult.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                };
            }

            return new RequestResult<Communication>
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "Effective Date should be before Expiry Date.",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                },
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Communication>> UpdateAsync(Communication communication, CancellationToken ct = default)
        {
            if (communication.EffectiveDateTime < communication.ExpiryDateTime)
            {
                logger.LogTrace("Updating communication... {Id)}", communication.Id.ToString());

                DbResult<Database.Models.Communication> dbResult = await communicationDelegate.UpdateAsync(mappingService.MapToDatabaseCommunication(communication), ct: ct);
                return new RequestResult<Communication>
                {
                    ResourcePayload = mappingService.MapToCommonCommunication(dbResult.Payload),
                    ResultStatus = dbResult.Status == DbStatusCode.Updated ? ResultType.Success : ResultType.Error,
                    ResultError = dbResult.Status == DbStatusCode.Updated
                        ? null
                        : new RequestResultError
                        {
                            ResultMessage = dbResult.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                };
            }

            return new RequestResult<Communication>
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Error,
                ResultError = new RequestResultError
                {
                    ResultMessage = "Effective Date should be before Expiry Date.",
                    ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                },
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<Communication>>> GetAllAsync(CancellationToken ct = default)
        {
            logger.LogTrace("Getting communication entries...");
            IList<Database.Models.Communication> communications = await communicationDelegate.GetAllAsync(ct);
            return new()
            {
                ResourcePayload = communications.Select(mappingService.MapToCommonCommunication).ToList(),
                ResultStatus = ResultType.Success,
                ResultError = null,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Communication>> DeleteAsync(Communication communication, CancellationToken ct = default)
        {
            if (communication.CommunicationStatusCode == CommunicationStatus.Processed)
            {
                logger.LogError("Processed communication can't be deleted");
                return new RequestResult<Communication>
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new RequestResultError
                    {
                        ResultMessage = "Processed communication can't be deleted.",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    },
                };
            }

            DbResult<Database.Models.Communication> dbResult = await communicationDelegate.DeleteAsync(mappingService.MapToDatabaseCommunication(communication), ct: ct);
            RequestResult<Communication> result = new()
            {
                ResourcePayload = mappingService.MapToCommonCommunication(dbResult.Payload),
                ResultStatus = dbResult.Status == DbStatusCode.Deleted ? ResultType.Success : ResultType.Error,
                ResultError = dbResult.Status == DbStatusCode.Deleted
                    ? null
                    : new RequestResultError
                    {
                        ResultMessage = dbResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    },
            };
            return result;
        }
    }
}
