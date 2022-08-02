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
namespace HealthGateway.DatabaseTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.DatabaseTests.Fixtures;
    using Microsoft.Extensions.Logging.Abstractions;
    using Xunit;

    /// <summary>
    /// Feedback Delegate unit tests.
    /// </summary>
    [Collection("FeedbackFixtures")]
    public class FeedbackDelegateTest : IAsyncDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackDelegateTest"/> class.
        /// </summary>
        public FeedbackDelegateTest()
        {
            this.UserFeedbackFixture = UserFeedbackFixture.CreateAsyncFeedbackFixture().Result;
            this.AdminTagFixture = AdminTagFixture.CreateAsyncAdminTagFixture().Result;
        }

        private UserFeedbackFixture UserFeedbackFixture { get; }

        private AdminTagFixture AdminTagFixture { get; }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Should get result from UserFeedback table.
        /// </summary>
        [Fact]
        public void ShouldGetUserFeedback()
        {
            const int expected = 1;

            // Arrange
            using GatewayDbContext context = Fixture.CreateContext();
            DBFeedbackDelegate feedbackDelegate = new(new NullLogger<DBFeedbackDelegate>(), context);

            // Act
            DBResult<IList<UserFeedback>> result = feedbackDelegate.GetAllUserFeedbackEntries();

            // Assert
            Assert.Equal(DBStatusCode.Read, result.Status);
            Assert.Equal(expected, result.Payload.Count);
        }

        /// <summary>
        /// Should add Tag association to UserFeedbackTag.
        /// </summary>
        [Fact]
        public void ShouldAddUserFeedbackTag()
        {
            // Arrange
            using GatewayDbContext context = Fixture.CreateContext();
            UserFeedback userFeedback = context.UserFeedback.Single(f => f.Comment == UserFeedbackFixture.UserFeedbackComment);
            AdminTag tag = context.AdminTag.Single(t => t.Name == AdminTagFixture.AdminTagName);
            userFeedback.Tags.Add(
                new UserFeedbackTag
                {
                    AdminTag = tag,
                    UserFeedback = userFeedback,
                });
            DBFeedbackDelegate feedbackDelegate = new(new NullLogger<DBFeedbackDelegate>(), context);

            // Act
            DBResult<UserFeedback> result = feedbackDelegate.UpdateUserFeedbackWithTagAssociations(userFeedback);

            // Assert
            Assert.Equal(DBStatusCode.Updated, result.Status);
            Assert.True(result.Payload.Tags.First().AdminTagId != Guid.Empty);
        }

        /// <summary>
        /// Calling cleanup to delete data from tables defined in fixtures and  to also re-seed original test data.
        /// </summary>
        /// <returns>Task.</returns>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!this.disposed)
            {
                await this.UserFeedbackFixture.Cleanup().ConfigureAwait(true);
                await this.AdminTagFixture.Cleanup().ConfigureAwait(true);
            }

            this.disposed = true;
        }
    }
}
