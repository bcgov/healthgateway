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
    public class FeedbackDelegateTest : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackDelegateTest"/> class.
        /// </summary>
        /// <param name="feedbackFixture">Instance of FeedbackFixture.</param>
        public FeedbackDelegateTest(FeedbackFixture feedbackFixture)
        {
            this.FeedbackFixture = feedbackFixture;
        }

        private FeedbackFixture FeedbackFixture { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Should get result from User Feedback table.
        /// </summary>
        [Fact]
        public void ShouldGetFeedback()
        {
            using GatewayDbContext context = Fixture.CreateContext();
            DBFeedbackDelegate feedbackDelegate = new(new NullLogger<DBFeedbackDelegate>(), context);
            DBResult<IList<UserFeedback>> result = feedbackDelegate.GetAllUserFeedbackEntries();
            Assert.Equal(DBStatusCode.Read, result.Status);
            Assert.Equal(1, result.Payload.Count);
        }

        /// <summary>
        /// Releases the unmanaged resources used by this class optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// If true, releases both managed and unmanaged resources. If false, releases only unmanaged
        /// resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.FeedbackFixture.Cleanup();
            }

            this.disposed = true;
        }
    }
}
