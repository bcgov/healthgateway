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
namespace HealthGateway.DatabaseTests.Fixtures
{
    using System;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Respawn.Graph;

    /// <summary>
    /// Fixture for tests using UserFeedback table.
    /// </summary>
    public class FeedbackFixture : IFixture
    {
        /// <summary>
        /// Test data value for Name.
        /// </summary>
        public const string AdminTagName = "Tag";

        /// <summary>
        /// Test data value for Comment.
        /// </summary>
        public const string Comment = "Unit Test";

        private const string Hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

        private readonly AdminTag adminTag = new()
        {
            CreatedBy = Hdid, CreatedDateTime = DateTime.UtcNow,
            UpdatedBy = Hdid, UpdatedDateTime = DateTime.UtcNow,
            Name = AdminTagName,
        };

        private readonly Table[] tablesToReset =
        {
            "AdminTag",
            "UserFeedback",
        };

        private readonly UserFeedback userFeedback = new()
        {
            CreatedBy = Hdid, CreatedDateTime = DateTime.UtcNow,
            UpdatedBy = Hdid, UpdatedDateTime = DateTime.UtcNow,
            IsSatisfied = false, IsReviewed = false,
            UserProfileId = Hdid, Comment = Comment,
        };

        /// <inheritdoc/>
        public Table[] TablesToReset()
        {
            return this.tablesToReset;
        }

        /// <inheritdoc/>
        public void SetupDatabase(GatewayDbContext context)
        {
            context.AddRangeAsync(this.userFeedback);
            context.AddRangeAsync(this.adminTag);
            context.SaveChanges();
        }
    }
}
