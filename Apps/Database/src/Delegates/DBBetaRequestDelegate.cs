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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.Database.Constant;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class DBBetaRequestDelegate : IBetaRequestDelegate
    {
        private readonly ILogger logger;
        private readonly GatewayDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBBetaRequestDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="dbContext">The context to be used when accessing the database.</param>
        public DBBetaRequestDelegate(
            ILogger<DBBetaRequestDelegate> logger,
            GatewayDbContext dbContext)
        {
            this.logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public DBResult<BetaRequest> InsertBetaRequest(BetaRequest betaRequest)
        {
            this.logger.LogTrace($"Inserting beta request to DB... {JsonSerializer.Serialize(betaRequest)}");
            DBResult<BetaRequest> result = new DBResult<BetaRequest>();
            this.dbContext.Add<BetaRequest>(betaRequest);
            try
            {
                this.dbContext.SaveChanges();
                result.Status = DBStatusCode.Created;
            }
            catch (DbUpdateException e)
            {
                result.Status = DBStatusCode.Error;
                result.Message = e.Message;
            }

            this.logger.LogDebug($"Finished inserting beta request to DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<BetaRequest> UpdateBetaRequest(BetaRequest betaRequest)
        {
            this.logger.LogTrace($"Updating beta request in DB... {JsonSerializer.Serialize(betaRequest)}");
            DBResult<BetaRequest> result = this.GetBetaRequest(betaRequest.HdId);
            if (result.Status == DBStatusCode.Read)
            {
                // Copy certain attributes into the fetched Beta Request
                result.Payload.EmailAddress = betaRequest.EmailAddress;
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

            this.logger.LogDebug($"Finished updating beta request in DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<BetaRequest> GetBetaRequest(string hdId)
        {
            this.logger.LogTrace($"Getting the beta request from DB... {hdId}");
            DBResult<BetaRequest> result = new DBResult<BetaRequest>();
            BetaRequest betaRequest = this.dbContext.BetaRequest.Find(hdId);
            result.Payload = betaRequest;
            result.Status = betaRequest != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            this.logger.LogDebug($"Finished getting the beta request from DB. {JsonSerializer.Serialize(result)}");
            return result;
        }

        /// <inheritdoc />
        public DBResult<List<BetaRequest>> GetPendingBetaRequest()
        {
            this.logger.LogTrace($"Getting pending beta requests from DB...");
            DBResult<List<BetaRequest>> result = new DBResult<List<BetaRequest>>();
            List<BetaRequest> betaRequests = this.dbContext.BetaRequest.Where(b => this.dbContext.EmailInvite.Select(e => e.HdId).Contains(b.HdId)).ToList();
            result.Payload = betaRequests;
            result.Status = betaRequests != null ? DBStatusCode.Read : DBStatusCode.NotFound;
            this.logger.LogDebug($"Finished getting pending beta request from DB. {JsonSerializer.Serialize(result)}");
            return result;
        }
    }
}
