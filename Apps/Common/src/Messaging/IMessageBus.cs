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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Utils;

    /// <summary>
    /// Message bus interface
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Sends a message over the bus
        /// </summary>
        /// <param name="queue">The queue name</param>
        /// <param name="messages">Messages to send</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        Task Send(string queue, IEnumerable<Message> messages, CancellationToken ct);

        /// <summary>
        /// Subscribes a handler to a queue/topic
        /// </summary>
        /// <param name="queue">The queue to subscribe to</param>
        /// <param name="messageHandler">The handler to invoke when a message is published on the queue</param>
        /// <param name="errorHandler">The handler to invoke when an error occurs</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        Task Subscribe(string queue, Func<Message, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct);

        /// <summary>
        /// Subscribes a handler to a queue/topic
        /// </summary>
        /// <param name="queue">The queue to subscribe to</param>
        /// <param name="sessionId">Subscribe only to this session id</param>
        /// <param name="messageHandler">The handler to invoke when a message is published on the queue</param>
        /// <param name="errorHandler">The handler to invoke when an error occurs</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        Task Subscribe(string queue, string sessionId, Func<Message, string, Task> messageHandler, Func<Exception, Task> errorHandler, CancellationToken ct);
    }

    /// <summary>
    /// Base class for message bus message types
    /// </summary>
    /// <param name="SessionId">Optional session id</param>
    [JsonConverter(typeof(PolymorphicJsonConverter<Message>))]
    public abstract record Message(string? SessionId)
    {
    }
}
