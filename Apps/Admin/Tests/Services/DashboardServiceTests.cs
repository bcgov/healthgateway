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
            GetAllTimeCountsMock mock = SetupGetAllTimeCountsMock();

            // Act
            AllTimeCounts actual = await mock.Service.GetAllTimeCountsAsync();

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetDailyUsageCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetDailyUsageCounts()
        {
            // Arrange
            GetDailyUsageCountsMock mock = SetupDailyUsageCountsMock();

            // Act
            DailyUsageCounts actual = await mock.Service.GetDailyUsageCountsAsync(mock.StartDate, mock.EndDate);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetRecurringUserCountAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetRecurringUserCount()
        {
            // Arrange
            GetRecurringUserCountMock mock = SetupGetRecurringUserCountMock();

            // Act
            int actual = await mock.Service.GetRecurringUserCountAsync(mock.DayCount, mock.StartDate, mock.EndDate);

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        /// <summary>
        /// GetAppLoginCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAppLoginCounts()
        {
            // Arrange
            GetAppLoginCountsMock mock = SetupGetAppLoginCountsMock();

            // Act
            AppLoginCounts actual = await mock.Service.GetAppLoginCountsAsync(mock.StartDate, mock.EndDate);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetRatingsSummaryAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetRatingsSummary()
        {
            // Arrange
            GetRatingsSummaryMock mock = SetupGetRatingsSummaryMock();

            // Act
            IDictionary<string, int> actual = await mock.Service.GetRatingsSummaryAsync(mock.StartDate, mock.EndDate);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// GetAgeCountsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetAgeCounts()
        {
            // Arrange
            GetAgeCountsMock mock = SetupGetAgeCountsMock();

            // Act
            IDictionary<int, int> actual = await mock.Service.GetAgeCountsAsync(mock.StartDate, mock.EndDate);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
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
            dependentDelegateMock = dependentDelegateMock ?? new();
            userProfileDelegateMock = userProfileDelegateMock ?? new();
            ratingDelegateMock = ratingDelegateMock ?? new();

            return new DashboardService(
                Configuration,
                dependentDelegateMock.Object,
                userProfileDelegateMock.Object,
                ratingDelegateMock.Object);
        }

        private static GetAllTimeCountsMock SetupGetAllTimeCountsMock()
        {
            const int userProfileCount = 5;
            const int dependentCount = 2;
            const int closedUserProfileCount = 2;

            AllTimeCounts expected = new()
            {
                RegisteredUsers = userProfileCount,
                Dependents = dependentCount,
                ClosedAccounts = closedUserProfileCount,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userProfileCount);
            userProfileDelegateMock.Setup(s => s.GetClosedUserProfileCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(closedUserProfileCount);

            Mock<IResourceDelegateDelegate> dependentDelegateMock = new();
            dependentDelegateMock.Setup(s => s.GetDependentCountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dependentCount);

            IDashboardService service = GetDashboardService(dependentDelegateMock, userProfileDelegateMock);
            return new(service, expected);
        }

        private static GetDailyUsageCountsMock SetupDailyUsageCountsMock()
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int userRegistrationCount = 5;
            const int userLoginCount = 2;
            const int dependentRegistrationCount = 2;

            IDictionary<DateOnly, int> userRegistrationCounts = new Dictionary<DateOnly, int>();
            userRegistrationCounts.Add(endDate, userRegistrationCount);
            userRegistrationCounts.Add(startDate, userRegistrationCount);

            IDictionary<DateOnly, int> userLoginCounts = new Dictionary<DateOnly, int>();
            userLoginCounts.Add(endDate, userLoginCount);
            userLoginCounts.Add(startDate, userLoginCount);

            IDictionary<DateOnly, int> dependentRegistrationCounts = new Dictionary<DateOnly, int>();
            dependentRegistrationCounts.Add(endDate, dependentRegistrationCount);
            dependentRegistrationCounts.Add(startDate, dependentRegistrationCount);

            DailyUsageCounts expected = new()
            {
                UserRegistrations = new SortedDictionary<DateOnly, int>(userRegistrationCounts),
                UserLogins = new SortedDictionary<DateOnly, int>(userLoginCounts),
                DependentRegistrations = new SortedDictionary<DateOnly, int>(dependentRegistrationCounts),
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetDailyUserRegistrationCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userRegistrationCounts);
            userProfileDelegateMock.Setup(s => s.GetDailyUniqueLoginCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(userLoginCounts);

            Mock<IResourceDelegateDelegate> dependentDelegateMock = new();
            dependentDelegateMock.Setup(s => s.GetDailyDependentRegistrationCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dependentRegistrationCounts);

            IDashboardService service = GetDashboardService(dependentDelegateMock, userProfileDelegateMock);
            return new(service, expected, startDate, endDate);
        }

        private static GetRecurringUserCountMock SetupGetRecurringUserCountMock()
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int dayCount = 5;
            const int userCount = 10;
            const int expected = 10;

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetRecurringUserCountAsync(dayCount, It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userCount);

            IDashboardService service = GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
            return new(service, expected, startDate, endDate, dayCount);
        }

        private static GetAppLoginCountsMock SetupGetAppLoginCountsMock()
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int webCount = 5;
            const int mobileCount = 2;
            const int salesforceCount = 2;

            IDictionary<UserLoginClientType, int> lastLoginClientCounts = new Dictionary<UserLoginClientType, int>();
            lastLoginClientCounts.Add(UserLoginClientType.Web, webCount);
            lastLoginClientCounts.Add(UserLoginClientType.Mobile, mobileCount);
            lastLoginClientCounts.Add(UserLoginClientType.Salesforce, salesforceCount);

            AppLoginCounts expected = new(webCount, mobileCount, salesforceCount);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetLoginClientCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastLoginClientCounts);

            IDashboardService service = GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
            return new(service, expected, startDate, endDate);
        }

        private static GetRatingsSummaryMock SetupGetRatingsSummaryMock()
        {
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly startDate = endDate.AddDays(-1);

            const int fiveStarCount = 5;
            const int threeStarCount = 1;

            IDictionary<string, int> summary = new Dictionary<string, int>();
            summary.Add("3", threeStarCount);
            summary.Add("5", fiveStarCount);

            IDictionary<string, int> expected = new Dictionary<string, int>();
            expected.Add("3", threeStarCount);
            expected.Add("5", fiveStarCount);

            Mock<IRatingDelegate> ratingsDelegateMock = new();
            ratingsDelegateMock.Setup(s => s.GetRatingsSummaryAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);

            IDashboardService service = GetDashboardService(ratingDelegateMock: ratingsDelegateMock);
            return new(service, expected, startDate, endDate);
        }

        private static GetAgeCountsMock SetupGetAgeCountsMock()
        {
            DateOnly startDate = new(2010, 1, 15);
            DateOnly startDatePlus5Years = startDate.AddYears(5);
            DateOnly endDate = new(2024, 1, 15);

            const int startDateAge = 14;
            const int startDatePlus5YearsAge = 9;
            const int startDateAgeCount = 5;
            const int startDatePlus5YearsAgeCount = 1;

            IDictionary<int, int> yearOfBirthCounts = new Dictionary<int, int>();
            yearOfBirthCounts.Add(startDate.Year, startDateAgeCount);
            yearOfBirthCounts.Add(startDatePlus5Years.Year, startDatePlus5YearsAgeCount);

            IDictionary<int, int> expected = new Dictionary<int, int>();
            expected.Add(startDateAge, startDateAgeCount);
            expected.Add(startDatePlus5YearsAge, startDatePlus5YearsAgeCount);

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetLoggedInUserYearOfBirthCountsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yearOfBirthCounts);

            IDashboardService service = GetDashboardService(userProfileDelegateMock: userProfileDelegateMock);
            return new(service, expected, startDate, endDate);
        }

        private record GetAllTimeCountsMock(IDashboardService Service, AllTimeCounts Expected);

        private record GetDailyUsageCountsMock(IDashboardService Service, DailyUsageCounts Expected, DateOnly StartDate, DateOnly EndDate);

        private record GetRecurringUserCountMock(IDashboardService Service, int Expected, DateOnly StartDate, DateOnly EndDate, int DayCount);

        private record GetAgeCountsMock(IDashboardService Service, IDictionary<int, int> Expected, DateOnly StartDate, DateOnly EndDate);

        private record GetAppLoginCountsMock(IDashboardService Service, AppLoginCounts Expected, DateOnly StartDate, DateOnly EndDate);

        private record GetRatingsSummaryMock(IDashboardService Service, IDictionary<string, int> Expected, DateOnly StartDate, DateOnly EndDate);
    }
}
