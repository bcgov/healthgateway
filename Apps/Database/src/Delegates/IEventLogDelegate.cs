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
namespace HealthGateway.Database.Delegates
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the EventLog model.
    /// </summary>
    public interface IEventLogDelegate
    {
        /// <summary>
        /// Writes an event log entry to the DB.
        /// </summary>
        /// <param name="eventLog">The event to write.</param>
        /// <param name="commit">If true, the records will be deleted from the DB immediately.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A DB result which encapsulates the return objects and status.</returns>
        Task<DbResult<EventLog>> WriteEventLogAsync(EventLog eventLog, bool commit = true, CancellationToken ct = default);
    }
}
