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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// UserProfileNotificationSettingService unit tests.
    /// </summary>
    public class UserProfileNotificationSettingServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string Email = "somebody@healthgateway.goc.bc.ca";
        private const string SmsNumber = "6047779500";

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
                    NotificationType = ProfileNotificationType.BcCancerScreening,
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
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            // Act
            IList<UserProfileNotificationSettingModel> actual =
                await service.GetAsync(Hdid, CancellationToken.None);

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

            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            IReadOnlyList<UserProfileNotificationSetting> notificationSettings = notificationSettingExists
                ?
                [
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Hdid = Hdid,
                        NotificationType = ProfileNotificationType.BcCancerScreening,
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
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            // Act
            await service.UpdateAsync(Hdid, notificationSettingModel, CancellationToken.None);

            // Assert
            notificationSettingDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(x =>
                        x.Hdid == expectedHdid &&
                        x.EmailEnabled == expectedEmailEnabled &&
                        x.SmsEnabled == expectedSmsEnabled &&
                        x.NotificationType == expectedType),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            outboxStoreMock.Verify(
                m => m.StoreAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(envelopes =>
                        envelopes.First().Content is NotificationChannelPreferencesChangedEvent),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// UpdateAsync updates only email when sms is not provided.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsyncUpdatesOnlyEmailWhenSmsNotProvided()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            IReadOnlyList<UserProfileNotificationSetting> notificationSettings =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Hdid = Hdid,
                    NotificationType = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = false,
                    SmsEnabled = true,
                },
            ];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            UserProfileNotificationSettingModel model = new()
            {
                Type = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = true,
                SmsEnabled = null,
            };

            // Act
            await service.UpdateAsync(Hdid, model, CancellationToken.None);

            // Assert
            notificationSettingDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(x =>
                        x.Hdid == Hdid &&
                        x.NotificationType == ProfileNotificationType.BcCancerScreening &&
                        x.EmailEnabled == true &&
                        x.SmsEnabled == true),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            outboxStoreMock.Verify(
                m => m.StoreAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(envelopes =>
                        envelopes.First().Content is NotificationChannelPreferencesChangedEvent),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// UpdateAsync updates only sms when email is not provided.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsyncUpdatesOnlySmsWhenEmailNotProvided()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            IReadOnlyList<UserProfileNotificationSetting> notificationSettings =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Hdid = Hdid,
                    NotificationType = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = true,
                    SmsEnabled = true,
                },
            ];

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettings);

            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            UserProfileNotificationSettingModel model = new()
            {
                Type = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = null,
                SmsEnabled = false,
            };

            // Act
            await service.UpdateAsync(Hdid, model, CancellationToken.None);

            // Assert
            notificationSettingDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(x =>
                        x.Hdid == Hdid &&
                        x.NotificationType == ProfileNotificationType.BcCancerScreening &&
                        x.EmailEnabled == true &&
                        x.SmsEnabled == false),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            outboxStoreMock.Verify(
                m => m.StoreAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(envelopes =>
                        envelopes.First().Content is NotificationChannelPreferencesChangedEvent),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// UpdateAsync does nothing when no values are provided.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsyncSendsEmptyTargetsWhenNoValuesProvided()
        {
            // Arrange
            Mock<IUserProfileDelegate> profileDelegateMock = new();
            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            Mock<IOutboxStore> outboxStoreMock = new();

            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = "somebody@healthgateway.gov.bc.ca",
                SmsNumber = "6047779500",
            };

            UserProfileNotificationSettingModel model = new()
            {
                Type = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = null,
                SmsEnabled = null,
            };

            List<MessageEnvelope>? capturedEvents = null;

            profileDelegateMock
                .Setup(x => x.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            notificationSettingDelegateMock
                .Setup(x => x.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            notificationSettingDelegateMock
                .Setup(x => x.UpdateAsync(
                    It.IsAny<UserProfileNotificationSetting>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            outboxStoreMock
                .Setup(x => x.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<MessageEnvelope>, bool, CancellationToken>((events, _, _) => { capturedEvents = events.ToList(); })
                .Returns(Task.CompletedTask);

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            // Act
            await service.UpdateAsync(Hdid, model, CancellationToken.None);

            // Assert
            profileDelegateMock.Verify(
                x => x.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                Times.Once);

            notificationSettingDelegateMock.Verify(
                x => x.GetAsync(Hdid, It.IsAny<CancellationToken>()),
                Times.Once);

            notificationSettingDelegateMock.Verify(
                x => x.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(s =>
                        s.Hdid == Hdid &&
                        s.NotificationType == ProfileNotificationType.BcCancerScreening &&
                        s.EmailEnabled == null &&
                        s.SmsEnabled == null),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            outboxStoreMock.Verify(
                x => x.StoreAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(events =>
                        events.First().Content is NotificationChannelPreferencesChangedEvent),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.NotNull(capturedEvents);
            MessageEnvelope envelope = Assert.Single(capturedEvents);

            NotificationChannelPreferencesChangedEvent notificationEvent =
                Assert.IsType<NotificationChannelPreferencesChangedEvent>(envelope.Content);

            Assert.Equal(Hdid, notificationEvent.Hdid);
            Assert.Empty(notificationEvent.EmailNotificationTargets);
            Assert.Empty(notificationEvent.SmsNotificationTargets);
        }

        [Theory]
        [InlineData("test@healthgateway.gov.bc.ca", true, "6046715000", true, 1, 1)]
        [InlineData("test@healthgateway.gov.bc.ca", false, "6046715000", true, 0, 1)]
        [InlineData(null, true, "6046715000", true, 0, 1)]
        [InlineData("test@healthgateway.gov.bc.ca", true, "6046715000", false, 1, 0)]
        [InlineData("test@healthgateway.gov.bc.ca", true, null, true, 1, 0)]
        [InlineData(null, false, null, false, 0, 0)]
        public async Task UpdateAsyncSetsTargetsCorrectly(
            string? email,
            bool emailEnabled,
            string? smsNumber,
            bool smsEnabled,
            int expectedEmailTargetCount,
            int expectedSmsTargetCount)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = email,
                SmsNumber = smsNumber,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            NotificationChannelPreferencesChangedEvent? capturedEvent = null;

            Mock<IOutboxStore> outboxStoreMock = new();
            outboxStoreMock
                .Setup(m => m.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<MessageEnvelope>, bool, CancellationToken>((envelopes, _, _) => { capturedEvent = envelopes.First().Content as NotificationChannelPreferencesChangedEvent; })
                .Returns(Task.CompletedTask);

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            UserProfileNotificationSettingModel model = new()
            {
                Type = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = emailEnabled,
                SmsEnabled = smsEnabled,
            };

            // Act
            await service.UpdateAsync(Hdid, model, CancellationToken.None);

            // Assert
            capturedEvent.ShouldNotBeNull();
            capturedEvent!.EmailNotificationTargets.Count.ShouldBe(expectedEmailTargetCount);
            capturedEvent.SmsNotificationTargets.Count.ShouldBe(expectedSmsTargetCount);
        }


        /// <summary>
        /// UpdateAsync throws NotFoundException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsyncThrowsNotFoundException()
        {
            // Arrange
            UserProfileNotificationSettingModel notificationSettingModel = new()
            {
                Type = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = true,
                SmsEnabled = false,
            };

            UserProfile? userProfile = null;

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            // Act and Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await service.UpdateAsync(Hdid, notificationSettingModel, CancellationToken.None));
        }

        /// <summary>
        /// UpdateAsync throws ArgumentOutOfRangeException when notification type is unsupported.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAsyncThrowsArgumentOutOfRangeExceptionWhenTypeUnsupported()
        {
            // Arrange
            const ProfileNotificationType unsupportedType = (ProfileNotificationType)999;

            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IOutboxStore> outboxStoreMock = new();
            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            UserProfileNotificationSettingModel model = new()
            {
                Type = unsupportedType,
                EmailEnabled = true,
                SmsEnabled = true,
            };

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
                await service.UpdateAsync(Hdid, model, CancellationToken.None));
        }

        /// <summary>
        /// UpdateAsync collection overload preserves existing channel values when a model value is not provided.
        /// </summary>
        /// <param name="existingEmailEnabled">The existing email enabled value.</param>
        /// <param name="existingSmsEnabled">The existing SMS enabled value.</param>
        /// <param name="modelEmailEnabled">The model email enabled value.</param>
        /// <param name="modelSmsEnabled">The model SMS enabled value.</param>
        /// <param name="expectedEmailTargets">The expected email target count.</param>
        /// <param name="expectedSmsTargets">The expected SMS target count.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, null, null, true, 1, 1)]
        [InlineData(true, null, null, false, 1, 0)]
        [InlineData(null, true, true, null, 1, 1)]
        [InlineData(null, true, false, null, 0, 1)]
        [InlineData(false, true, null, null, 0, 1)]
        [InlineData(true, false, null, null, 1, 0)]
        public async Task UpdateAsyncCollectionUsesExistingValuesWhenModelValuesAreNull(
            bool? existingEmailEnabled,
            bool? existingSmsEnabled,
            bool? modelEmailEnabled,
            bool? modelSmsEnabled,
            int expectedEmailTargets,
            int expectedSmsTargets)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            UserProfileNotificationSetting existingSetting = new()
            {
                Id = Guid.NewGuid(),
                Hdid = Hdid,
                NotificationType = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = existingEmailEnabled,
                SmsEnabled = existingSmsEnabled,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([existingSetting]);

            NotificationChannelPreferencesChangedEvent? capturedEvent = null;

            Mock<IOutboxStore> outboxStoreMock = new();
            outboxStoreMock
                .Setup(s => s.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<MessageEnvelope>, bool, CancellationToken>((events, _, _) => { capturedEvent = events.Single().Content as NotificationChannelPreferencesChangedEvent; })
                .Returns(Task.CompletedTask);

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            IReadOnlyCollection<UserProfileNotificationSettingModel> models =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = modelEmailEnabled,
                    SmsEnabled = modelSmsEnabled,
                },
            ];

            // Act
            await service.UpdateAsync(Hdid, models, true, CancellationToken.None);

            // Assert
            notificationSettingDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(x =>
                        x.Hdid == Hdid &&
                        x.NotificationType == ProfileNotificationType.BcCancerScreening),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            outboxStoreMock.Verify(
                s => s.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            capturedEvent.ShouldNotBeNull();
            capturedEvent!.EmailNotificationTargets.Count.ShouldBe(expectedEmailTargets);
            capturedEvent.SmsNotificationTargets.Count.ShouldBe(expectedSmsTargets);
        }

        [Fact]
        public async Task UpdateAsyncCollectionPassesCommitToOutboxStore()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IOutboxStore> outboxStoreMock = new();

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            IReadOnlyCollection<UserProfileNotificationSettingModel> models =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = true,
                    SmsEnabled = true,
                },
            ];

            // Act
            await service.UpdateAsync(Hdid, models, false, CancellationToken.None);

            // Assert
            outboxStoreMock.Verify(
                s => s.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        /// <summary>
        /// UpdateAsync collection overload preserves the existing channel value when only the other channel is provided.
        /// </summary>
        /// <param name="existingEmailEnabled">The existing email enabled value.</param>
        /// <param name="existingSmsEnabled">The existing SMS enabled value.</param>
        /// <param name="modelEmailEnabled">The model email enabled value.</param>
        /// <param name="modelSmsEnabled">The model SMS enabled value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, null, null, true)]
        [InlineData(null, true, true, null)]
        public async Task UpdateAsyncCollectionPreservesExistingChannelWhenOnlyOtherChannelProvided(
            bool? existingEmailEnabled,
            bool? existingSmsEnabled,
            bool? modelEmailEnabled,
            bool? modelSmsEnabled)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = Hdid,
                Email = Email,
                SmsNumber = SmsNumber,
            };

            UserProfileNotificationSetting existingSetting = new()
            {
                Id = Guid.NewGuid(),
                Hdid = Hdid,
                NotificationType = ProfileNotificationType.BcCancerScreening,
                EmailEnabled = existingEmailEnabled,
                SmsEnabled = existingSmsEnabled,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new();
            profileDelegateMock
                .Setup(s => s.GetUserProfileAsync(Hdid, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IUserProfileNotificationSettingDelegate> notificationSettingDelegateMock = new();
            notificationSettingDelegateMock
                .Setup(s => s.GetAsync(Hdid, It.IsAny<CancellationToken>()))
                .ReturnsAsync([existingSetting]);

            NotificationChannelPreferencesChangedEvent? capturedEvent = null;

            Mock<IOutboxStore> outboxStoreMock = new();
            outboxStoreMock
                .Setup(s => s.StoreAsync(
                    It.IsAny<IEnumerable<MessageEnvelope>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<MessageEnvelope>, bool, CancellationToken>((events, _, _) => { capturedEvent = events.Single().Content as NotificationChannelPreferencesChangedEvent; })
                .Returns(Task.CompletedTask);

            IUserProfileNotificationSettingService service =
                GetNotificationSettingService(profileDelegateMock, notificationSettingDelegateMock, outboxStoreMock);

            IReadOnlyCollection<UserProfileNotificationSettingModel> models =
            [
                new()
                {
                    Type = ProfileNotificationType.BcCancerScreening,
                    EmailEnabled = modelEmailEnabled,
                    SmsEnabled = modelSmsEnabled,
                },
            ];

            // Act
            await service.UpdateAsync(Hdid, models, true, CancellationToken.None);

            // Assert
            notificationSettingDelegateMock.Verify(
                x => x.UpdateAsync(
                    It.Is<UserProfileNotificationSetting>(s =>
                        s.Hdid == Hdid &&
                        s.NotificationType == ProfileNotificationType.BcCancerScreening &&
                        s.EmailEnabled == true &&
                        s.SmsEnabled == true),
                    false,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            capturedEvent.ShouldNotBeNull();
            capturedEvent!.EmailNotificationTargets.ShouldContain(NotificationTargets.BcCancer);
            capturedEvent.SmsNotificationTargets.ShouldContain(NotificationTargets.BcCancer);
        }

        private static IUserProfileNotificationSettingService GetNotificationSettingService(
            Mock<IUserProfileDelegate>? profileDelegateMock = null,
            Mock<IUserProfileNotificationSettingDelegate>? notificationSettingDelegateMock = null,
            Mock<IOutboxStore>? outboxStoreMock = null)
        {
            profileDelegateMock ??= new();
            notificationSettingDelegateMock ??= new();
            outboxStoreMock ??= new();

            return new UserProfileNotificationSettingService(
                profileDelegateMock.Object,
                notificationSettingDelegateMock.Object,
                MappingService,
                outboxStoreMock.Object);
        }
    }
}
