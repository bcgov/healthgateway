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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling;
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

        private readonly ILogger<CommunicationService> logger;
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
            RequestResult<Communication?>? result = this.GetCommunicationFromCache(communicationType);
            if (result == null)
            {
                try
                {
                    Communication? communication = await this.communicationDelegate.GetNextAsync(communicationType, ct);
                    result = this.AddCommunicationToCache(communication, communicationType);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Error retrieving active communication");
                    result = new()
                    {
                        ResultStatus = ResultType.Error,
                        ResultError = new()
                        {
                            ResultMessage = e.Message,
                            ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        },
                    };
                }
            }

            if (DateTime.UtcNow < result.ResourcePayload?.EffectiveDateTime)
            {
                this.logger.LogDebug("There is no active communication for communication type {CommunicationType}", communicationType);
                result = new()
                {
                    ResultStatus = ResultType.Success,
                    TotalResultCount = 0,
                };
            }

            return result;
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
                _ => throw new NotImplementedException($"Communication type {communicationType} is not supported"),
            };
        }

        private void ProcessChangeUncached(BannerChangeEvent changeEvent, Communication communication)
        {
            this.logger.LogDebug("{Action} change event for communication {CommunicationId} with no communication cached", changeEvent.Action, communication.Id);
            if (changeEvent.Action is Insert or Update && communication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
            }
        }

        private void ProcessChangeSameCommunicationCached(BannerChangeEvent changeEvent, Communication communication)
        {
            this.logger.LogDebug("{Action} change event for communication {CommunicationId} found in cache", changeEvent.Action, communication.Id);

            // in all cases, the old communication is removed from the cache
            this.RemoveCommunicationFromCache(communication.CommunicationTypeCode);

            // for delete actions, an empty result is not cached in place of the old communication, because a future-dated communication may exist
            // for insert and update actions, the updated communication is cached in place of the old communication
            if (changeEvent.Action is Insert or Update && communication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.AddCommunicationToCache(communication, communication.CommunicationTypeCode);
            }
        }

        private void ProcessChangeDifferentCommunicationCached(BannerChangeEvent changeEvent, Communication newCommunication, Communication oldCommunication)
        {
            // check the new communication to see if it is effective earlier and not expired
            if (DateTime.UtcNow < newCommunication.ExpiryDateTime && newCommunication.EffectiveDateTime < oldCommunication.EffectiveDateTime &&
                newCommunication.CommunicationStatusCode is CommunicationStatus.New)
            {
                this.logger.LogDebug("{Action} change event for communication {CommunicationId} is replacing {OldCommunicationId}", changeEvent.Action, newCommunication.Id, oldCommunication.Id);
                this.AddCommunicationToCache(newCommunication, newCommunication.CommunicationTypeCode);
            }
            else
            {
                this.logger.LogDebug("{Action} change event for communication {CommunicationId} being ignored", changeEvent.Action, newCommunication.Id);
            }
        }

        private void RemoveCommunicationFromCache(CommunicationType communicationType)
        {
            string cacheKey = GetCacheKey(communicationType);
            this.logger.LogDebug("Removing communication from cache for {CacheIdentifierType}", communicationType);
            this.cacheProvider.RemoveItem(cacheKey);
        }

        private RequestResult<Communication?>? GetCommunicationFromCache(CommunicationType communicationType)
        {
            string cacheKey = GetCacheKey(communicationType);

            this.logger.LogDebug("Accessing communication cache for {CacheIdentifierType}", communicationType);
            RequestResult<Communication?>? result = this.cacheProvider.GetItem<RequestResult<Communication?>>(cacheKey);

            this.logger.LogDebug("Communication cache access outcome: {CacheResult}", result == null ? "miss" : "hit");
            return result;
        }

        private RequestResult<Communication?> AddCommunicationToCache(Communication? communication, CommunicationType communicationType)
        {
            RequestResult<Communication?> cacheEntry = new()
            {
                ResultStatus = ResultType.Success,
                TotalResultCount = 0,
            };

            DateTime now = DateTime.UtcNow;
            TimeSpan? expiry = null;

            if (communication != null && now < communication.ExpiryDateTime)
            {
                if (now < communication.EffectiveDateTime)
                {
                    // communication is not yet active
                    this.logger.LogDebug("Caching empty communication until {EffectiveDateTime}", communication.EffectiveDateTime);
                    expiry = communication.EffectiveDateTime - now;
                }
                else
                {
                    // communication is active
                    this.logger.LogDebug("Caching communication {CommunicationId} until {ExpiryDateTime}", communication.Id, communication.ExpiryDateTime);
                    expiry = communication.ExpiryDateTime - now;
                    cacheEntry.TotalResultCount = 1;
                    cacheEntry.ResourcePayload = communication;
                }
            }
            else
            {
                // communication is expired or null
                this.logger.LogDebug("Caching empty communication with no expiration");
            }

            string cacheKey = GetCacheKey(communicationType);
            this.logger.LogDebug("Storing communication in cache for {CacheIdentifierType}", communicationType);
            this.cacheProvider.AddItem(cacheKey, cacheEntry, expiry);
            return cacheEntry;
        }
    }
}
