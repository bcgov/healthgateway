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
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Receive messages.
/// </summary>
public interface IMessageReceiver
{
    /// <summary>
    /// Subscribes handlers to receive one-way messages.
    /// </summary>
    /// <param name="receiveHandler">Message receive handler that receives a session id and array of messages,
    /// it can return false to reject the messages or true when processed successfully.</param>
    /// <param name="errorHandler">Error handler that receives the exception when an error occurred.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    Task Subscribe(Func<string, IEnumerable<MessageEnvelope>, Task<bool>> receiveHandler, Func<Exception, Task> errorHandler, CancellationToken ct = default);
}
