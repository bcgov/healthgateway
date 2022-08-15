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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.DatabaseTests.Fixtures;
    using HealthGateway.DatabaseTests.Utils;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Respawn.Graph;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Feedback Delegate unit tests.
    /// </summary>
    public class FeedbackDelegateTest : DatabaseTest,  IClassFixture<FeedbackFixture>
    {
        private readonly FeedbackFixture feedbackFixture;
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackDelegateTest"/> class.
        /// </summary>
        /// <param name="feedbackFixture">Contains an instance of FeedbackFixture.</param>
        /// <param name="testOutputHelper">Instance of TestOutputHelper.</param>
        public FeedbackDelegateTest(FeedbackFixture feedbackFixture, ITestOutputHelper testOutputHelper)
        {
            this.feedbackFixture = feedbackFixture;
            this.testOutputHelper = testOutputHelper;
        }

        /// <summary>
        /// Should get result from UserFeedback table.
        /// </summary>
        [Fact]
        public void ShouldGetUserFeedback()
        {
            const int expected = 1;

            // Arrange
            DBFeedbackDelegate feedbackDelegate = new(new NullLogger<DBFeedbackDelegate>(), this.DbContext);

            // Act
            DBResult<IList<UserFeedback>> result = feedbackDelegate.GetAllUserFeedbackEntries();

            // Assert
            Assert.Equal(DBStatusCode.Read, result.Status);
            Assert.Equal(expected, result.Payload.Count);

            this.testOutputHelper.WriteLine($"UserFeedbackId: {result.Payload.First().Id}");
        }

        /// <summary>
        /// Should add Tag association to UserFeedbackTag.
        /// </summary>
        [Fact]
        public void ShouldAddUserFeedbackTag()
        {
            // Arrange
            UserFeedback userFeedback = this.DbContext!.UserFeedback.Single(f => f.Comment == FeedbackFixture.Comment);
            this.testOutputHelper.WriteLine($"UserFeedbackId: {userFeedback.Id}");

            AdminTag adminTag = this.DbContext!.AdminTag.Single(t => t.Name == FeedbackFixture.AdminTagName);
            userFeedback.Tags.Add(
                new UserFeedbackTag
                {
                    AdminTag = adminTag,
                    UserFeedback = userFeedback,
                });
            DBFeedbackDelegate feedbackDelegate = new(new NullLogger<DBFeedbackDelegate>(), this.DbContext);

            // Act
            DBResult<UserFeedback> result = feedbackDelegate.UpdateUserFeedbackWithTagAssociations(userFeedback);

            // Assert
            Assert.Equal(DBStatusCode.Updated, result.Status);
            Assert.True(result.Payload.Tags.First().AdminTagId != Guid.Empty);
        }

        /// <inheritdoc />
        protected override string SeedSql()
        {
            return ReadSeedData("HealthGateway.DatabaseTests.SeedData.seed.sql");
        }

        /// <inheritdoc />
        protected override Table[] TablesToReset()
        {
            return this.feedbackFixture.TablesToReset();
        }

        /// <inheritdoc />
        protected override void SetupDatabase(GatewayDbContext context)
        {
            this.feedbackFixture.SetupDatabase(context);
        }

        /// <inheritdoc />
        protected override void Log(string message)
        {
            this.testOutputHelper.WriteLine($"Database Test: {message}");
        }
    }
}
