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
using HealthGateway.Database.Delegates;
using HealthGateway.Database.Models;
using Microsoft.Extensions.Logging;

/// <summary>
/// Outbox store implementation that uses a dedicated Outbox table in the database.
/// </summary>
public class DbOutboxStore : IOutboxStore
{
    private readonly IOutboxQueueDelegate outboxDelegate;
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IMessageSender messageSender;
    private readonly ILogger<DbOutboxStore> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbOutboxStore"/> class.
    /// </summary>
    /// <param name="outboxDelegate">The outbox db delegate.</param>
    /// <param name="backgroundJobClient">Hangfire background job client.</param>
    /// <param name="messageSender">The destination message sender to forward messages to.</param>
    /// <param name="logger">A logger.</param>
    public DbOutboxStore(IOutboxQueueDelegate outboxDelegate, IBackgroundJobClient backgroundJobClient, IMessageSender messageSender, ILogger<DbOutboxStore> logger)
    {
        this.outboxDelegate = outboxDelegate;
        this.backgroundJobClient = backgroundJobClient;
        this.messageSender = messageSender;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task StoreAsync(IEnumerable<MessageEnvelope> messages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Storing messages in the DB");
        IEnumerable<OutboxItem> outboxItems = messages.Select(
            m => new OutboxItem
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
        this.outboxDelegate.Enqueue(outboxItems);
        await this.outboxDelegate.CommitAsync(ct);

        this.logger.LogDebug("Scheduling background job to dispatch messages");
        this.backgroundJobClient.Enqueue<DbOutboxStore>(store => store.DispatchOutboxItemsAsync(CancellationToken.None));
    }

    /// <summary>
    /// Dispatches any outbox messages not sent.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    [Queue(AzureServiceBusSettings.OutboxQueueName)]
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    public async Task DispatchOutboxItemsAsync(CancellationToken ct = default)
    {
        this.logger.LogDebug("Dispatching queued messages");

        try
        {
            IEnumerable<OutboxItem> pendingItems = await this.outboxDelegate.DequeueAsync(ct);

            IEnumerable<MessageEnvelope> messages = pendingItems.Select(
                i =>
                {
                    MessageBase message = (MessageBase)Encoding.UTF8.GetBytes(i.Content).Deserialize(Type.GetType(i.Metadata.AssemblyQualifiedName, true))!;
                    return new MessageEnvelope(message, i.Metadata.SessionId);
                });
            await this.messageSender.SendAsync(messages, ct);
            await this.outboxDelegate.CommitAsync(ct);
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error dispatching queued messages");
            throw;
        }
    }
}
