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
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Providers;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit Tests for UserSmsService.
    /// </summary>
    public class UserSmsServiceTests
    {
        private const string HdIdMock = "hdIdMock";
        private const string InvalidHdidMock = "invalid" + HdIdMock;
        private const string SmsValidationCode = "SMSValidationCodeMock";
        private const bool ChangeFeedEnabled = false;

        /// <summary>
        /// ValidateSmsAsync.
        /// </summary>
        /// <param name="hdid">The hdid associated with the sms.</param>
        /// <param name="smsValidationCode">The sms validation code.</param>
        /// <param name="userProfileExists">The bool value indicating if the user profile for the associated hdid exists.</param>
        /// <param name="smsVerificationExpired">The bool value indicating if the sms verification has expired.</param>
        /// <param name="changeFeedEnabled">The bool value indicating if change feed should be validated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(HdIdMock, SmsValidationCode, true, false, false)]
        [InlineData(HdIdMock, SmsValidationCode, true, false, true)]
        [InlineData(HdIdMock, SmsValidationCode, true, true, true)]
        [InlineData(HdIdMock, SmsValidationCode, false, false, false)]
        [InlineData(InvalidHdidMock, SmsValidationCode, true, false, false)]
        public async Task ShouldValidateSms(
            string hdid,
            string smsValidationCode,
            bool userProfileExists,
            bool smsVerificationExpired,
            bool changeFeedEnabled)
        {
            // Arrange
            bool shouldValidate =
                userProfileExists &&
                !smsVerificationExpired &&
                hdid == HdIdMock;

            UserProfile? userProfile = userProfileExists ? new UserProfile() : null;

            MessagingVerification verification = new()
            {
                UserProfileId = hdid,
                VerificationAttempts = 0,
                SmsValidationCode = smsValidationCode,
                ExpireDate = DateTime.UtcNow.AddDays(smsVerificationExpired ? -1 : 1),
            };

            Mock<IJobService> jobServiceMock = new();
            Mock<IUserProfileNotificationSettingService> profileNotificationSettingServiceMock = new();
            Mock<IBackgroundJobClient> backgroundJobClientMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IUserSmsService service = GetUserSmsService(
                messagingVerification: verification,
                userProfile: userProfile,
                jobServiceMock: jobServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                profileNotificationSettingServiceMock: profileNotificationSettingServiceMock,
                backgroundJobClientMock: backgroundJobClientMock,
                changeFeedEnabled: changeFeedEnabled);

            // Act
            RequestResult<bool> result =
                await service.ValidateSmsAsync(HdIdMock, smsValidationCode, CancellationToken.None);

            // Assert
            Assert.Equal(shouldValidate, result.ResourcePayload);

            Times successPathTimes = shouldValidate ? Times.Once() : Times.Never();
            Times changeFeedTimes = shouldValidate && changeFeedEnabled ? Times.Once() : Times.Never();

            jobServiceMock.Verify(
                v => v.NotifySmsVerificationAsync(
                    HdIdMock,
                    It.IsAny<string>(),
                    false,
                    It.IsAny<CancellationToken>()),
                changeFeedTimes);

            profileNotificationSettingServiceMock.Verify(
                v => v.UpdateAsync(
                    HdIdMock,
                    It.Is<IReadOnlyCollection<UserProfileNotificationSettingModel>>(models =>
                        models.Single().Type == ProfileNotificationType.BcCancerScreening &&
                        models.Single().EmailEnabled == null &&
                        models.Single().SmsEnabled == true),
                    false,
                    It.IsAny<CancellationToken>()),
                successPathTimes);

            backgroundJobClientMock.Verify(
                v => v.Create(
                    It.Is<Job>(job => job.Type == typeof(DbOutboxStore)),
                    It.IsAny<EnqueuedState>()),
                successPathTimes);

            notificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()),
                successPathTimes);
        }

        /// <summary>
        /// ValidateSmsAsync returns not found error when updating user profile to the database.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateSmsReturnsErrorWhenUserProfileUpdateNotFound()
        {
            // Arrange
            DbResult<UserProfile> dbResult = new() { Status = DbStatusCode.NotFound }; // Should cause error to be returned.
            UserProfile userProfile = new();
            MessagingVerification messagingVerification = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = SmsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IJobService> jobServiceMock = new();
            IUserSmsService service = GetUserSmsService(
                messagingVerification: messagingVerification,
                userProfile: userProfile,
                jobServiceMock: jobServiceMock,
                updateUserProfileResult: dbResult);

            // Act
            RequestResult<bool> actual = await service.ValidateSmsAsync(HdIdMock, SmsValidationCode, CancellationToken.None);

            // Assert
            Assert.False(actual.ResourcePayload);
            Assert.Equal(ErrorMessages.UserProfileNotFound, actual.ResultError?.ResultMessage);

            // Verify
            jobServiceMock.Verify(
                v => v.NotifySmsVerificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.Is<bool>(b => b == false),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        /// <summary>
        /// UpdateUserSmsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserSms()
        {
            // Arrange
            const string sms = "2508801234";
            UserProfile userProfile = new();

            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = SmsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            IUserSmsService service = GetUserSmsService(
                messagingVerificationDelegateMock,
                expectedResult,
                userProfileDelegateMock,
                userProfile,
                notificationSettingsServiceMock: notificationSettingsServiceMock);

            // Act and Assert
            Assert.True(await service.UpdateUserSmsAsync(HdIdMock, sms));

            // Verify
            userProfileDelegateMock
                .Verify(
                    s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            messagingVerificationDelegateMock
                .Verify(
                    s => s.ExpireAsync(It.IsAny<MessagingVerification>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            messagingVerificationDelegateMock
                .Verify(
                    s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, It.IsAny<CancellationToken>()),
                    Times.Once);

            notificationSettingsServiceMock
                .Verify(
                    s => s.QueueNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        /// <summary>
        /// UpdateUserSmsAsync throws NotFoundException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateUserSmsThrowsNotFoundException()
        {
            // Arrange
            const string sms = "2508801234";
            UserProfile? userProfile = null; // Should cause NotFoundException

            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = SmsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            IUserSmsService service = GetUserSmsService(
                messagingVerificationDelegateMock,
                expectedResult,
                userProfileDelegateMock,
                userProfile,
                notificationSettingsServiceMock: notificationSettingsServiceMock);

            // Act and Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateUserSmsAsync(HdIdMock, sms));
        }

        /// <summary>
        /// UpdateUserSmsAsync throws ValidationException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateUserSmsThrowsValidationException()
        {
            // Arrange
            const string sms = "0000000000"; // Invalid sms should cause ValidationException
            UserProfile userProfile = new();

            MessagingVerification expectedResult = new()
            {
                UserProfileId = HdIdMock,
                VerificationAttempts = 0,
                SmsValidationCode = SmsValidationCode,
                ExpireDate = DateTime.Now.AddDays(1),
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            IUserSmsService service = GetUserSmsService(
                messagingVerificationDelegateMock,
                expectedResult,
                userProfileDelegateMock,
                userProfile,
                notificationSettingsServiceMock: notificationSettingsServiceMock);

            // Act and Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.UpdateUserSmsAsync(HdIdMock, sms));
        }

        /// <summary>
        /// CreateUserSmsAsync.
        /// </summary>
        /// <param name="smsNumber">The sms number to sanitize.</param>
        [Theory]
        [InlineData("1234561234")]
        [InlineData("(123)4561234")]
        [InlineData("123 456 1234")]
        [InlineData("+1 123-456-1234")]
        public void ShouldCreateUserSms(string smsNumber)
        {
            // Arrange
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            IUserSmsService service = GetUserSmsService(messagingVerificationDelegateMock: messagingVerificationDelegateMock);

            // Act
            service.CreateUserSmsAsync(HdIdMock, smsNumber);

            // Verify
            messagingVerificationDelegateMock
                .Verify(s => s.InsertAsync(It.Is<MessagingVerification>(x => x.UserProfileId == HdIdMock && x.SmsNumber.All(char.IsDigit)), !ChangeFeedEnabled, CancellationToken.None));
        }

        private static Mock<IGatewayDbContextTransactionProvider> GetTransactionProviderMock()
        {
            Mock<IGatewayDbContextTransactionProvider> transactionProviderMock = new();
            Mock<IDbContextTransaction> transactionMock = new();

            transactionProviderMock
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            transactionProviderMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            transactionMock
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return transactionProviderMock;
        }

        private static IUserSmsService GetUserSmsService(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            MessagingVerification? messagingVerification = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            UserProfile? userProfile = null,
            Mock<IJobService>? jobServiceMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserProfileNotificationSettingService>? profileNotificationSettingServiceMock = null,
            Mock<IBackgroundJobClient>? backgroundJobClientMock = null,
            Mock<IGatewayDbContextTransactionProvider>? transactionProviderMock = null,
            bool changeFeedEnabled = false,
            DbResult<UserProfile>? updateUserProfileResult = null)
        {
            updateUserProfileResult ??= new DbResult<UserProfile> { Status = DbStatusCode.Updated };
            messagingVerificationDelegateMock ??= new();
            messagingVerificationDelegateMock
                .Setup(s => s.GetLastForUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);
            messagingVerificationDelegateMock
                .Setup(s => s.InsertAsync(It.IsAny<MessagingVerification>(), !ChangeFeedEnabled, CancellationToken.None))
                .ReturnsAsync(Guid.Empty);

            userProfileDelegateMock ??= new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(userProfile);
            userProfileDelegateMock.Setup(s => s.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateUserProfileResult);

            notificationSettingsServiceMock ??= new();
            profileNotificationSettingServiceMock ??= new();
            jobServiceMock ??= new();
            backgroundJobClientMock ??= new();
            transactionProviderMock ??= GetTransactionProviderMock();

            return new UserSmsService(
                new Mock<ILogger<UserSmsService>>().Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                notificationSettingsServiceMock.Object,
                profileNotificationSettingServiceMock.Object,
                jobServiceMock.Object,
                backgroundJobClientMock.Object,
                transactionProviderMock.Object,
                GetConfiguration(changeFeedEnabled));
        }

        private static IConfiguration GetConfiguration(bool changeFeedEnabled = false)
        {
            const string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            Dictionary<string, string?> myConfiguration = new()
            {
                { changeFeedKey, changeFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
