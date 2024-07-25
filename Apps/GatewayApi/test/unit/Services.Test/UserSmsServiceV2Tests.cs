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
    using FluentValidation;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for UserSmsServiceV2.
    /// </summary>
    public class UserSmsServiceV2Tests
    {
        private const string Hdid = "hdid-mock";
        private const string SmsNumber = "2505556000";
        private const string SmsNumberAllAlpha = "sms-number-all-alpha";
        private const string SmsNumberInvalid = "0000000000";
        private const string SmsNumberWithDashes = "250-555-6000";
        private const string SmsVerificationCode = "sms-verification-code";
        private const int SmsVerificationExpirySeconds = 43200;
        private const int ExceedsMaxVerificationAttempts = 6;
        private const int VerificationExpiryDays = 5;

        /// <summary>
        /// GenerateMessagingVerification.
        /// </summary>
        /// <param name="sms">SMS number to be set for the user.</param>
        /// <param name="sanitize">If set to true, the provided SMS number will be sanitized before being used.</param>
        [Theory]
        [InlineData(SmsNumber, true)]
        [InlineData(SmsNumber, false)]
        [InlineData(SmsNumberWithDashes, true)]
        [InlineData(SmsNumberWithDashes, false)]
        public void ShouldGenerateMessagingVerification(string sms, bool sanitize)
        {
            // Arrange
            string expectedSmsNumber = sanitize ? SmsNumber : !sms.Contains('-', StringComparison.Ordinal) ? SmsNumber : SmsNumberWithDashes;
            DateTime expectedExpireDate = DateTime.UtcNow.AddDays(VerificationExpiryDays);

            GenerateVerificationMock mock = SetupGenerateVerificationMock(Hdid, sms);

            // Act
            MessagingVerification actual = mock.Service.GenerateMessagingVerification(mock.Hdid, mock.Sms, sanitize);

            // Assert
            Assert.Equal(Hdid, actual.UserProfileId);
            Assert.Equal(expectedSmsNumber, actual.SmsNumber);
            Assert.Equal(MessagingVerificationType.Sms, actual.VerificationType);
            Assert.Equal(expectedExpireDate.Date, actual.ExpireDate.Date);
            Assert.NotNull(actual.SmsValidationCode);
        }

        /// <summary>
        /// UpdateSmsNumberAsync.
        /// </summary>
        /// <param name="sms">SMS number's verification to be updated.</param>
        /// <param name="lastSmsVerificationExists">
        /// The bool value indicating if last sms verification exists for
        /// GetLastForUserAsync.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(SmsNumber, true)]
        [InlineData(SmsNumber, false)]
        [InlineData(SmsNumberWithDashes, true)]
        [InlineData(SmsNumberWithDashes, false)]
        [InlineData(SmsNumberAllAlpha, true)]
        [InlineData(SmsNumberAllAlpha, false)]
        public async Task ShouldUpdateSmsNumberAsync(string sms, bool lastSmsVerificationExists)
        {
            // Arrange
            Times expectedVerificationExpireTimes = ConvertToTimes(lastSmsVerificationExists);
            Times expectedVerificationInsertTimes = ConvertToTimes(sms != SmsNumberAllAlpha);

            UpdateSmsNumberMock mock = SetupUpdateSmsNumberMock(sms, lastSmsVerificationExists: lastSmsVerificationExists);

            // Act
            await mock.Service.UpdateSmsNumberAsync(mock.Hdid, mock.Sms, CancellationToken.None);

            // Assert and Verify
            VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
            VerifyVerificationExpire(mock.MessagingVerificationDelegateMock, expectedVerificationExpireTimes);
            VerifyVerificationInsert(mock.MessagingVerificationDelegateMock, expectedVerificationInsertTimes);
            VerifyQueueNotificationSettingsRequest(mock.NotificationSettingsServiceMock);
        }

        /// <summary>
        /// UpdateSmsNumberAsync throws exception.
        /// </summary>
        /// <param name="sms">SMS number's verification to be updated.</param>
        /// <param name="userProfileExists">
        /// The bool value indicating whether the user profile exists or not.
        /// </param>
        /// <param name="updateProfileStatus">The status returned when user profile is updated.</param>
        /// <param name="expectedException">The expected exception type to be thrown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(SmsNumberInvalid, false, null, typeof(ValidationException))]
        [InlineData(SmsNumber, false, null, typeof(NotFoundException))]
        [InlineData(SmsNumber, true, DbStatusCode.Error, typeof(DatabaseException))]
        public async Task UpdateSmsNumberAsyncThrowsExceptionAsync(
            string sms,
            bool userProfileExists,
            DbStatusCode? updateProfileStatus,
            Type expectedException)
        {
            // Arrange
            UpdateSmsNumberMock mock = SetupUpdateSmsNumberMock(
                sms,
                userProfileExists,
                updateProfileStatus: updateProfileStatus);

            // Act and Assert
            await Assert.ThrowsAsync(
                expectedException,
                async () => { await mock.Service.UpdateSmsNumberAsync(mock.Hdid, mock.Sms, CancellationToken.None); });
        }

        /// <summary>
        /// VerifySmsNumberAsync.
        /// </summary>
        /// <param name="changeFeedEnabled">The bool value indicating if change feed should be validated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldVerifySmsNumberAsync(bool changeFeedEnabled)
        {
            // Arrange
            Times expectedNotificationChannelVerifiedEventTimes = changeFeedEnabled ? Times.Once() : Times.Never();

            VerifySmsNumberMock mock = SetupVerifySmsNumberMock(changeFeedEnabled: changeFeedEnabled);

            // Act
            bool actual = await mock.Service.VerifySmsNumberAsync(mock.Hdid, mock.VerificationCode, CancellationToken.None);

            // Assert and Verify
            Assert.True(actual);

            VerifyNotificationChannelVerifiedEvent(mock.MessageSenderMock, expectedNotificationChannelVerifiedEventTimes);
            VerifyQueueNotificationSettingsRequest(mock.NotificationSettingsServiceMock);
        }

        /// <summary>
        /// VerifySmsNumberAsync fails verification.
        /// </summary>
        /// <param name="verificationValidated">The bool value indicating if sms verification was successfully validated.</param>
        /// <param name="verificationDeleted">The bool value indicating if sms verification deleted was deleted.</param>
        /// <param name="verificationAttempts">The value indicating the number of sms verification attempts.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, false, 0)] // sms verification already validated
        [InlineData(false, true, 0)] // sms verification deleted
        [InlineData(false, false, ExceedsMaxVerificationAttempts)] // exceeds max sms verification attempts
        public async Task VerifySmsNumberAsyncFailsVerification(bool verificationValidated, bool verificationDeleted, int verificationAttempts)
        {
            // Arrange
            VerifySmsNumberMock mock = SetupVerifySmsNumberMock(
                verificationValidated: verificationValidated,
                verificationDeleted: verificationDeleted,
                verificationAttempts: verificationAttempts);

            // Act
            bool actual = await mock.Service.VerifySmsNumberAsync(mock.Hdid, mock.VerificationCode, CancellationToken.None);

            // Assert and Verify
            Assert.False(actual);

            VerifyNotificationChannelVerifiedEvent(mock.MessageSenderMock, Times.Never());
            VerifyQueueNotificationSettingsRequest(mock.NotificationSettingsServiceMock, Times.Never());
        }

        /// <summary>
        /// VerifySmsNumberAsync throws database exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifySmsNumberAsyncThrowsDatabaseException()
        {
            // Arrange
            VerifySmsNumberMock mock = SetupVerifySmsNumberMock(updateProfileStatus: DbStatusCode.Error);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(
                async () => { await mock.Service.VerifySmsNumberAsync(mock.Hdid, mock.VerificationCode, CancellationToken.None); });
        }

        private static void VerifyNotificationChannelVerifiedEvent(Mock<IMessageSender> messageSenderMock, Times? times = null)
        {
            messageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyQueueNotificationSettingsRequest(Mock<INotificationSettingsService> notificationSettingsServiceMock, Times? times = null)
        {
            notificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyUserProfileUpdate(Mock<IUserProfileDelegate> userProfileDelegateMock, Times? times = null)
        {
            userProfileDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationExpire(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                s => s.ExpireAsync(
                    It.IsAny<MessagingVerification>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationInsert(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                s => s.InsertAsync(
                    It.IsAny<MessagingVerification>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static Times ConvertToTimes(bool expected)
        {
            return expected ? Times.Once() : Times.Never();
        }

        private static MessagingVerification GenerateMessagingVerification(
            string userProfileId = Hdid,
            string smsNumber = SmsNumber,
            string? smsValidationCode = null,
            int verificationAttempts = 0,
            bool validated = false,
            bool deleted = false,
            Guid? inviteKey = null,
            DateTime? expireDate = null)
        {
            return new()
            {
                UserProfileId = userProfileId,
                VerificationAttempts = verificationAttempts,
                InviteKey = inviteKey ?? Guid.NewGuid(),
                ExpireDate = expireDate ?? DateTime.UtcNow.AddSeconds(SmsVerificationExpirySeconds),
                Validated = validated,
                Deleted = deleted,
                SmsNumber = smsNumber,
                SmsValidationCode = smsValidationCode,
                VerificationType = MessagingVerificationType.Sms,
            };
        }

        private static UserProfile GenerateUserProfile(
            string hdid = Hdid,
            DateTime? loginDate = null,
            DateTime? closedDateTime = null,
            int daysFromLoginDate = 0,
            string? email = null,
            string? smsNumber = null,
            BetaFeature? betaFeature = null)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = hdid,
                Email = email,
                SmsNumber = smsNumber,
                ClosedDateTime = closedDateTime,
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
                BetaFeatureCodes =
                [
                    new BetaFeatureCode
                        { Code = betaFeature ?? BetaFeature.Salesforce },
                ],
            };
        }

        private static DbResult<UserProfile> GenerateUserProfileDbResult(
            DbStatusCode status,
            UserProfile? userProfile = null)
        {
            return new()
            {
                Status = status,
                Payload = userProfile!,
            };
        }

        private static IConfiguration GetConfiguration(bool changeFeedEnabled = false)
        {
            const string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            Dictionary<string, string?> myConfiguration = new()
            {
                { changeFeedKey, changeFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IUserSmsServiceV2 GetUserSmsService(
            Mock<IMessageSender>? messageSenderMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            bool changeFeedEnabled = false)
        {
            messageSenderMock ??= new();
            messagingVerificationDelegateMock ??= new();
            notificationSettingsServiceMock ??= new();
            userProfileDelegateMock ??= new();

            return new UserSmsServiceV2(
                new Mock<ILogger<UserSmsServiceV2>>().Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                notificationSettingsServiceMock.Object,
                messageSenderMock.Object,
                GetConfiguration(changeFeedEnabled));
        }

        private static Mock<IMessagingVerificationDelegate> SetupMessagingVerificationDelegateMock(
            MessagingVerification messagingVerification)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();

            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.IsAny<string>(),
                        It.Is<string>(x => x == MessagingVerificationType.Sms),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);

            return messagingVerificationDelegateMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            UserProfile? userProfile = null,
            DbResult<UserProfile>? updateProfileResult = null)
        {
            userProfileDelegateMock ??= new();

            if (userProfile != null)
            {
                userProfileDelegateMock.Setup(
                        s => s.GetUserProfileAsync(
                            It.IsAny<string>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(userProfile);
            }

            if (updateProfileResult != null)
            {
                userProfileDelegateMock.Setup(
                        s => s.UpdateAsync(
                            It.IsAny<UserProfile>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(updateProfileResult);
            }

            return userProfileDelegateMock;
        }

        private static UpdateSmsNumberMock SetupUpdateSmsNumberMock(
            string sms = SmsNumber,
            bool userProfileExists = true,
            bool lastSmsVerificationExists = true,
            DbStatusCode? updateProfileStatus = DbStatusCode.Updated)
        {
            UserProfile? userProfile = userProfileExists ? GenerateUserProfile() : null;
            MessagingVerification? lastSmsVerification = lastSmsVerificationExists
                ? GenerateMessagingVerification(
                    smsValidationCode: SmsVerificationCode,
                    validated: true)
                : null;

            Mock<IMessageSender> messageSenderMock = new();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(lastSmsVerification);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(
                userProfile: userProfile);

            if (updateProfileStatus != null)
            {
                DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(
                    updateProfileStatus.Value,
                    userProfile);

                userProfileDelegateMock = SetupUserProfileDelegateMock(
                    userProfileDelegateMock,
                    updateProfileResult: updateProfileResult);
            }

            IUserSmsServiceV2 service = GetUserSmsService(
                messageSenderMock,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                userProfileDelegateMock);

            return new(
                service,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                userProfileDelegateMock,
                Hdid,
                sms);
        }

        private static VerifySmsNumberMock SetupVerifySmsNumberMock(
            bool userProfileExists = true,
            bool changeFeedEnabled = false,
            bool verificationValidated = false,
            bool verificationDeleted = false,
            int verificationAttempts = 0,
            DbStatusCode updateProfileStatus = DbStatusCode.Updated)
        {
            UserProfile? userProfile = userProfileExists ? GenerateUserProfile() : null;
            MessagingVerification messagingVerification = GenerateMessagingVerification(
                smsValidationCode: SmsVerificationCode,
                validated: verificationValidated,
                deleted: verificationDeleted,
                verificationAttempts: verificationAttempts);
            DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(
                updateProfileStatus,
                userProfile);

            Mock<IMessageSender> messageSenderMock = new();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(messagingVerification);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(
                userProfile: userProfile,
                updateProfileResult: updateProfileResult);

            IUserSmsServiceV2 service = GetUserSmsService(
                messageSenderMock,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                userProfileDelegateMock,
                changeFeedEnabled);

            return new(
                service,
                messageSenderMock,
                notificationSettingsServiceMock,
                Hdid,
                SmsVerificationCode);
        }

        private static GenerateVerificationMock SetupGenerateVerificationMock(string hdid, string sms)
        {
            IUserSmsServiceV2 service = GetUserSmsService();

            return new(service, hdid, sms);
        }

        private sealed record GenerateVerificationMock(IUserSmsServiceV2 Service, string Hdid, string Sms);

        private sealed record UpdateSmsNumberMock(
            IUserSmsServiceV2 Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            string Hdid,
            string Sms);

        private sealed record VerifySmsNumberMock(
            IUserSmsServiceV2 Service,
            Mock<IMessageSender> MessageSenderMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            string Hdid,
            string VerificationCode);
    }
}
