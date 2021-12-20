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
    using System;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CommunicationService : ICommunicationService
    {
        private const string BannerCacheKey = "BannerCacheKey";
        private const string InAppCacheKey = "InAppCacheKey";

        private readonly ILogger logger;
        private readonly ICommunicationDelegate communicationDelegate;
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="communicationDelegate">Injected Note delegate.</param>
        /// <param name="memoryCache">The cache to use to reduce lookups.</param>
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
            this.memoryCache = memoryCache;
        }

        /// <inheritdoc />
        public RequestResult<Communication> GetActiveBanner(CommunicationType communicationType)
        {
            if (communicationType != CommunicationType.Banner && communicationType != CommunicationType.InApp)
            {
                throw new ArgumentOutOfRangeException(nameof(communicationType), "Communiction Type must be Banner or InApp");
            }

            RequestResult<Communication>? cacheEntry = this.GetBannerCache(communicationType);
            if (cacheEntry == null)
            {
                this.logger.LogInformation("Active Communication not found in cache, getting from DB...");
                cacheEntry = new();
                DBResult<Communication> dbResult = this.communicationDelegate.GetActiveBanner(communicationType);
                if (dbResult.Status == DBStatusCode.Read || dbResult.Status == DBStatusCode.NotFound)
                {
                    cacheEntry.ResourcePayload = dbResult.Payload;
                    cacheEntry.ResultStatus = ResultType.Success;
                    cacheEntry.TotalResultCount = dbResult.Status == DBStatusCode.Read ? 1 : 0;
                    this.AddBannerCache(cacheEntry, communicationType);
                }
                else
                {
                    this.logger.LogInformation($"Error getting Communication from DB {dbResult.Message}");
                    cacheEntry.ResultStatus = ResultType.Error;
                    cacheEntry.ResultError = new()
                    {
                        ResultMessage = dbResult.Message,
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    };
                }
            }
            else
            {
                this.logger.LogDebug("Active Banner was found in cache, returning...");
            }

            return cacheEntry;
        }

        /// <inheritdoc />
        public void RemoveBannerCache(CommunicationType cacheType)
        {
            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            this.memoryCache.Remove(cacheKey);
        }

        /// <inheritdoc />
        public RequestResult<Communication>? GetBannerCache(CommunicationType cacheType)
        {
            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            this.memoryCache.TryGetValue(cacheKey, out RequestResult<Communication> cacheEntry);
            return cacheEntry;
        }

        /// <inheritdoc />
        public void AddBannerCache(RequestResult<Communication> cacheEntry, CommunicationType cacheType)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new();
            if (cacheEntry.ResultStatus == ResultType.Success &&
                cacheEntry.ResourcePayload != null)
            {
                this.logger.LogInformation($"Active Banner found, setting expiration to {cacheEntry.ResourcePayload.ExpiryDateTime}");
                cacheEntryOptions.AbsoluteExpiration = new System.DateTimeOffset(cacheEntry.ResourcePayload.ExpiryDateTime);
            }
            else
            {
                this.logger.LogInformation($"No Active Banner found, caching empty result indefinitely");
            }

            string cacheKey = cacheType == CommunicationType.Banner ? BannerCacheKey : InAppCacheKey;
            this.memoryCache.Set(cacheKey, cacheEntry, cacheEntryOptions);
        }
    }
}
