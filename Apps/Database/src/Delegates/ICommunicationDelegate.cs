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
    using System.Collections.Generic;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for Communications.
    /// </summary>
    public interface ICommunicationDelegate
    {
        /// <summary>
        /// Gets the next oldest by effective date, non-expired communication banner by type.
        /// </summary>
        /// <param name="communicationType">The active communication type to retrieve.</param>
        /// <returns>The Communication wrapped in a DBResult.</returns>
        DBResult<Communication?> GetNext(CommunicationType communicationType);

        /// <summary>
        /// Add the given communication.
        /// </summary>
        /// <param name="communication">The communication to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>The added communication wrapped in a DBResult.</returns>
        DBResult<Communication> Add(Communication communication, bool commit = true);

        /// <summary>
        /// Get a list of all past communications.
        /// </summary>
        /// <returns>A list of all communications added, wrapped in a DBResult.</returns>
        DBResult<IEnumerable<Communication>> GetAll();

        /// <summary>
        /// Update the given communication.
        /// </summary>
        /// <param name="communication">The communication to be updated in the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>The updated communication wrapped in a DBResult.</returns>
        DBResult<Communication> Update(Communication communication, bool commit = true);

        /// <summary>
        /// Deletes the given communication from the database.
        /// </summary>
        /// <param name="communication">The communication to be deleted from the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A Communication wrapped in a DBResult.</returns>
        DBResult<Communication> Delete(Communication communication, bool commit = true);
    }
}
