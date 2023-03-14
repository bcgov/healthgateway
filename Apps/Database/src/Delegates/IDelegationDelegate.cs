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
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for models relating to Dependent.
    /// </summary>
    public interface IDelegationDelegate
    {
        /// <summary>
        /// Deletes the allowed delegation object in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will be overridden by our framework.
        /// </summary>
        /// <param name="allowedDelegation">The dependent to delete.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<AllowedDelegation> DeleteAllowedDelegation(AllowedDelegation allowedDelegation, bool commit = true);

        /// <summary>
        /// Fetches the Dependents from the database.
        /// </summary>
        /// <param name="hdid">The dependent hdid to query on.</param>
        /// <param name="includeAllowedDelegation">
        /// Indicates whether allowed delegation should be included in the returned
        /// dependent.
        /// </param>
        /// <returns>A DB result which encapsulates the return objects and status.</returns>
        DbResult<IList<Dependent>> GetDependents(string hdid, bool includeAllowedDelegation = false);

        /// <summary>
        /// Inserts the dependent object including allowed delegation associations in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will be overridden by our framework.
        /// </summary>
        /// <param name="dependent">The dependent to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<Dependent> InsertDependent(Dependent dependent, bool commit = true);

        /// <summary>
        /// Inserts the allowed delegation object in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will be overridden by our framework.
        /// </summary>
        /// <param name="allowedDelegation">The allowed delegation to insert.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<AllowedDelegation> InsertAllowedDelegation(AllowedDelegation allowedDelegation, bool commit = true);

        /// <summary>
        /// Updates the dependent object including allowed delegation associations in the DB.
        /// Version must be set or a Concurrency exception will occur.
        /// UpdatedDateTime will be overridden by our framework.
        /// </summary>
        /// <param name="dependent">The dependent to update.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return object and status.</returns>
        DbResult<Dependent> UpdateDependent(Dependent dependent, bool commit = true);
    }
}
