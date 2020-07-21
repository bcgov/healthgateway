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
    using System;
    using System.Collections.Generic;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;

    /// <summary>
    /// Operations to be performed for Communication Email.
    /// </summary>
    public interface ICommunicationEmailDelegate
    {
        /// <summary>
        /// Add the given communication email.
        /// </summary>
        /// <param name="communicationEmail">The communication email to be added to the database.</param>
        /// <param name="commit">if true the transaction is persisted immediately.</param>
        /// <returns>The added communication email wrapped in a DBResult.</returns>
        DBResult<CommunicationEmail> Add(CommunicationEmail communicationEmail, bool commit = true);

        /// <summary>
        /// Get a list of active user profiles which are not linked to any communication email by the specified Communication Id.
        ///     AND were created on of after a specific date and time .
        /// </summary>
        /// <param name="communicationId">The communication id for filtering communication emails.</param>
        /// <param name="createdOnOrAfter">The profiles must have created on or after to this date.</param>
        /// <param name="maxRows">The maximum amount of user profiles to return.</param>
        /// <returns>A list of communication emails of the specified Communication, wrapped in a DBResult.</returns>
        List<UserProfile> GetActiveUserProfilesByCommunicationId(Guid communicationId, DateTime? createdOnOrAfter, int maxRows);
    }
}
