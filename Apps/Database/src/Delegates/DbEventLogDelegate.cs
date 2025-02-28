﻿// -------------------------------------------------------------------------
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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dbContext">The context to be used when accessing the database.</param>
    [ExcludeFromCodeCoverage]
    public class DbEventLogDelegate(ILogger<DbEventLogDelegate> logger, GatewayDbContext dbContext) : IEventLogDelegate
    {
        /// <inheritdoc/>
        public async Task<DbResult<EventLog>> WriteEventLogAsync(EventLog eventLog, bool commit = true, CancellationToken ct = default)
        {
            logger.LogDebug("Adding event log to DB");

            DbResult<EventLog> result = new();

            dbContext.Add(eventLog);

            if (commit)
            {
                try
                {
                    await dbContext.SaveChangesAsync(ct);
                    result.Status = DbStatusCode.Created;
                }
                catch (DbUpdateException e)
                {
                    logger.LogError(e, "Error adding event log to DB");
                    result.Status = DbStatusCode.Error;
                    result.Message = e.Message;
                }
            }

            return result;
        }
    }
}
