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
using System.Diagnostics.CodeAnalysis;
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
    public async Task SendAsync(IEnumerable<MessageEnvelope> messages, CancellationToken ct = default)
    {
        this.logger.LogDebug("Sending messages over Azure Service Bus");
        IEnumerable<ServiceBusMessage> sbMessages = messages.Select(
            m =>
                new ServiceBusMessage
                {
                    Body = new BinaryData(m.Content.Serialize(false)),
                    SessionId = m.SessionId,
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
            if (!batch.TryAddMessage(message))
            {
                this.logger.LogDebug("Sending {Count} messages in batch {BatchId}", batch.Count, batchId);
                await this.sender.SendMessagesAsync(batch, ct);
                batchId++;
                batch = await this.sender.CreateMessageBatchAsync(ct);
                batch.TryAddMessage(message);
            }

            this.logger.LogDebug("Added message {Type} to batch {BatchId}", message.ApplicationProperties["$type"], batchId);
        }

        if (batch.Count > 0)
        {
            this.logger.LogDebug("Sending {Count} messages in batch {BatchId}", batch.Count, batchId);
            await this.sender.SendMessagesAsync(batch, ct);
        }
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage(Justification = "Non-virtual events can't easily be mocked: https://github.com/Azure/azure-sdk-for-net/issues/35660")]
    public async Task SubscribeAsync(Func<string, IEnumerable<MessageEnvelope>, Task<bool>> receiveHandler, Func<Exception, Task> errorHandler, CancellationToken ct = default)
    {
        this.logger.LogInformation("Subscribing to Azure Service Bus");
        this.sessionProcessor.ProcessMessageAsync += args => this.HandleProcessMessageAsync(args, receiveHandler, errorHandler, ct);
        this.sessionProcessor.ProcessErrorAsync += args => errorHandler(args.Exception);

        if (!this.sessionProcessor.IsProcessing)
        {
            await this.sessionProcessor.StartProcessingAsync(ct);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.sender.DisposeAsync();
        await this.sessionProcessor.DisposeAsync();
    }

    /// <summary>
    /// Handler for processing received messages.
    /// </summary>
    /// <param name="args">
    /// Event arguments specific to the <see cref="ServiceBusReceivedMessage"/> and session that is being
    /// processed.
    /// </param>
    /// <param name="receiveHandler">
    /// Message receive handler that receives a session id and array of messages,
    /// it can return false to reject the messages or true when processed successfully.
    /// </param>
    /// <param name="errorHandler">Error handler that receives the exception when an error occurred.</param>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to handle all exception types thrown from receive handlers")]
    internal async Task HandleProcessMessageAsync(
        ProcessSessionMessageEventArgs args,
        Func<string, IEnumerable<MessageEnvelope>, Task<bool>> receiveHandler,
        Func<Exception, Task> errorHandler,
        CancellationToken ct = default)
    {
        this.logger.LogDebug("Processing received Azure Service Bus message {Session}:{SequenceNumber}", args.Message.SessionId, args.Message.SequenceNumber);

        try
        {
            SessionState? sessionState = (await args.GetSessionStateAsync(ct))?.ToObjectFromJson<SessionState>();
            if (sessionState is { IsFaulted: true } && args.Message.Subject != "unlock")
            {
                // session is blocked, do not process this message and this message is not marked to unblock the session
                throw new InvalidOperationException($"Session {args.SessionId} is in error mode");
            }

            string? typeName = (args.Message.ApplicationProperties["$aqn"] ?? args.Message.ApplicationProperties["$type"])?.ToString();
            if (string.IsNullOrEmpty(typeName))
            {
                throw new InvalidOperationException($"Message {args.Message.SessionId}:{args.Message.SequenceNumber} is missing the type property");
            }

            Type? type = Type.GetType(typeName, true);

            MessageBase message = (MessageBase?)args.Message.Body.ToArray().Deserialize(type) ??
                                  throw new InvalidOperationException($"Message {args.Message.SessionId}:{args.Message.SequenceNumber} failed to deserialize to type {typeName}");

            bool receiveResult = await receiveHandler(args.SessionId, [new MessageEnvelope(message, args.Message.SessionId)]);
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
            this.logger.LogError(e, "Error processing Azure Service Bus message");
            await args.DeadLetterMessageAsync(args.Message, e.Message, e.ToString(), ct);
            await args.SetSessionStateAsync(BinaryData.FromObjectAsJson(new SessionState(true)), ct);
            await errorHandler(e);
        }
    }

    internal sealed record SessionState(bool IsFaulted);
}
