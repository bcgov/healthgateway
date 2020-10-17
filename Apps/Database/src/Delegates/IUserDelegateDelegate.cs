//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
// <auto-generated />
#pragma warning disable CS1591
namespace HealthGateway.Database.Delegates
{
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Delegate that performs operations for the User Delegate model.
    /// </summary>
    public interface IUserDelegateDelegate
    {
        /// <summary>
        /// Creates a User Delegate record in the database.
        /// </summary>
        /// <param name="userDelegate">The user delegate to create.</param>
        /// <param name="commit">Indicates if the transaction should be persisted immediately.</param>
        /// <returns>A DB result which encapsulates the return record and status.</returns>
        DBResult<UserDelegate> Insert(UserDelegate userDelegate, bool commit);

        /// <summary>
        /// Gets the list of User Delegate records for a specific delegate Id from the database.
        /// </summary>
        /// <param name="delegateId">The user delegate to create.</param>
        /// <param name="page">The data page to fetch.</param>
        /// <param name="pageSize">The amount of rows to fetch per call.</param>
        /// <returns>A list of UserDelegates wrapped in a DBResult</returns>
        DBResult<IEnumerable<UserDelegate>> Get(string delegateId, int page, int pageSize);
    }
}
