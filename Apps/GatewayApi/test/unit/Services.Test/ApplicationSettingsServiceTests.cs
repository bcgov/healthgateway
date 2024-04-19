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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Services;
    using Moq;
    using Xunit;

    /// <summary>
    /// ApplicationSettingsService unit tests.
    /// </summary>
    public class ApplicationSettingsServiceTests
    {
        /// <summary>
        /// GetLatestTourChangeDateTimeAsync.
        /// </summary>
        /// <param name="useCache">The value indicating whether cache should be used or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(true)]
        [InlineData(false)]
        [Theory]
        public async Task ShouldGetLatestTourChangeDateTimeAsync(bool useCache)
        {
            // Arrange
            GetLatestTourChangeDateTimeMock mock = SetupGetLatestTourChangeDateTimeMock(useCache);

            // Act
            DateTime? actual = await mock.ApplicationSettingsService.GetLatestTourChangeDateTimeAsync();

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        private static GetLatestTourChangeDateTimeMock SetupGetLatestTourChangeDateTimeMock(bool useCache)
        {
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            string cacheKey = $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}:{TourApplicationSettings.LatestChangeDateTime}";

            ApplicationSetting applicationSetting = new()
            {
                Id = Guid.NewGuid(),
                Application = "Web",
                Component = "Tour",
                Value = latestTourDate.ToString("o"),
            };

            Mock<IApplicationSettingsDelegate> applicationSettingsDelegateMock = new();
            Mock<ICacheProvider> cacheProviderMock = new();

            if (useCache)
            {
                cacheProviderMock.Setup(
                        s => s.GetOrSetAsync(
                            It.Is<string>(key => key.Contains(cacheKey)),
                            It.IsAny<Func<Task<DateTime?>>>(),
                            It.IsAny<TimeSpan?>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(latestTourDate);
            }
            else
            {
                applicationSettingsDelegateMock.Setup(
                        s => s.GetApplicationSettingAsync(
                            It.Is<string>(x => x == TourApplicationSettings.Application),
                            It.Is<string>(x => x == TourApplicationSettings.Component),
                            It.Is<string>(x => x == TourApplicationSettings.LatestChangeDateTime),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(applicationSetting);

                cacheProviderMock.Setup(
                        s => s.GetOrSetAsync(
                            It.Is<string>(key => key.Contains(cacheKey)),
                            It.IsAny<Func<Task<DateTime?>>>(),
                            It.IsAny<TimeSpan?>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        (string _, Func<Task<DateTime?>> valueFactory, TimeSpan _, CancellationToken _) =>
                        {
                            Task<DateTime?> value = valueFactory.Invoke();
                            return value.Result;
                        });
            }

            IApplicationSettingsService applicationSettingsService = new ApplicationSettingsService(
                applicationSettingsDelegateMock.Object,
                cacheProviderMock.Object);

            return new(applicationSettingsService, DateTime.Parse(applicationSetting.Value, CultureInfo.InvariantCulture).ToUniversalTime());
        }

        private sealed record GetLatestTourChangeDateTimeMock(IApplicationSettingsService ApplicationSettingsService, DateTime? Expected);
    }
}
