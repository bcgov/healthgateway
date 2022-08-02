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
    using System.Threading.Tasks;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Models;
    using Respawn.Graph;
    using Xunit;

    /// <summary>
    /// Fixture for tests using UserFeedback table.
    /// </summary>
    public class UserFeedbackFixture
    {
        /// <summary>
        /// Test data value for Comment.
        /// </summary>
        public const string UserFeedbackComment = "Unit Test";
        private const string Hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

        private readonly Table[] tablesToInclude =
        {
            "UserFeedback",
        };

        private readonly UserFeedback userFeedback = new()
        {
            CreatedBy = Hdid, CreatedDateTime = DateTime.UtcNow,
            UpdatedBy = Hdid, UpdatedDateTime = DateTime.UtcNow,
            IsSatisfied = false, IsReviewed = false,
            UserProfileId = Hdid, Comment = UserFeedbackComment,
        };

        /// <summary>
        /// Creates a new instance of UserFeedbackFixture and seeds data to the database.
        /// </summary>
        /// <returns>A new instance of UserFeedbackFixture.</returns>
        public static Task<UserFeedbackFixture> CreateAsyncFeedbackFixture()
        {
            UserFeedbackFixture userFeedbackFixture = new();
            return userFeedbackFixture.InitializeAsyncFeedbackFixture();
        }

        /// <summary>
        /// Deletes data in User Feedback table and then re-seeds original test data.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Cleanup()
        {
            using GatewayDbContext context = Fixture.CreateContext();
            await Fixture.ResetDatabase(this.tablesToInclude).ConfigureAwait(true);
            await context.AddRangeAsync(this.userFeedback).ConfigureAwait(true);
            context.SaveChanges();
        }

        private async Task<UserFeedbackFixture> InitializeAsyncFeedbackFixture()
        {
            await this.Cleanup().ConfigureAwait(true);
            return this;
        }
    }

    /// <summary>
    /// Contains a collection of tests implementing FeedbackFixtures.
    /// This supports multiple tests implementing UserFeedbackFixture running in parallel.
    /// </summary>
    [CollectionDefinition("FeedbackFixtures")]
#pragma warning disable SA1402
    public class FeedbackCollection : ICollectionFixture<UserFeedbackFixture>
#pragma warning restore SA1402
    {
    }
}
