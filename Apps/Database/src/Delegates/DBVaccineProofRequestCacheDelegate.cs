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
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public class DBVaccineProofRequestCacheDelegate : IVaccineProofRequestCacheDelegate
    {
        private readonly ILogger<DBVaccineProofRequestCacheDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBVaccineProofRequestCacheDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBVaccineProofRequestCacheDelegate(
            ILogger<DBVaccineProofRequestCacheDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "GetCache should never throw")]
        public VaccineProofRequestCache? GetCacheItem(string personalIdentifier, VaccineProofTemplate proofTemplate, string shcImageHash)
        {
            VaccineProofRequestCache? cacheItem = null;

            try
            {
                cacheItem = this.dbContext.VaccineProofRequestCache
                                          .Where(p => p.PersonIdentifier == personalIdentifier &&
                                                      p.ProofTemplate == proofTemplate &&
                                                      p.ShcImageHash == shcImageHash &&
                                                      DateTime.UtcNow < p.ExpiryDateTime)
                                          .OrderByDescending(o => o.CreatedBy)
                                          .FirstOrDefault();
            }
            catch (Exception e)
            {
                this.logger.LogWarning($"Error querying VaccineProofRequestCache {e}");
            }

            return cacheItem;
        }

        /// <inheritdoc />
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "AddCache should never throw")]
        public void AddCacheItem(VaccineProofRequestCache cacheItem, bool commit = true)
        {
            this.logger.LogTrace($"Adding VaccineProofRequest cache item to DB...");

            this.dbContext.VaccineProofRequestCache.Add(cacheItem);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    this.logger.LogWarning($"Unable to save cache item to DB {e}");
                }
            }

            this.logger.LogDebug($"Finished adding cache item to DB");
        }
    }
}
