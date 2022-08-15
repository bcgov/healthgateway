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
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Context;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.DatabaseTests.Utils;
    using Microsoft.Extensions.Logging;
    using Respawn.Graph;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Tests for the DBProfileDelegate.
    /// </summary>
    public class DBProfileDelegateTests : DatabaseTest
    {
        private readonly ITestOutputHelper testOutputHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBProfileDelegateTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">Instance of TestOutputHelper.</param>
        public DBProfileDelegateTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

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

        /// <inheritdoc/>
        protected override string SeedSql()
        {
            return ReadSeedData("HealthGateway.DatabaseTests.SeedData.seed.sql");
        }

        /// <inheritdoc/>
        protected override Table[] TablesToReset()
        {
            return Array.Empty<Table>();
        }

        /// <inheritdoc/>
        protected override void SetupDatabase(GatewayDbContext context)
        {
            // Do nothing.
        }

        /// <inheritdoc/>
        protected override void Log(string message)
        {
            this.testOutputHelper.WriteLine($"Database Test: {message}");
        }
    }
}
