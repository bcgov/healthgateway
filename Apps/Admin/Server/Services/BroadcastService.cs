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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation.Results;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Validations;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <inheritdoc/>
    public class BroadcastService : IBroadcastService
    {
        private const string HttpRequestError = "Error with HTTP Request";

        private readonly ILogger<BroadcastService> logger;
        private readonly ICommonMappingService commonMappingService;
        private readonly ISystemBroadcastApi systemBroadcastApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="systemBroadcastApi">The injected API for interacting with system broadcasts.</param>
        /// <param name="commonMappingService">The injected common mapping service.</param>
        public BroadcastService(ILogger<BroadcastService> logger, ISystemBroadcastApi systemBroadcastApi, ICommonMappingService commonMappingService)
        {
            this.logger = logger;
            this.systemBroadcastApi = systemBroadcastApi;
            this.commonMappingService = commonMappingService;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Broadcast>> CreateBroadcastAsync(Broadcast broadcast, CancellationToken ct = default)
        {
            RequestResult<Broadcast> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                ValidationResult? validationResults = await new BroadcastValidator().ValidateAsync(broadcast, ct);
                if (!validationResults.IsValid)
                {
                    this.logger.LogDebug("Broadcast failed validation");
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Effective Date should be before Expiry Date",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    };
                }
                else
                {
                    BroadcastRequest broadcastRequest = this.commonMappingService.MapToBroadcastRequest(broadcast);

                    this.logger.LogDebug("Creating broadcast");
                    BroadcastResponse response = await this.systemBroadcastApi.CreateBroadcastAsync(broadcastRequest, ct);

                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = this.commonMappingService.MapToBroadcast(response);
                    requestResult.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Unable to create broadcast");
                requestResult.ResultError = new()
                {
                    ResultMessage = HttpRequestError,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcastsAsync(CancellationToken ct = default)
        {
            RequestResult<IEnumerable<Broadcast>> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Retrieving broadcasts");
                IEnumerable<BroadcastResponse> response = await this.systemBroadcastApi.GetBroadcastsAsync(ct);

                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload = response.Select(this.commonMappingService.MapToBroadcast).ToList();
                requestResult.TotalResultCount = requestResult.ResourcePayload.Count();
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Unable to retrieve broadcast");
                requestResult.ResultError = new()
                {
                    ResultMessage = HttpRequestError,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Broadcast>> UpdateBroadcastAsync(Broadcast broadcast, CancellationToken ct = default)
        {
            RequestResult<Broadcast> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                ValidationResult? validationResults = await new BroadcastValidator().ValidateAsync(broadcast, ct);
                if (!validationResults.IsValid)
                {
                    this.logger.LogDebug("Broadcast failed validation");
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "Effective Date should be before Expiry Date",
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.InvalidState),
                    };
                }
                else
                {
                    BroadcastRequest broadcastRequest = this.commonMappingService.MapToBroadcastRequest(broadcast);

                    this.logger.LogDebug("Updating broadcast");
                    BroadcastResponse response = await this.systemBroadcastApi.UpdateBroadcastAsync(broadcast.Id.ToString(), broadcastRequest, ct);

                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = this.commonMappingService.MapToBroadcast(response);
                    requestResult.TotalResultCount = 1;
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error updating broadcast");
                requestResult.ResultError = new()
                {
                    ResultMessage = HttpRequestError,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<Broadcast>> DeleteBroadcastAsync(Broadcast broadcast, CancellationToken ct = default)
        {
            RequestResult<Broadcast> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                this.logger.LogDebug("Deleting broadcast");
                await this.systemBroadcastApi.DeleteBroadcastAsync(broadcast.Id.ToString(), ct);

                requestResult.ResultStatus = ResultType.Success;
                requestResult.ResourcePayload = broadcast;
                requestResult.TotalResultCount = 1;
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.logger.LogWarning(e, "Error deleting broadcast");
                requestResult.ResultError = new()
                {
                    ResultMessage = HttpRequestError,
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.Phsa),
                };
            }

            return requestResult;
        }
    }
}
