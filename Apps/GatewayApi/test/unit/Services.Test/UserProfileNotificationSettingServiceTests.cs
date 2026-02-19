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
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileNotificationSettingService unit tests.
    /// </summary>
    public class UserProfileNotificationSettingServiceTests
    {
        private const string Hdid = "hdid-mock";

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// GetAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetAsyncShouldReturnUserSettings()
        {
            // Arrange
            IReadOnlyList<UserProfileNotificationSetting> notificationSettings =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Hdid = Hdid,
                    NotificationTypeCode = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = true,
                    SmsEnabled = true,
                },
            ];

            IReadOnlyList<UserProfileNotificationSettingModel> expected =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = true,
                    SmsEnabled = true,
                },
            ];

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock.Setup(s => s.GetAsync(
                    Hdid,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            IUserProfileNotificationSettingService service = GetNotificationSettingService(notificationSettingDelegateMock);

            // Act
            IList<UserProfileNotificationSettingModel> actual = await service.GetAsync(Hdid, CancellationToken.None);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        /// <summary>
        /// UpdateAsync.
        /// </summary>
        /// <param name="notificationSettingExists">The value indicating whether a notification setting exists or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateAsync(bool notificationSettingExists)
        {
            // Arrange
            const ProfileNotificationType expectedType = ProfileNotificationType.BcCancerScreening;
            const string expectedHdid = Hdid;
            const bool expectedEmailEnabled = true;
            const bool expectedSmsEnabled = false;

            IReadOnlyList<UserProfileNotificationSetting> notificationSettings = notificationSettingExists
                ?
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Hdid = Hdid,
                        NotificationTypeCode = ProfileNotificationType.BcCancerScreening,
                        EmailEnabled = true,
                        SmsEnabled = true,
                    },
                ]
                : [];

            UserProfileNotificationSettingModel notificationSettingModel = new()
            {
                Type = expectedType,
                EmailEnabled = expectedEmailEnabled,
                SmsEnabled = expectedSmsEnabled,
            };

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock.Setup(s => s.GetAsync(
                    Hdid,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            IUserProfileNotificationSettingService service = GetNotificationSettingService(notificationSettingDelegateMock);

            // Act
            await service.UpdateAsync(Hdid, notificationSettingModel, CancellationToken.None);

            // Verify
            notificationSettingDelegateMock.Verify(v => v.UpdateAsync(
                It.Is<UserProfileNotificationSetting>(x => x.Hdid == expectedHdid
                                                           && x.EmailEnabled == expectedEmailEnabled
                                                           && x.SmsEnabled == expectedSmsEnabled
                                                           && x.NotificationTypeCode == expectedType),
                It.IsAny<CancellationToken>()));
        }

        private static IUserProfileNotificationSettingService GetNotificationSettingService(
            Mock<IUserProfileNotificationSettingDelegate>? notificationSettingDelegateMock = null)
        {
            notificationSettingDelegateMock ??= new();

            return new UserProfileNotificationSettingService(
                notificationSettingDelegateMock.Object,
                MappingService);
        }
    }
}
