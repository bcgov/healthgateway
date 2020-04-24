//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Database.Delegates
{
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for Communications.
    /// </summary>
    public interface ICommunicationDelegate
    {
        /// <summary>
        /// Gets the active communication from the DB.
        /// </summary>
        /// <returns>The Communication wrapped in a DBResult.</returns>
        DBResult<Communication> GetActive();

        /// <summary>
        /// Add the given communication.
        /// </summary>
        /// <param name="communication">The communication to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>The added communication wrapped in a DBResult.</returns>
        DBResult<Communication> Add(Communication communication, bool commit = true);
    }
}
