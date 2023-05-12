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

namespace HealthGateway.Common.Messaging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using HealthGateway.Common.Utils;
using HealthGateway.Database.Context;
using HealthGateway.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Outbox store implementation that uses a dedicated Outbox table in the database.
/// </summary>
internal class DbOutboxStore : IOutboxStore
{
    private readonly GatewayDbContext dbContext;
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IMessageSender messageSender;
    private readonly ILogger<DbOutboxStore> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbOutboxStore"/> class.
    /// </summary>
    /// <param name="dbContext">The EF database context.</param>
    /// <param name="backgroundJobClient">Hangfire background job client.</param>
    /// <param name="messageSender">The destination message sender to forward messages to.</param>
    /// <param name="logger">A logger.</param>
    public DbOutboxStore(GatewayDbContext dbContext, IBackgroundJobClient backgroundJobClient, IMessageSender messageSender, ILogger<DbOutboxStore> logger)
    {
        this.dbContext = dbContext;
        this.backgroundJobClient = backgroundJobClient;
        this.messageSender = messageSender;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task StoreAsync(IEnumerable<MessageEnvelope> messages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Storing messages in the DB");
        var outboxItems = messages.Select(m => new OutboxItem
        {
            Content = Encoding.UTF8.GetString(m.Content.Serialize()),
            Metadata = new OutboxItemMetadata
            {
                CreatedOn = m.CreatedOn,
                Type = m.MessageType,
                SessionId = m.SessionId,
                AssemblyQualifiedName = m.Content.GetType().AssemblyQualifiedName!,
            },
        });
        await this.dbContext.AddRangeAsync(outboxItems, ct);
        await this.dbContext.SaveChangesAsync(ct);

        this.backgroundJobClient.Enqueue<DbOutboxStore>(store => store.DispatchOutboxItems(CancellationToken.None));
    }

    /// <summary>
    /// Dispatches any outbox messages not sent.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    [Queue(AzureServiceBusSettings.OutboxQueueName)]
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    public async Task DispatchOutboxItems(CancellationToken ct = default)
    {
        this.logger.LogDebug("Forwarding messages to destination");

        using var tx = await this.dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);

        try
        {
            var pendingItems = await this.dbContext.Outbox.OrderBy(i => i.CreatedOn).ToListAsync(ct);
            this.dbContext.RemoveRange(pendingItems);
            await this.dbContext.SaveChangesAsync(ct);

            var messages = pendingItems.Select(i =>
            {
                var message = (MessageBase)Encoding.UTF8.GetBytes(i.Content).Deserialize(Type.GetType(i.Metadata.AssemblyQualifiedName, true))!;
                return new MessageEnvelope(message, i.Metadata.SessionId);
            });
            await this.messageSender.SendAsync(messages, ct);
            await tx.CommitAsync(ct);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Failed to dispatch outbox items");
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}