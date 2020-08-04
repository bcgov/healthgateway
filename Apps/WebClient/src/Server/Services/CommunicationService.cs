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
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class CommunicationService : ICommunicationService
    {
        private const string BannerCacheKey = "BannerCacheKey";

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
        public RequestResult<Communication> GetActiveBanner()
        {
            RequestResult<Communication> cacheEntry;

            if (!this.memoryCache.TryGetValue(BannerCacheKey, out cacheEntry))
            {
                this.logger.LogInformation("Active Banner not found in cache, getting from DB...");
                DBResult<Communication> dbComm = this.communicationDelegate.GetActiveBanner();
                cacheEntry = new RequestResult<Communication>()
                {
                    ResourcePayload = dbComm.Payload,
                    ResultStatus = dbComm.Status == Database.Constants.DBStatusCode.Read ? Common.Constants.ResultType.Success : Common.Constants.ResultType.Error,
                    ResultMessage = dbComm.Message,
                };

                this.SetActiveBannerCache(cacheEntry);
            }
            else
            {
                this.logger.LogDebug("Active Banner was found in cache, returning...");
            }

            return cacheEntry;
        }

        /// <inheritdoc />
        public void SetActiveBannerCache(RequestResult<Communication> cacheEntry)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions();
            if (cacheEntry.ResultStatus == Common.Constants.ResultType.Success &&
                cacheEntry.ResourcePayload != null)
            {
                this.logger.LogInformation($"Active Banner found, setting expiration to {cacheEntry.ResourcePayload.ExpiryDateTime}");
                cacheEntryOptions.AbsoluteExpiration = new System.DateTimeOffset(cacheEntry.ResourcePayload.ExpiryDateTime);
            }
            else
            {
                this.logger.LogInformation($"No Active Banner found, caching empty result indefinitely");
            }

            this.memoryCache.Set(BannerCacheKey, cacheEntry, cacheEntryOptions);
        }
    }
}
