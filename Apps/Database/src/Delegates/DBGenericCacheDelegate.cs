//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Entity framework based implementation of the GenericCache delegate.
    /// </summary>
    public class DBGenericCacheDelegate : IGenericCacheDelegate
    {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
        private readonly ILogger<DBGenericCacheDelegate> logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBGenericCacheDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBGenericCacheDelegate(
            ILogger<DBGenericCacheDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<GenericCache> CacheObject(object cacheObject, string hdid, string domain, int expires, bool commit = true)
        {
            string json = JsonSerializer.Serialize(cacheObject, cacheObject.GetType());
            JsonDocument jsonDoc = JsonDocument.Parse(json);
            GenericCache genericCache = new GenericCache()
            {
                HdId = hdid,
                Domain = domain,
                JSON = jsonDoc,
                JSONType = cacheObject.GetType().AssemblyQualifiedName,
                CreatedBy = hdid,
                ExpiryDateTime = DateTime.UtcNow.AddMinutes(expires),
            };
            return this.AddCacheObject(genericCache, commit);
        }

        /// <inheritdoc />
        public T? GetCacheObject<T>(string hdid, string domain)
            where T : class
        {
            T? retVal = null;
            DBResult<GenericCache> cacheObject = this.GetCacheObject(hdid, domain);
            if (cacheObject.Status == DBStatusCode.Read)
            {
                if (cacheObject.Payload != null &&
                    cacheObject.Payload.JSON != null &&
                    cacheObject.Payload.JSONType != null)
                {
                    Type? t = Type.GetType(cacheObject.Payload.JSONType);
                    if (t != null)
                    {
                        try
                        {
                            retVal = (T)JsonSerializer.Deserialize(cacheObject.Payload.JSON.RootElement.GetRawText(), t);
                        }
                        catch (JsonException e)
                        {
                            this.logger.LogError($"Error parsing GenericCache object {cacheObject.Payload.Id} Error = {e.Message}");
                        }
                    }
                }
            }

            return retVal;
        }

        /// <inheritdoc />
        public DBResult<GenericCache> GetCacheObject(string hdid, string typeName)
        {
            DBResult<GenericCache> result = new DBResult<GenericCache>()
            {
                Status = DBStatusCode.NotFound,
            };
            result.Payload = this.dbContext.GenericCache
                                    .Where(p => p.HdId == hdid &&
                                                p.Domain == typeName &&
                                                p.ExpiryDateTime >= DateTime.UtcNow)
                                    .OrderByDescending(o => o.CreatedDateTime)
                                    .FirstOrDefault();
            if (result.Payload != null)
            {
                result.Status = DBStatusCode.Read;
            }

            return result;
        }

        /// <inheritdoc />
        public DBResult<GenericCache> AddCacheObject(GenericCache cacheObject, bool commit = true)
        {
            this.logger.LogTrace("Adding GenericCache object to DB...");
            DBResult<GenericCache> result = new DBResult<GenericCache>()
            {
                Payload = cacheObject,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.GenericCache.Add(cacheObject);
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    this.logger.LogError($"Unable to save note to DB {e.ToString()}");
                    result.Status = DBStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished adding Cache object in DB with result {result.Status}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<GenericCache> UpdateCacheObject(GenericCache cacheObject, bool commit = true)
        {
            this.logger.LogTrace($"Updating GenericCache request in DB...");
            DBResult<GenericCache> result = new DBResult<GenericCache>()
            {
                Payload = cacheObject,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.GenericCache.Update(cacheObject);
            this.dbContext.Entry(cacheObject).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Updated;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished updating cache object in DB with result {result.Status}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<GenericCache> DeleteCacheObject(GenericCache cacheObject, bool commit = true)
        {
            this.logger.LogTrace("Deleting GenericCache object from DB...");
            DBResult<GenericCache> result = new DBResult<GenericCache>()
            {
                Payload = cacheObject,
                Status = DBStatusCode.Deferred,
            };
            this.dbContext.GenericCache.Remove(cacheObject);
            this.dbContext.Entry(cacheObject).Property(p => p.HdId).IsModified = false;
            if (commit)
            {
                try
                {
                    this.dbContext.SaveChanges();
                    result.Status = DBStatusCode.Deleted;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    result.Status = DBStatusCode.Concurrency;
                    result.Message = e.Message;
                }
            }

            this.logger.LogDebug($"Finished deleting Generic cache object in DB with result {result.Status}");
            return result;
        }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
    }
}
