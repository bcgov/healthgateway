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
namespace HealthGateway.Common.Services
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Api;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.PHSA;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <inheritdoc/>
    public class BroadcastService : IBroadcastService
    {
        private readonly ILogger logger;
        private readonly IMapper autoMapper;
        private readonly ISystemBroadcastApi systemBroadcastApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="systemBroadcastApi">The injected API for interacting with system broadcasts.</param>
        /// <param name="autoMapper">The injected automapper provider.</param>
        public BroadcastService(ILogger logger, ISystemBroadcastApi systemBroadcastApi, IMapper autoMapper)
        {
            this.logger = logger;
            this.systemBroadcastApi = systemBroadcastApi;
            this.autoMapper = autoMapper;
        }

        private static ActivitySource Source { get; } = new(nameof(BroadcastService));

        /// <inheritdoc/>
        public async Task<RequestResult<Broadcast>> CreateBroadcastAsync(Broadcast broadcast)
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Creating broadcast");

            RequestResult<Broadcast> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                BroadcastRequest broadcastRequest = this.autoMapper.Map<BroadcastRequest>(broadcast);
                IApiResponse<BroadcastResponse> response = await this.systemBroadcastApi.CreateBroadcast(broadcastRequest).ConfigureAwait(true);

                if (response.StatusCode == HttpStatusCode.OK && response.Error is null && response.Content is not null)
                {
                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = this.autoMapper.Map<Broadcast>(response.Content);
                    requestResult.TotalResultCount = 1;
                }
                else
                {
                    this.logger.LogError("Broadcast request returned unsuccessful response");
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "An unexpected error occurred while processing external call",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Error}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }

            this.logger.LogDebug("Finished creating broadcast");
            return requestResult;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcastsAsync()
        {
            using Activity? activity = Source.StartActivity();
            this.logger.LogDebug("Retrieving broadcasts");

            RequestResult<IEnumerable<Broadcast>> requestResult = new()
            {
                ResultStatus = ResultType.Error,
            };

            try
            {
                IApiResponse<IEnumerable<BroadcastResponse>> response = await this.systemBroadcastApi.GetBroadcasts().ConfigureAwait(true);

                if (response.StatusCode == HttpStatusCode.OK && response.Error is null && response.Content is not null)
                {
                    requestResult.ResultStatus = ResultType.Success;
                    requestResult.ResourcePayload = this.autoMapper.Map<List<Broadcast>>(response.Content);
                    requestResult.TotalResultCount = requestResult.ResourcePayload.Count();
                }
                else
                {
                    this.logger.LogError("Broadcast request returned unsuccessful response");
                    requestResult.ResultError = new()
                    {
                        ResultMessage = "An unexpected error occurred while processing external call",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                    };
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.LogCritical("HTTP Request Exception {Error}", e.ToString());
                requestResult.ResultError = new()
                {
                    ResultMessage = "Error with HTTP Request",
                    ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationExternal, ServiceType.PHSA),
                };
            }

            this.logger.LogDebug("Finished retrieving broadcasts");
            return requestResult;
        }
    }
}
