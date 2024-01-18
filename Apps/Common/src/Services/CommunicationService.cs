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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class CommunicationService : ICommunicationService
    {
        /// <summary>
        /// The Cache key used to store public banner communications.
        /// </summary>
        public const string BannerCacheKey = "Communication:Banner";

        /// <summary>
        /// The Cache key used to store in-app banner communications.
        /// </summary>
        public const string InAppCacheKey = "Communication:InApp";

        /// <summary>
        /// The Cache key used to store mobile communications.
        /// </summary>
        public const string MobileCacheKey = "Communication:Mobile";

        private const string Update = "UPDATE";
        private const string Insert = "INSERT";

        private readonly ILogger logger;
        private readonly ICommunicationDelegate communicationDelegate;
        private readonly ICacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="communicationDelegate">Injected Note delegate.</param>
        /// <param name="cacheProvider">The cache to use to reduce lookups.</param>
        public CommunicationService(ILogger<CommunicationService> logger, ICommunicationDelegate communicationDelegate, ICacheProvider cacheProvider)
        {
            this.logger = logger;
            this.communicationDelegate = communicationDelegate;
            this.cacheProvider = cacheProvider;
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        public async Task<RequestResult<Communication?>> GetActiveCommunicationAsync(CommunicationType communicationType, CancellationToken ct = default)
        {
            RequestResult<Communication?>? cacheEntry = new()
            {
                ResourcePayload = await this.communicationDelegate.GetNextAsync(communicationType, ct),
                ResultStatus = ResultType.Success,
            };
            /* RequestResult<Communication?>? cacheEntry = this.GetCommunicationFromCache(communicationType);
            if (cacheEntry == null)
            {
                this.logger.LogInformation("Active Communication not found in cache, getting from DB...");
                try
                {
                    Communication? communication = await this.communicationDelegate.GetNextAsync(communicationType, ct);
                    cacheEntry = this.AddCommunicationToCache(communication, communicationType);
                }
                catch (Exception e)
                {
                    this.logger.LogError("Error getting Communication from DB {Error}", e.ToString());
                    cacheEntry = new()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new()
                        {
                            ResultMessage = e.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                    };
                }
            } */

            if (DateTime.UtcNow < cacheEntry.ResourcePayload?.EffectiveDateTime)
            {
                this.logger.LogDebug("Banner is future dated, returning empty RequestResult");
                cacheEntry = new()
                {
                    ResultStatus = ResultType.Success,
                    TotalResultCount = 0,
                };
            }

            return cacheEntry;
        }

        /// <inheritdoc/>
        public void ProcessChange(BannerChangeEvent changeEvent)
        {
            Communication communication = changeEvent.Data;

            RequestResult<Communication?>? cacheEntry = this.GetCommunicationFromCache(communication.CommunicationTypeCode);
            Communication? cachedComm = cacheEntry?.ResourcePayload;
            if (cachedComm == null)
            {
                this.ProcessChangeUncached(changeEvent, communication);
            }
            else if (cachedComm.Id == communication.Id)
            {
                this.ProcessChangeSameCommunicationCached(changeEvent, communication);
            }
            else
            {
                this.ProcessChangeDifferentCommunicationCached(changeEvent, communication, cachedComm);
            }
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            this.RemoveCommunicationFromCache(CommunicationType.Banner);
            this.RemoveCommunicationFromCache(CommunicationType.InApp);
            this.RemoveCommunicationFromCache(CommunicationType.Mobile);
        }

        /// <summary>
        /// Retrieves the key for the cache associated with a given CommunicationType. Only Banner, InApp, and Mobile
        /// CommunicationTypes are supported.
        /// </summary>
        /// <param name="communicationType">The CommunicationType to retrieve the key for.</param>
        /// <returns>The key for the cache associated with the given CommunicationType.</returns>
        internal static string GetCacheKey(CommunicationType communicationType)
        {
            return communicationType switch
            {
                CommunicationType.Banner => BannerCacheKey,
                CommunicationType.InApp => InAppCacheKey,
                CommunicationType.Mobile => MobileCacheKey,
                _ => throw new NotImplementedException($"CommunicationType {communicationType} is not supported"),
            };
        }

        private void ProcessChangeUncached(BannerChangeEvent changeEvent, Communication communication)
        {
            this.logger.LogInformation("No Communications in the Cache, processing {Action} for communication {Id}", changeEvent.Action, communication.Id);
            if (changeEvent.Action is Insert or Update && communication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
            }
        }

        private void ProcessChangeSameCommunicationCached(BannerChangeEvent changeEvent, Communication communication)
        {
            this.logger.LogInformation("{Action} ChangeEvent for Communication {Id} found in Cache", changeEvent.Action, communication.Id);
            this.RemoveCommunicationFromCache(communication.CommunicationTypeCode);
            if (changeEvent.Action is Insert or Update && communication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
            }
            else
            {
                // Delete: We don't cache the empty result as a future dated comm may exist and the next call to GetActiveBanner will find it.
                this.logger.LogInformation("{Action} ChangeEvent for Communication {Id} was processed and removed from Cache", changeEvent.Action, communication.Id);
            }
        }

        private void ProcessChangeDifferentCommunicationCached(BannerChangeEvent changeEvent, Communication newCommunication, Communication oldCommunication)
        {
            // Check the new comm to see if it is effective earlier and not expired
            if (DateTime.UtcNow < newCommunication.ExpiryDateTime && newCommunication.EffectiveDateTime < oldCommunication.EffectiveDateTime &&
                newCommunication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.logger.LogInformation("{Action} ChangeEvent for Communication {Id} replacing {CachedId}", changeEvent.Action, newCommunication.Id, oldCommunication.Id);
                this.AddCommunicationToCache(newCommunication, newCommunication.CommunicationTypeCode);
            }
            else
            {
                this.logger.LogInformation("{Action} ChangeEvent for Communication {Id} being ignored", changeEvent.Action, newCommunication.Id);
            }
        }

        private void RemoveCommunicationFromCache(CommunicationType communicationType)
        {
            string cacheKey = GetCacheKey(communicationType);
            this.cacheProvider.RemoveItem(cacheKey);
        }

        private RequestResult<Communication?>? GetCommunicationFromCache(CommunicationType communicationType)
        {
            string cacheKey = GetCacheKey(communicationType);
            return this.cacheProvider.GetItem<RequestResult<Communication?>>(cacheKey);
        }

        private RequestResult<Communication?> AddCommunicationToCache(Communication? communication, CommunicationType communicationType)
        {
            RequestResult<Communication?> cacheEntry = new()
            {
                ResourcePayload = communication,
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
            };
            DateTime now = DateTime.UtcNow;
            TimeSpan? expiry = null;
            if (communication != null && now < communication.ExpiryDateTime)
            {
                if (now < communication.EffectiveDateTime)
                {
                    this.logger.LogInformation("Communication {Id} is not effective, cached empty communication until {EffectiveDateTime}", communication.Id, communication.EffectiveDateTime);
                    expiry = communication.EffectiveDateTime - now;
                }
                else
                {
                    this.logger.LogInformation("Caching communication {Id} until {ExpiryDateTime}", communication.Id, communication.EffectiveDateTime);
                    expiry = communication.ExpiryDateTime - now;
                    cacheEntry.TotalResultCount = 1;
                }
            }
            else
            {
                this.logger.LogInformation("Communication is expired or null, caching Empty RequestResult forever");
                cacheEntry.ResourcePayload = null;
            }

            string cacheKey = GetCacheKey(communicationType);
            this.cacheProvider.AddItem(cacheKey, cacheEntry, expiry);
            return cacheEntry;
        }
    }
}
