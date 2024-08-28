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
namespace HealthGateway.Admin.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// Tests for the DashboardService class.
    /// </summary>
    public class DashboardServiceTests
    {
        private static readonly IConfiguration Configuration = GetIConfigurationRoot();

        /// <summary>
        /// GetAllTimeCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAllTimeCounts()
        {
            // Arrange
            const int userProfileCount = 5;
            const int dependentCount = 2;
            const int closedUserProfileCount = 2;

            AllTimeCounts expected = new()
            {
                RegisteredUsers = userProfileCount,
                ClosedAccounts = closedUserProfileCount,
                Dependents = dependentCount,
            };

            IDashboardService service = SetupGetAllTimeCountsMock(userProfileCount, closedUserProfileCount, dependentCount);

            // Act
            AllTimeCounts actual = await service.GetAllTimeCountsAsync();

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetDailyUsageCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDailyUsageCounts()
        {
            // Arrange
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int userRegistrationCount = 5;
            const int userLoginCount = 2;
            const int dependentRegistrationCount = 2;

            IDictionary<DateOnly, int> userRegistrationCounts = new Dictionary<DateOnly, int>
            {
                { endDate, userRegistrationCount },
                { startDate, userRegistrationCount },
            };

            IDictionary<DateOnly, int> userLoginCounts = new Dictionary<DateOnly, int>
            {
                { endDate, userLoginCount },
                { startDate, userLoginCount },
            };

            IDictionary<DateOnly, int> dependentRegistrationCounts = new Dictionary<DateOnly, int>
            {
                { endDate, dependentRegistrationCount },
                { startDate, dependentRegistrationCount },
            };

            DailyUsageCounts expected = new()
            {
                UserRegistrations = new SortedDictionary<DateOnly, int>(userRegistrationCounts),
                UserLogins = new SortedDictionary<DateOnly, int>(userLoginCounts),
                DependentRegistrations = new SortedDictionary<DateOnly, int>(dependentRegistrationCounts),
            };

            IDashboardService service = SetupDailyUsageCountsMock(userRegistrationCounts, userLoginCounts, dependentRegistrationCounts);

            // Act
            DailyUsageCounts actual = await service.GetDailyUsageCountsAsync(startDate, endDate);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetRecurringUserCountAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetRecurringUserCount()
        {
            // Arrange
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int dayCount = 5;
            const int userCount = 10;
            const int expected = 10;

            IDashboardService service = SetupGetRecurringUserCountMock(dayCount, userCount);

            // Act
            int actual = await service.GetRecurringUserCountAsync(dayCount, startDate, endDate);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// GetAppLoginCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAppLoginCounts()
        {
            // Arrange
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int webCount = 5;
            const int mobileCount = 2;
            const int androidCount = 1;
            const int iosCount = 1;
            const int salesforceCount = 2;

            AppLoginCounts expected = new(webCount, mobileCount + androidCount + iosCount, androidCount, iosCount, salesforceCount);

            IDashboardService service = SetupGetAppLoginCountsMock(webCount, mobileCount, androidCount, iosCount, salesforceCount);

            // Act
            AppLoginCounts actual = await service.GetAppLoginCountsAsync(startDate, endDate);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetRatingsSummaryAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetRatingsSummary()
        {
            // Arrange
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int fiveStarCount = 5;
            const int threeStarCount = 1;

            IDictionary<string, int> expected = new Dictionary<string, int>
            {
                { "3", threeStarCount },
                { "5", fiveStarCount },
            };

            IDashboardService service = SetupGetRatingsSummaryMock(threeStarCount, fiveStarCount);

            // Act
            IDictionary<string, int> actual = await service.GetRatingsSummaryAsync(startDate, endDate);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetAgeCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAgeCounts()
        {
            // Arrange
            DateOnly startDate = new(2010, 1, 15);
            DateOnly startDatePlus5Years = startDate.AddYears(5);
            DateOnly endDate = new(2024, 1, 15);

            const int startDateAge = 14;
            const int startDatePlus5YearsAge = 9;
            const int startDateAgeCount = 5;
            const int startDatePlus5YearsAgeCount = 1;

            IDictionary<int, int> expected = new Dictionary<int, int>
            {
                { startDateAge, startDateAgeCount },
                { startDatePlus5YearsAge, startDatePlus5YearsAgeCount },
            };

            IDashboardService service = SetupGetAgeCountsMock(startDate, startDatePlus5Years, startDateAgeCount, startDatePlus5YearsAgeCount);

            // Act
            IDictionary<int, int> actual = await service.GetAgeCountsAsync(startDate, endDate);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "TimeZone:UnixTimeZoneId", "America/Vancouver" },
                { "TimeZone:WindowsTimeZoneId", "Pacific Standard Time" },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static IDashboardService GetDashboardService(
            Mock<IResourceDelegateDelegate>? dependentDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IRatingDelegate>? ratingDelegateMock = null)
        {
            dependentDelegateMock ??= new();
            userProfileDelegateMock ??= new();
            ratingDelegateMock ??= new();

            return new DashboardService(
                Configuration,
                dependentDelegateMock.Object,
                userProfileDelegateMock.Object,
                ratingDelegateMock.Object);
        }

        private static IDashboardService SetupGetAllTimeCountsMock(int userProfileCount, int closedUserProfileCount, int dependentCount)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfileCount);
            userProfileDelegateMock.Setup(s => s.GetClosedUserProfileCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(closedUserProfileCount);

            Mock<IResourceDelegateDelegate> dependentDelegateMock = new();
            dependentDelegateMock.Setup(s => s.GetDependentCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dependentCount);

            return GetDashboardService(dependentDelegateMock, userProfileDelegateMock);
        }

        private static IDashboardService SetupDailyUsageCountsMock(
            IDictionary<DateOnly, int> userRegistrationCounts,
            IDictionary<DateOnly, int> userLoginCounts,
            IDictionary<DateOnly, int> dependentRegistrationCounts)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetDailyUserRegistrationCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRegistrationCounts);
            userProfileDelegateMock.Setup(s => s.GetDailyUniqueLoginCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(userLoginCounts);

            Mock<IResourceDelegateDelegate> dependentDelegateMock = new();
            dependentDelegateMock.Setup(s => s.GetDailyDependentRegistrationCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dependentRegistrationCounts);

            return GetDashboardService(dependentDelegateMock, userProfileDelegateMock);
        }

        private static IDashboardService SetupGetRecurringUserCountMock(int dayCount, int userCount)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetRecurringUserCountAsync(dayCount, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCount);

            return GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
        }

        private static IDashboardService SetupGetAppLoginCountsMock(int webCount, int mobileCount, int androidCount, int iosCount, int salesforceCount)
        {
            IDictionary<UserLoginClientType, int> lastLoginClientCounts = new Dictionary<UserLoginClientType, int>
            {
                { UserLoginClientType.Web, webCount },
                { UserLoginClientType.Mobile, mobileCount },
                { UserLoginClientType.Android, androidCount },
                { UserLoginClientType.Ios, iosCount },
                { UserLoginClientType.Salesforce, salesforceCount },
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetLoginClientCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastLoginClientCounts);

            return GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
        }

        private static IDashboardService SetupGetRatingsSummaryMock(int threeStarCount, int fiveStarCount)
        {
            IDictionary<string, int> summary = new Dictionary<string, int>
            {
                { "3", threeStarCount },
                { "5", fiveStarCount },
            };

            Mock<IRatingDelegate> ratingsDelegateMock = new();
            ratingsDelegateMock.Setup(s => s.GetRatingsSummaryAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            return GetDashboardService(ratingDelegateMock: ratingsDelegateMock);
        }

        private static IDashboardService SetupGetAgeCountsMock(DateOnly startDate, DateOnly startDatePlus5Years, int startDateAgeCount, int startDatePlus5YearsAgeCount)
        {
            IDictionary<int, int> yearOfBirthCounts = new Dictionary<int, int>
            {
                { startDate.Year, startDateAgeCount },
                { startDatePlus5Years.Year, startDatePlus5YearsAgeCount },
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetLoggedInUserYearOfBirthCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yearOfBirthCounts);

            return GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
        }
    }
}
