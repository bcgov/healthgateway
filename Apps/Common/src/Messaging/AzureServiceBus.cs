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

namespace HealthGateway.Common.Messaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure.Core;
    using Azure.Messaging.ServiceBus;
    using HealthGateway.Common.Utils;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Azure Service Bus implementation.
    /// </summary>
    internal class AzureServiceBus : IMessageBus, IAsyncDisposable
    {
        private readonly ServiceBusClient serviceBusClient;
        private readonly ILogger<AzureServiceBus> logger;
        private readonly ConcurrentDictionary<string, ServiceBusSender> senders = new ConcurrentDictionary<string, ServiceBusSender>();
        private readonly ConcurrentDictionary<string, ServiceBusProcessor> processors = new ConcurrentDictionary<string, ServiceBusProcessor>();
        private readonly ConcurrentDictionary<string, ServiceBusSessionProcessor> sessionProcessors = new ConcurrentDictionary<string, ServiceBusSessionProcessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBus"/> class.
        /// </summary>
        /// <param name="serviceBusClient">Azure service bus client</param>
        /// <param name="logger">Logger</param>
        public AzureServiceBus(ServiceBusClient serviceBusClient, ILogger<AzureServiceBus> logger)
        {
            this.serviceBusClient = serviceBusClient;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task Send(string queue, IEnumerable<Message> messages, CancellationToken ct)
        {
            using var scope = this.logger.BeginScope("Send");
            var sender = this.GetSender(queue);
            var sbMessages = messages.Select(m =>
                new ServiceBusMessage()
                {
                    Body = new BinaryData(m.Serialize(false)),
                    SessionId = m.SessionId ?? string.Empty,
                    ContentType = ContentType.ApplicationJson.ToString(),
                });

            ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync(ct);
            foreach (ServiceBusMessage message in sbMessages)
            {
                this.logger.LogDebug("sending message {Body}", message.Body.ToString());
                if (!batch.TryAddMessage(message))
                {
                    await sender.SendMessagesAsync(batch, ct);
                    this.logger.LogDebug("sent {Count} messages", batch.Count);
                    batch = await sender.CreateMessageBatchAsync(ct);
                    batch.TryAddMessage(message);
                }
            }

            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch, ct);
                this.logger.LogDebug("sent {Count} messages", batch.Count);
            }
        }

        /// <inheritdoc/>
        public async Task Subscribe(string queue, Func<Message, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct)
        {
            var processor = this.GetProcessor(queue);
            processor.ProcessMessageAsync += async args =>
            {
                using var scope = this.logger.BeginScope("Receive");
                try
                {
                    this.logger.LogDebug("received message {Session}:{SequenceNumber}", args.Message.SessionId, args.Message.SequenceNumber);
                    await messageHandler(args.Message.Body.ToArray().Deserialize<Message>());
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Failed to receive message");
                    await args.DeadLetterMessageAsync(args.Message, e.Message, e.ToString(), ct);
                }
            };
            processor.ProcessErrorAsync += args => errorHandler(args.Exception);
            if (!processor.IsProcessing) await processor.StartProcessingAsync(ct);
        }

        /// <inheritdoc/>
        public async Task Subscribe(string queue, string sessionId, Func<Message, string, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct)
        {
            var sessionProcessor = string.IsNullOrEmpty(sessionId)
                ? this.GetSessionProcessor(queue, new ServiceBusSessionProcessorOptions())
                : this.GetSessionProcessor(queue, new ServiceBusSessionProcessorOptions { SessionIds = { sessionId } });

            sessionProcessor.ProcessMessageAsync += async args =>
            {
                using var scope = this.logger.BeginScope("Receive");
                this.logger.LogDebug("received message {Session}:{SequenceNumber}", args.Message.SessionId, args.Message.SequenceNumber);
                var sessionState = (await args.GetSessionStateAsync(ct))?.ToObjectFromJson<SessionState>();

                try
                {
                    if (sessionState != null && sessionState.IsFaulted && args.Message.Subject != "unlock")
                    {
                        //session is blocked, do not process this message
                        throw new InvalidOperationException($"Session {args.SessionId} is in error mode");
                    }
                    await messageHandler(args.Message.Body.ToArray().Deserialize<Message>(), args.SessionId);
                    // clear the state
                    if (sessionState != null) await args.SetSessionStateAsync(null, ct);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, "Failed to receive message");
                    // send message to DLQ and set the state to fault
                    await args.DeadLetterMessageAsync(args.Message, e.Message, e.ToString(), ct);
                    await args.SetSessionStateAsync(BinaryData.FromObjectAsJson(new SessionState(true)), ct);
                    await errorHandler(e);
                }
            };
            sessionProcessor.ProcessErrorAsync += args => errorHandler(args.Exception);
            if (!sessionProcessor.IsProcessing) await sessionProcessor.StartProcessingAsync(ct);
        }

        private ServiceBusSender GetSender(string queue) => this.senders.GetOrAdd(queue, this.serviceBusClient.CreateSender);

        private ServiceBusProcessor GetProcessor(string queue, ServiceBusProcessorOptions? options = null) =>
            this.processors.GetOrAdd(queue, q => this.serviceBusClient.CreateProcessor(q, options));

        private ServiceBusSessionProcessor GetSessionProcessor(string queue, ServiceBusSessionProcessorOptions? options = null) =>
            this.sessionProcessors.GetOrAdd(queue, q => this.serviceBusClient.CreateSessionProcessor(q, options));

        public async ValueTask DisposeAsync()
        {
            foreach (var sender in this.senders)
            {
                await sender.Value.DisposeAsync();
            }

            foreach (var processor in this.processors)
            {
                await processor.Value.DisposeAsync();
            }

            foreach (var sessionProcessor in this.sessionProcessors)
            {
                await sessionProcessor.Value.DisposeAsync();
            }

            await this.serviceBusClient.DisposeAsync();
        }

        private record SessionState(bool IsFaulted);
    }
}
