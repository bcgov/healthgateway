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
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Database.Models;

/// <summary>
/// Database delegate for outbox queue.
/// </summary>
public interface IOutboxQueueDelegate
{
    /// <summary>
    /// Add items to outbox.
    /// </summary>
    /// <param name="items">The items to add to the outbox queue.</param>
    void Enqueue(IEnumerable<OutboxItem> items);

    /// <summary>
    /// Get and remove pending items from the outbox queue.
    /// </summary>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>An ordered list of queued outbox items by date ascending.</returns>
    Task<IEnumerable<OutboxItem>> DequeueAsync(CancellationToken ct = default);

    /// <summary>
    /// Commit changes to the database.
    /// </summary>
    /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitAsync(CancellationToken ct = default);
}
