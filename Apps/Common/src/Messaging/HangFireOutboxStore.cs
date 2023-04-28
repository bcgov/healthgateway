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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using HealthGateway.Common.Utils;
using Microsoft.Extensions.Logging;

/// <summary>
/// Hangfire Outbox store.
/// </summary>
internal class HangFireOutboxStore : IOutboxStore
{
    private readonly IBackgroundJobClient backgroundJobClient;
    private readonly IMessageSender messageSender;
    private readonly ILogger<HangFireOutboxStore> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HangFireOutboxStore"/> class.
    /// </summary>
    /// <param name="backgroundJobClient">Hangfire background job client.</param>
    /// <param name="messageSender">The message sender to forward messages to.</param>
    /// <param name="logger">A logger.</param>
    public HangFireOutboxStore(IBackgroundJobClient backgroundJobClient, IMessageSender messageSender, ILogger<HangFireOutboxStore> logger)
    {
        this.backgroundJobClient = backgroundJobClient;
        this.messageSender = messageSender;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task StoreAsync(IEnumerable<MessageEnvelope> messages, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        this.logger.LogDebug("Storing messages");
        var serializedMessages = messages.Serialize(false);
        this.backgroundJobClient.Enqueue(() => this.ForwardAsync(serializedMessages, CancellationToken.None));
    }

    /// <summary>
    /// Forwards stored messages to the destination messaging middleware.
    /// </summary>
    /// <param name="serializedMessages">The serialized messages to forward.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    [Queue(AzureServiceBusSettings.OutboxQueueName)]
    public async Task ForwardAsync(byte[] serializedMessages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Forwarding messages");
        var messages = serializedMessages.Deserialize<IEnumerable<MessageEnvelope>>();
        await this.messageSender.SendAsync(messages, ct);
    }
}
