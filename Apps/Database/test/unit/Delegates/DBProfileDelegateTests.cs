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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.DatabaseTests.Utils;
    using Microsoft.Extensions.Logging;
    using Xunit;

    /// <summary>
    /// Tests for the DBProfileDelegate.
    /// </summary>
    public class DBProfileDelegateTests : DatabaseTest
    {
        /// <summary>
        /// Verifies database reads.
        /// </summary>
        [Fact]
        public void ShouldGetProfile()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<DBProfileDelegate> logger = loggerFactory.CreateLogger<DBProfileDelegate>();
            IUserProfileDelegate dbDelegate = new DBProfileDelegate(logger, this.DbContext);
            DBResult<UserProfile> profileResult = dbDelegate.GetUserProfile("P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A");
            Assert.True(profileResult.Status == DBStatusCode.Read);
        }

        /// <inheritdoc />
        protected override string SeedSql()
        {
            return ReadSeedData("HealthGateway.DatabaseTests.SeedData.seed.sql");
        }
    }
}
