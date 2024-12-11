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

namespace HealthGateway.Database.Delegates;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Database.Context;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implements IOutboxQueueDelegate.
/// </summary>
/// <param name="logger">The injected logger.</param>
/// <param name="dbContext">The context to be used when accessing the database.</param>
[ExcludeFromCodeCoverage]
public class DbOutboxQueueDelegate(ILogger<DbOutboxQueueDelegate> logger, GatewayDbContext dbContext) : IOutboxQueueDelegate
{
    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Saving DB changes");
        await dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OutboxItem>> DequeueAsync(CancellationToken ct = default)
    {
        logger.LogDebug("Dequeuing outbox items from DB");
        List<OutboxItem> results = await dbContext.Outbox.OrderBy(i => i.CreatedOn).ToListAsync(ct);
        dbContext.RemoveRange(results);

        return results;
    }

    /// <inheritdoc/>
    public void Enqueue(IEnumerable<OutboxItem> items)
    {
        logger.LogDebug("Enqueuing outbox items in DB");
        dbContext.AddRange(items);
    }
}
