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
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Messaging.ServiceBus;
using HealthGateway.Common.Utils;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Azure Service Bus messaging implementation.
/// </summary>
internal class AzureServiceBus : IMessageSender, IMessageReceiver, IAsyncDisposable
{
    private readonly ServiceBusSender sender;
    private readonly ServiceBusSessionProcessor sessionProcessor;
    private readonly ILogger<AzureServiceBus> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
    /// </summary>
    /// <param name="serviceBusClientFactory">Azure Service Bus client factory.</param>
    /// <param name="settingsOptions">Settings to configure Azure Service Bus.</param>
    /// <param name="logger">A logger.</param>
    public AzureServiceBus(IAzureClientFactory<ServiceBusClient> serviceBusClientFactory, IOptions<AzureServiceBusSettings> settingsOptions, ILogger<AzureServiceBus> logger)
    {
        this.logger = logger;
        string queueName = settingsOptions.Value.QueueName;
        ServiceBusClient? client = serviceBusClientFactory.CreateClient(queueName);

        this.sender = client.CreateSender(queueName);
        this.sessionProcessor = client.CreateSessionProcessor(queueName);
    }

    /// <inheritdoc/>
    public virtual async Task SendAsync(IEnumerable<MessageEnvelope> messages, CancellationToken ct = default)
    {
        IEnumerable<ServiceBusMessage> sbMessages = messages.Select(
            m =>
                new ServiceBusMessage
                {
                    Body = new BinaryData(m.Content.Serialize(false)),
                    SessionId = m.SessionId ?? string.Empty,
                    ContentType = ContentType.ApplicationJson.ToString(),
                    ApplicationProperties =
                    {
                        { "$type", m.MessageType },
                        { "$createdon", m.CreatedOn },
                        { "$aqn", m.Content.GetType().AssemblyQualifiedName! },
                    },
                });

        ServiceBusMessageBatch? batch = await this.sender.CreateMessageBatchAsync(ct);
        int batchId = 1;
        foreach (ServiceBusMessage message in sbMessages)
        {
            this.logger.LogDebug("adding message {Type} to batch {BatchId}", message.ApplicationProperties["$type"], batchId);
            if (!batch.TryAddMessage(message))
            {
                await this.sender.SendMessagesAsync(batch, ct);
                this.logger.LogDebug("sent {Count} messages in batch {BatchId}", batch.Count, batchId);
                batchId++;
                batch = await this.sender.CreateMessageBatchAsync(ct);
                batch.TryAddMessage(message);
            }
        }

        if (batch.Count > 0)
        {
            await this.sender.SendMessagesAsync(batch, ct);
            this.logger.LogDebug("sent {Count} messages in batch {BatchId}", batch.Count, batchId);
        }
    }

#pragma warning disable CA1031 // Do not catch general exception types - need to handle all exception types thrown from receive handlers

    /// <inheritdoc/>
    public virtual async Task Subscribe(Func<string, IEnumerable<MessageEnvelope>, Task<bool>> receiveHandler, Func<Exception, Task> errorHandler, CancellationToken ct = default)
    {
        this.sessionProcessor.ProcessMessageAsync += async args =>
        {
            this.logger.LogDebug("received message {Session}:{SequenceNumber}", args.Message.SessionId, args.Message.SequenceNumber);

            try
            {
                SessionState? sessionState = (await args.GetSessionStateAsync(ct))?.ToObjectFromJson<SessionState>();
                if (sessionState != null && sessionState.IsFaulted && args.Message.Subject != "unlock")
                {
                    // session is blocked, do not process this message and this message is not marked to unblock the session
                    throw new InvalidOperationException($"Session {args.SessionId} is in error mode");
                }

                string? typeName = (args.Message.ApplicationProperties["$aqn"] ?? args.Message.ApplicationProperties["$type"])?.ToString();
                if (string.IsNullOrEmpty(typeName))
                {
                    throw new InvalidOperationException($"message {args.Message.SessionId}:{args.Message.SequenceNumber} is missing the type property");
                }

                Type? type = Type.GetType(typeName, true);

                MessageBase? message = (MessageBase?)args.Message.Body.ToArray().Deserialize(type);
                if (message == null)
                {
                    throw new InvalidOperationException($"message {args.Message.SessionId}:{args.Message.SequenceNumber} failed to deserialize to type {typeName}");
                }

                bool receiveResult = await receiveHandler(args.SessionId, new[] { new MessageEnvelope(message, args.Message.SessionId) });
                if (!receiveResult)
                {
                    // put back in the queue if the handler rejected the messages
                    await args.AbandonMessageAsync(args.Message, null, ct);
                }

                // clear the state
                if (sessionState != null)
                {
                    await args.SetSessionStateAsync(null, ct);
                }
            }
            catch (Exception e)
            {
                // send message to DLQ and set the state to fault
                this.logger.LogError(e, "failed to receive message");
                await args.DeadLetterMessageAsync(args.Message, e.Message, e.ToString(), ct);
                await args.SetSessionStateAsync(BinaryData.FromObjectAsJson(new SessionState(true)), ct);
                await errorHandler(e);
            }
        };

        this.sessionProcessor.ProcessErrorAsync += args => errorHandler(args.Exception);

        if (!this.sessionProcessor.IsProcessing)
        {
            await this.sessionProcessor.StartProcessingAsync(ct);
        }
    }

#pragma warning restore CA1031 // Do not catch general exception types

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.sender.DisposeAsync();
        await this.sessionProcessor.DisposeAsync();
    }

    private sealed record SessionState(bool IsFaulted);
}
