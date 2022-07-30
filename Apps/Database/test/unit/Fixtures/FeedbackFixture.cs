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
    using Xunit;

    /// <summary>
    /// Fixture for tests using User Feedback table.
    /// </summary>
    public class FeedbackFixture
    {
        private const string Hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
        private readonly UserFeedback userFeedback = new()
        {
            CreatedBy = Hdid, CreatedDateTime = DateTime.UtcNow,
            UpdatedBy = Hdid, UpdatedDateTime = DateTime.UtcNow,
            IsSatisfied = false, IsReviewed = false,
            UserProfileId = Hdid, Comment = "Unit Test",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackFixture"/> class and seeds test data in User Feedback table.
        /// </summary>
        public FeedbackFixture()
        {
            this.Cleanup();
        }

        /// <summary>
        /// Deletes data in User Feedback table and re-seeds test data.
        /// </summary>
        public void Cleanup()
        {
            using GatewayDbContext context = Fixture.CreateContext();
            context.UserFeedback.RemoveRange(context.UserFeedback);
            context.AddRangeAsync(this.userFeedback).ConfigureAwait(true);
            context.SaveChanges();
        }
    }

    /// <summary>
    /// Contains a collection of tests implementing FeedbackFixtures.
    /// This supports multiple tests implementing FeedbackFixture running in parallel.
    /// </summary>
    [CollectionDefinition("FeedbackFixtures")]
#pragma warning disable SA1402
    public class FeedbackCollection : ICollectionFixture<FeedbackFixture>
#pragma warning restore SA1402
    {
    }
}
