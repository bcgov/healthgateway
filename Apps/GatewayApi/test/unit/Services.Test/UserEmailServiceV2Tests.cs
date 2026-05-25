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
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
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
    /// Unit Tests for UserEmailServiceV2Tests.
    /// </summary>
    public class UserEmailServiceV2Tests
    {
        private const string Hdid = "hdid-mock";
        private const string InvalidHdid = "Does not match hdid-mock";
        private const string MainEmailAddress = "main@healthgateway.gov.bc.ca";
        private const string SecondaryEmailAddress = "secondary@healthgateway.gov.bc.ca";
        private const string EmailTemplateHost = "https://www.healthgateway.gov.bc.ca";
        private const int EmailVerificationExpirySeconds = 43200;

        /// <summary>
        /// VerifyEmailAddressAsync - Happy path scenario.
        /// </summary>
        /// <param name="changeFeedEnabled">
        /// The bool value indicating whether change feed on notifications is enabled or not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldVerifyEmailAddress(bool changeFeedEnabled)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            Times expectedNotificationChannelVerifiedEventTimes = ConvertToTimes(changeFeedEnabled);

            VerifyEmailAddressMock mock = SetupVerifyEmailAddressMock(inviteKey, changeFeedEnabled);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(Hdid, inviteKey, CancellationToken.None);

            // Assert and Verify
            Assert.True(actual);

            Assert.Equal(MainEmailAddress, mock.UserProfile.Email);
            VerifyNotifyEmailVerification(mock.JobServiceMock, expectedNotificationChannelVerifiedEventTimes);
            VerifyUserProfileNotificationSettingsUpdate(mock.ProfileNotificationSettingServiceMock, mock.BackgroundJobClientMock);
            VerifyQueueNotificationSettings(mock.NotificationSettingServiceMock);
        }

        /// <summary>
        /// VerifyEmailAddressAsync - Too many attempts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressTooManyAttempts()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            IUserEmailServiceV2 service = SetupVerifyEmailAddressTooManyAttemptsMock(inviteKey);

            // Act
            bool actual = await service.VerifyEmailAddressAsync(Hdid, inviteKey, CancellationToken.None);

            // Assert and Verify
            Assert.False(actual);
        }

        /// <summary>
        /// VerifyEmailAddressAsync - Already validated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressAlreadyValidated()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            IUserEmailServiceV2 service = SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock(inviteKey);

            // Act and assert
            await Assert.ThrowsAsync<AlreadyExistsException>(async () => { await service.VerifyEmailAddressAsync(Hdid, inviteKey); });
        }

        /// <summary>
        /// VerifyEmailAddressAsync - invalid invite last sent.
        /// </summary>
        /// <param name="hdid">The hdid associated with the verification by invite key.</param>
        /// <param name="verificationExists">
        /// The bool value indicating whether a matching verification exists.
        /// </param>
        /// <param name="deleted">The matching verification's deleted value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(InvalidHdid, true, false)] // User Profile Hdid does not match hdid in matching verification.
        [InlineData(Hdid, false, false)] // Matching verification does not exist.
        [InlineData(Hdid, true, true)] // Matching verification is deleted.
        public async Task VerifyEmailAddressInvalidInvite(string hdid, bool verificationExists, bool deleted)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();

            VerifyEmailAddressInvalidInviteMock mock = SetupVerifyEmailAddressInvalidInviteMock(
                hdid,
                inviteKey,
                deleted,
                verificationExists);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(Hdid, inviteKey);

            // Assert and Verify
            Assert.False(actual);

            VerifyVerificationUpdateValidatedFalse(mock.MessagingVerificationDelegateMock);
        }

        /// <summary>
        /// VerifyEmailAddressAsync throws database exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressShouldThrowDatabaseException()
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            IUserEmailServiceV2 service = SetupVerifyEmailAddressThrowsDatabaseExceptionMock(inviteKey);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(async () => { await service.VerifyEmailAddressAsync(Hdid, inviteKey); });
        }

        /// <summary>
        /// UpdateEmailAddressAsync.
        /// </summary>
        /// <param name="latestVerificationExists">
        /// The bool value indicating whether the latest messaging verification exists or
        /// not.
        /// </param>
        /// <param name="emailAddress">The email address to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, MainEmailAddress)]
        [InlineData(true, null)]
        [InlineData(true, "")]
        [InlineData(false, MainEmailAddress)]
        [InlineData(true, SecondaryEmailAddress)]
        [InlineData(false, SecondaryEmailAddress)]
        public async Task ShouldUpdateEmailAddress(bool latestVerificationExists, string? emailAddress)
        {
            // Arrange
            Times expectedVerificationInsertTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));
            Times expectedQueueNewEmailByEntityTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));

            EmailAddressMock mock = SetupUpdateEmailAddressMock(
                Hdid,
                latestVerificationExists,
                emailAddress);

            // Act and Verify
            await mock.Service.UpdateEmailAddressAsync(Hdid, emailAddress);

            VerifyAddEmailVerification(mock.MessagingVerificationServiceMock, expectedVerificationInsertTimes);
            VerifyUserProfileNotificationSettingsEmailDisabled(mock.ProfileNotificationSettingServiceMock);
            VerifyQueueNotificationSettings(mock.NotificationSettingServiceMock);
            VerifyQueueNewEmail(mock.EmailQueueServiceMock, expectedQueueNewEmailByEntityTimes);
        }

        /// <summary>
        /// UpdateEmailAddressAsync throws exception.
        /// </summary>
        /// <param name="userProfileExists">
        /// The bool value indicating whether the user profile exists or not.
        /// </param>
        /// <param name="updateProfileStatus">The status returned when user profile is updated.</param>
        /// <param name="expectedException">The expected exception type to be thrown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, DbStatusCode.Updated, typeof(NotFoundException))]
        public async Task UpdateEmailAddressShouldThrowExceptionAsync(
            bool userProfileExists,
            DbStatusCode updateProfileStatus,
            Type expectedException)
        {
            // Arrange
            IUserEmailServiceV2 service = SetupUpdateEmailAddressThrowsExceptionMock(
                userProfileExists,
                updateProfileStatus);

            // Act and Assert
            await Assert.ThrowsAsync(
                expectedException,
                async () => { await service.UpdateEmailAddressAsync(Hdid, MainEmailAddress); });
        }

        private static void VerifyQueueNewEmail(Mock<IEmailQueueService> emailQueueServiceMock, Times? times = null)
        {
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyNotifyEmailVerification(Mock<IOutboxStoreService> outboxStoreServiceMock, Times? times = null)
        {
            outboxStoreServiceMock.Verify(
                v => v.QueueEmailVerificationEventAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyAddEmailVerification(
            Mock<IMessagingVerificationService> messagingVerificationServiceMock,
            Times? times = null)
        {
            messagingVerificationServiceMock.Verify(
                s => s.AddEmailVerificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<Guid>(),
                    It.Is<bool>(x => !x),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationUpdateValidatedFalse(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MessagingVerification>(mv => !mv.Validated &&
                                                       mv.VerificationAttempts == 1),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyQueueNotificationSettings(Mock<INotificationSettingsService> notificationSettingsServiceMock, Times? times = null)
        {
            notificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyUserProfileNotificationSettingsUpdate(
            Mock<IUserProfileNotificationSettingService> notificationSettingServiceMock,
            Mock<IBackgroundJobClient> backgroundJobClient,
            Times? times = null)
        {
            notificationSettingServiceMock.Verify(
                v => v.UpdateAsync(
                    Hdid,
                    It.Is<IReadOnlyCollection<UserProfileNotificationSettingModel>>(models =>
                        models.Single().Type == ProfileNotificationType.BcCancerScreening &&
                        models.Single().EmailEnabled == true &&
                        models.Single().SmsEnabled == null),
                    false,
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());

            backgroundJobClient.Verify(
                v => v.Create(
                    It.Is<Job>(job => job.Type == typeof(DbOutboxStore)),
                    It.IsAny<EnqueuedState>()),
                times ?? Times.Once());
        }

        private static void VerifyUserProfileNotificationSettingsEmailDisabled(
            Mock<IUserProfileNotificationSettingService> notificationSettingServiceMock,
            Times? times = null)
        {
            notificationSettingServiceMock.Verify(
                v => v.UpdateAsync(
                    Hdid,
                    It.Is<IReadOnlyCollection<UserProfileNotificationSettingModel>>(models =>
                        models.Single().Type == ProfileNotificationType.BcCancerScreening &&
                        models.Single().EmailEnabled == false &&
                        models.Single().SmsEnabled == null),
                    false,
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static Email GenerateEmail(string toEmailAddress, Guid? emailId = null)
        {
            return new()
            {
                Id = emailId ?? Guid.NewGuid(),
                To = toEmailAddress,
            };
        }

        private static MessagingVerification GenerateMessagingVerification(
            string verificationType,
            string userProfileId,
            int verificationAttempts = 0,
            bool validated = false,
            bool deleted = false,
            string? emailAddress = null,
            Email? email = null,
            Guid? inviteKey = null,
            DateTime? expireDate = null)
        {
            return new()
            {
                UserProfileId = userProfileId,
                VerificationAttempts = verificationAttempts,
                InviteKey = inviteKey ?? Guid.NewGuid(),
                ExpireDate = expireDate ?? DateTime.UtcNow.AddSeconds(EmailVerificationExpirySeconds),
                Validated = validated,
                Deleted = deleted,
                EmailAddress = emailAddress,
                Email = email,
                VerificationType = verificationType,
            };
        }

        private static Times ConvertToTimes(bool expected)
        {
            return expected ? Times.Once() : Times.Never();
        }

        private static IConfiguration GetConfiguration(bool changeFeedEnabled)
        {
            const string changeFeedKey = $"{ChangeFeedOptions.ChangeFeed}:Notifications:Enabled";
            const string emailVerificationExpirySecondsKey = "WebClient:EmailVerificationExpirySeconds";
            const string emailTemplateHostKey = "EmailTemplate:Host";

            Dictionary<string, string?> myConfiguration = new()
            {
                { changeFeedKey, changeFeedEnabled.ToString() },
                { emailVerificationExpirySecondsKey, EmailVerificationExpirySeconds.ToString(CultureInfo.InvariantCulture) },
                { emailTemplateHostKey, EmailTemplateHost },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
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

        private static IUserEmailServiceV2 GetUserEmailService(
            Mock<IOutboxStoreService>? outboxStoreServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IMessagingVerificationService>? messagingVerificationServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserProfileNotificationSettingService>? profileNotificationSettingServiceMock = null,
            Mock<IBackgroundJobClient>? backgroundJobClientMock = null,
            Mock<IGatewayDbContextTransactionProvider>? transactionProviderMock = null,
            bool changeFeedEnabled = false)
        {
            outboxStoreServiceMock ??= new();
            emailQueueServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();
            messagingVerificationServiceMock ??= new();
            userProfileDelegateMock ??= new();
            notificationSettingsServiceMock ??= new();
            profileNotificationSettingServiceMock ??= new();
            backgroundJobClientMock ??= new();
            transactionProviderMock ??= GetTransactionProviderMock();

            return new UserEmailServiceV2(
                new Mock<ILogger<UserEmailServiceV2>>().Object,
                messagingVerificationDelegateMock.Object,
                messagingVerificationServiceMock.Object,
                notificationSettingsServiceMock.Object,
                profileNotificationSettingServiceMock.Object,
                userProfileDelegateMock.Object,
                emailQueueServiceMock.Object,
                outboxStoreServiceMock.Object,
                backgroundJobClientMock.Object,
                transactionProviderMock.Object,
                GetConfiguration(changeFeedEnabled));
        }

        private static Mock<IMessagingVerificationDelegate> SetupMessagingVerificationDelegateMock(
            string userProfileId = Hdid,
            bool setupMatchingVerification = true,
            Guid? matchingVerificationInviteKey = null,
            bool matchingVerificationExists = true,
            int matchingVerificationAttempts = 0,
            bool matchingVerificationValidated = false,
            bool matchingVerificationDeleted = false,
            string matchingVerificationEmailAddress = MainEmailAddress,
            bool setupLatestVerification = false,
            Guid? latestVerificationInviteKey = null,
            bool latestVerificationExists = true,
            string latestVerificationEmailAddress = MainEmailAddress)
        {
            matchingVerificationInviteKey ??= Guid.NewGuid();
            latestVerificationInviteKey ??= Guid.NewGuid();

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();

            if (setupMatchingVerification)
            {
                Email email = GenerateEmail(toEmailAddress: matchingVerificationEmailAddress);

                MessagingVerification? matchingVerification =
                    matchingVerificationExists
                        ? GenerateMessagingVerification(
                            MessagingVerificationType.Email,
                            userProfileId,
                            email: email,
                            inviteKey: matchingVerificationInviteKey,
                            verificationAttempts: matchingVerificationAttempts,
                            validated: matchingVerificationValidated,
                            deleted: matchingVerificationDeleted)
                        : null;

                messagingVerificationDelegateMock.Setup(s => s.GetLastByInviteKeyAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(matchingVerification);
            }

            if (setupLatestVerification)
            {
                Email email = GenerateEmail(toEmailAddress: latestVerificationEmailAddress);

                MessagingVerification? latestEmailVerification =
                    latestVerificationExists
                        ? GenerateMessagingVerification(
                            MessagingVerificationType.Email,
                            userProfileId,
                            inviteKey: latestVerificationInviteKey,
                            email: email,
                            deleted: false)
                        : null;

                messagingVerificationDelegateMock.Setup(s => s.GetLastForUserAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(latestEmailVerification);
            }

            return messagingVerificationDelegateMock;
        }

        private static Mock<IMessagingVerificationService> SetupMessagingVerificationServiceMock(MessagingVerification messagingVerification)
        {
            Mock<IMessagingVerificationService> messagingVerificationMock = new();

            if (messagingVerification.VerificationType == MessagingVerificationType.Email)
            {
                messagingVerificationMock.Setup(s => s.AddEmailVerificationAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<Guid>(),
                        It.Is<bool>(x => !x),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(messagingVerification);
            }

            return messagingVerificationMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            UserProfile? userProfile = null,
            bool userProfileExists = true,
            DbStatusCode? dbUpdateStatus = null)
        {
            userProfile ??= userProfileExists ? new() : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            userProfileDelegateMock.Setup(u => u.GetUserProfileAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            if (dbUpdateStatus != null)
            {
                userProfileDelegateMock.Setup(s => s.UpdateAsync(
                        It.IsAny<UserProfile>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        new DbResult<UserProfile> { Status = dbUpdateStatus.Value });
            }

            return userProfileDelegateMock;
        }

        private static VerifyEmailAddressMock SetupVerifyEmailAddressMock(Guid inviteKey, bool changeFeedEnabled)
        {
            UserProfile userProfile = new();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(userProfile, dbUpdateStatus: DbStatusCode.Updated);
            Mock<IOutboxStoreService> outboxStoreServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IUserProfileNotificationSettingService> profileNotificationSettingServiceMock = new();
            Mock<IBackgroundJobClient> backgroundJobClientMock = new();
            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                outboxStoreServiceMock,
                emailQueueServiceMock,
                messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                profileNotificationSettingServiceMock: profileNotificationSettingServiceMock,
                backgroundJobClientMock: backgroundJobClientMock,
                changeFeedEnabled: changeFeedEnabled);

            return new(
                service,
                outboxStoreServiceMock,
                notificationSettingsServiceMock,
                profileNotificationSettingServiceMock,
                backgroundJobClientMock,
                userProfile);
        }

        private static IUserEmailServiceV2 SetupVerifyEmailAddressTooManyAttemptsMock(Guid inviteKey)
        {
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(
                        matchingVerificationInviteKey: inviteKey,
                        matchingVerificationAttempts: 1000000000); // This will cause too many attempts error.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            return GetUserEmailService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock);
        }

        private static IUserEmailServiceV2 SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock(Guid inviteKey)
        {
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(
                        matchingVerificationInviteKey: inviteKey,
                        matchingVerificationValidated: true); // This will cause an AlreadyExistsException to be thrown.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            return GetUserEmailService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock);
        }

        private static VerifyEmailAddressInvalidInviteMock SetupVerifyEmailAddressInvalidInviteMock(
            string userProfileId,
            Guid inviteKey,
            bool matchingVerificationDeleted = false,
            bool matchingVerificationExists = true)
        {
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(
                    matchingVerificationInviteKey: inviteKey,
                    userProfileId: userProfileId, // See if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    matchingVerificationExists: matchingVerificationExists, // if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    matchingVerificationDeleted: matchingVerificationDeleted, // See if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();
            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock);
            return new(service, messagingVerificationDelegateMock);
        }

        private static IUserEmailServiceV2 SetupVerifyEmailAddressThrowsDatabaseExceptionMock(Guid inviteKey)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);

            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Updated);

            Mock<IGatewayDbContextTransactionProvider> transactionProviderMock = new();
            Mock<IDbContextTransaction> transactionMock = new();

            transactionProviderMock
                .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transactionMock.Object);

            transactionProviderMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DatabaseException("Save failed")); // Throws DatabaseException.

            return GetUserEmailService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                transactionProviderMock: transactionProviderMock,
                changeFeedEnabled: false);
        }

        private static EmailAddressMock SetupUpdateEmailAddressMock(
            string hdid,
            bool latestVerificationExists,
            string? emailAddress,
            DbStatusCode updateProfileStatus = DbStatusCode.Updated)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true,
                    latestVerificationExists: latestVerificationExists,
                    latestVerificationEmailAddress: MainEmailAddress);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(dbUpdateStatus: updateProfileStatus);

            Mock<IMessagingVerificationService>? messagingVerificationServiceMock = null;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                Email email = GenerateEmail(toEmailAddress: emailAddress);
                MessagingVerification messagingVerification = GenerateMessagingVerification(MessagingVerificationType.Email, hdid, email: email);
                messagingVerificationServiceMock = SetupMessagingVerificationServiceMock(messagingVerification);
            }

            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IEmailQueueService> emailQueueServiceMock = new();
            Mock<IUserProfileNotificationSettingService> profileNotificationSettingServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                emailQueueServiceMock: emailQueueServiceMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                messagingVerificationServiceMock: messagingVerificationServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                profileNotificationSettingServiceMock: profileNotificationSettingServiceMock);

            return new(
                service,
                emailQueueServiceMock,
                notificationSettingsServiceMock,
                messagingVerificationServiceMock ?? new Mock<IMessagingVerificationService>(),
                profileNotificationSettingServiceMock);
        }

        private static IUserEmailServiceV2 SetupUpdateEmailAddressThrowsExceptionMock(
            bool userProfileExists = true, // if false, NotFoundException is thrown
            DbStatusCode updateProfileStatus = DbStatusCode.Updated) // if DbStatusCode.Error, DatabaseException is thrown
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfileExists: userProfileExists, dbUpdateStatus: updateProfileStatus);
            Mock<IOutboxStoreService> outboxStoreServiceMock = new();
            Mock<IMessagingVerificationService> messagingVerificationServiceMock = new();
            Mock<IEmailQueueService> emailQueueServiceMock = new();

            return GetUserEmailService(
                outboxStoreServiceMock,
                emailQueueServiceMock,
                messagingVerificationDelegateMock,
                messagingVerificationServiceMock,
                userProfileDelegateMock);
        }

        private sealed record EmailAddressMock(
            IUserEmailServiceV2 Service,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            Mock<INotificationSettingsService> NotificationSettingServiceMock,
            Mock<IMessagingVerificationService> MessagingVerificationServiceMock,
            Mock<IUserProfileNotificationSettingService> ProfileNotificationSettingServiceMock);

        private sealed record VerifyEmailAddressMock(
            IUserEmailServiceV2 Service,
            Mock<IOutboxStoreService> JobServiceMock,
            Mock<INotificationSettingsService> NotificationSettingServiceMock,
            Mock<IUserProfileNotificationSettingService> ProfileNotificationSettingServiceMock,
            Mock<IBackgroundJobClient> BackgroundJobClientMock,
            UserProfile UserProfile);

        private sealed record VerifyEmailAddressInvalidInviteMock(
            IUserEmailServiceV2 Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock);
    }
}
