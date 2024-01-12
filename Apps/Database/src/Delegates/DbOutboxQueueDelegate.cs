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

/// <summary>
/// Implements IOutboxDelegate
/// </summary>
[ExcludeFromCodeCoverage]
public class DbOutboxQueueDelegate : IOutboxQueueDelegate
{
    private readonly GatewayDbContext dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbOutboxQueueDelegate"/> class
    /// </summary>
    /// <param name="dbContext">A Gateway DB Context instance</param>
    public DbOutboxQueueDelegate(GatewayDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await this.dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<OutboxItem>> DequeueAsync(CancellationToken ct = default)
    {
        List<OutboxItem> results = await this.dbContext.Outbox.OrderBy(i => i.CreatedOn).ToListAsync(ct);
        this.dbContext.RemoveRange(results);

        return results;
    }

    /// <inheritdoc/>
    public void Enqueue(IEnumerable<OutboxItem> items)
    {
        this.dbContext.AddRange(items);
    }
}
