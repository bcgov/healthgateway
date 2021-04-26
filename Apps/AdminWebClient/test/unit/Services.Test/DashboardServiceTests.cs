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
namespace HealthGateway.Admin.Test.Services
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Admin.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// DashboardService's Unit Tests.
    /// </summary>
    public class DashboardServiceTests
    {
        /// <summary>
        /// GetDependentCount - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetDependentCount()
        {
            Dictionary<DateTime, int> expected = new Dictionary<DateTime, int>() { { default(DateTime), 10 } };
            Mock<IResourceDelegateDelegate> dependentDelegateMock = new Mock<IResourceDelegateDelegate>();
            dependentDelegateMock.Setup(s => s.GetDailyDependentCount(It.IsAny<TimeSpan>())).Returns(expected);

            // Set up service
            IDashboardService service = new DashboardService(
                new Mock<INoteDelegate>().Object,
                dependentDelegateMock.Object,
                new Mock<IUserProfileDelegate>().Object,
                new ConfigurationBuilder().AddJsonFile("UnitTest.json").Build());

            IDictionary<DateTime, int> actualResult = service.GetDailyDependentCount(5);

            // Check result
            Assert.Equal(expected, actualResult);
        }
    }
}
