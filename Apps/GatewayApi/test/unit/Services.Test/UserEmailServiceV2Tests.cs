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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
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
        public async Task ShouldVerifyEmailAddressAsync(bool changeFeedEnabled)
        {
            // Arrange
            Times expectedNotificationChannelVerifiedEventTimes = ConvertToTimes(changeFeedEnabled);

            VerifyEmailAddressMock mock = SetupVerifyEmailAddressMock(changeFeedEnabled);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey, CancellationToken.None);

            // Assert and Verify
            Assert.True(actual);

            VerifyVerificationUpdateValidatedTrue(mock.MessagingVerificationDelegateMock);
            VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
            VerifyQueueNotificationSettings(mock.NotificationSettingsServiceMock);
            VerifyNotificationChannelVerifiedEvent(mock.MessageSenderMock, expectedNotificationChannelVerifiedEventTimes);
        }

        /// <summary>
        /// VerifyEmailAddressAsync - Too many attempts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressTooManyAttempts()
        {
            // Arrange
            VerifyEmailAddressExceptionMock mock = SetupVerifyEmailAddressTooManyAttemptsMock();

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey, CancellationToken.None);

            // Assert and Verify
            Assert.False(actual);
        }

        /// <summary>
        /// VerifyEmailAddressAsync - Already validated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressAsyncAlreadyValidated()
        {
            // Arrange
            VerifyEmailAddressExceptionMock mock = SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock();

            // Act and assert
            await Assert.ThrowsAsync<AlreadyExistsException>(
                async () => { await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey); });
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
        public async Task VerifyEmailAddressAsyncInvalidInvite(string hdid, bool verificationExists, bool deleted)
        {
            // Arrange
            VerifyEmailAddressInvalidInviteMock mock = SetupVerifyEmailAddressInvalidInviteMock(
                hdid,
                deleted,
                verificationExists);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey);

            // Assert and Verify
            Assert.False(actual);

            VerifyVerificationUpdateValidatedFalse(mock.MessagingVerificationDelegateMock);
        }

        /// <summary>
        /// VerifyEmailAddressAsync throws database exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task VerifyEmailAddressAsyncThrowsDatabaseException()
        {
            // Arrange
            VerifyEmailAddressExceptionMock mock = SetupVerifyEmailAddressThrowsDatabaseExceptionMock();

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(
                async () => { await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey); });
        }

        /// <summary>
        /// GenerateMessagingVerificationAsync.
        /// </summary>
        /// <param name="isVerified">The bool value indicating whether the messaging verification is verified or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGenerateMessagingVerificationAsync(bool isVerified)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            Email email = GenerateEmail();

            MessagingVerification expected = GenerateMessagingVerification(validated: isVerified, email: email, inviteKey: inviteKey);

            GenerateMessagingVerificationMock mock = SetupGenerateMessagingVerificationMock(inviteKey, email);

            // Act
            MessagingVerification actual = await mock.Service.GenerateMessagingVerificationAsync(
                mock.Hdid,
                mock.EmailAddress,
                mock.InviteKey,
                isVerified);

            // Assert
            Assert.Equal(expected.InviteKey, actual.InviteKey);
            Assert.Equal(expected.UserProfileId, actual.UserProfileId);
            Assert.Equal(expected.Validated, actual.Validated);
            Assert.Equal(expected.EmailAddress, actual.EmailAddress);
            Assert.Equal(
                TruncateToSeconds(expected.ExpireDate),
                TruncateToSeconds(actual.ExpireDate));
            actual.Email.ShouldDeepEqual(expected.Email);
            return;

            static DateTime TruncateToSeconds(DateTime dateTime)
            {
                return new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    dateTime.Hour,
                    dateTime.Minute,
                    dateTime.Second,
                    dateTime.Kind);
            }
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
        public async Task ShouldUpdateEmailAddressAsync(bool latestVerificationExists, string? emailAddress)
        {
            // Arrange
            Times expectedVerificationExpireTimes = ConvertToTimes(latestVerificationExists);
            Times expectedVerificationInsertTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));
            Times expectedQueueNewEmailByEntityTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));

            UpdateEmailAddressMock mock = SetupUpdateEmailAddressMock(
                latestVerificationExists,
                emailAddress);

            // Act and Verify
            await mock.Service.UpdateEmailAddressAsync(mock.Hdid, mock.EmailAddress);

            VerifyVerificationExpire(mock.MessagingVerificationDelegateMock, expectedVerificationExpireTimes);
            VerifyVerificationInsert(mock.MessagingVerificationDelegateMock, expectedVerificationInsertTimes);
            VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
            VerifyQueueNotificationSettings(mock.NotificationSettingsServiceMock);
            VerifyQueueNewEmailByEntity(mock.EmailQueueServiceMock, expectedQueueNewEmailByEntityTimes);
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
        [InlineData(true, DbStatusCode.Error, typeof(DatabaseException))]
        public async Task UpdateEmailAddressAsyncThrowsExceptionAsync(
            bool userProfileExists,
            DbStatusCode updateProfileStatus,
            Type expectedException)
        {
            // Arrange
            UpdateEmailAddressExceptionMock mock = SetupUpdateEmailAddressThrowsExceptionMock(
                userProfileExists,
                updateProfileStatus);

            // Act and Assert
            await Assert.ThrowsAsync(
                expectedException,
                async () => { await mock.Service.UpdateEmailAddressAsync(mock.Hdid, mock.EmailAddress); });
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

        private static void VerifyQueueNewEmailByEntity(Mock<IEmailQueueService> emailQueueServiceMock, Times? times = null)
        {
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
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

        private static void VerifyVerificationUpdateValidatedFalse(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MessagingVerification>(
                        mv => mv.Validated == false &&
                              mv.VerificationAttempts == 1),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationUpdateValidatedTrue(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MessagingVerification>(
                        mv => mv.Validated == true),
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

        private static Email GenerateEmail(Guid? emailId = null, string toEmailAddress = MainEmailAddress)
        {
            return new()
            {
                Id = emailId ?? Guid.NewGuid(),
                To = toEmailAddress,
            };
        }

        private static MessagingVerification GenerateMessagingVerification(
            string userProfileId = Hdid,
            string emailAddress = MainEmailAddress,
            int verificationAttempts = 0,
            bool validated = false,
            bool deleted = false,
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
                Email = email ?? GenerateEmail(toEmailAddress: emailAddress),
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

        private static IUserEmailServiceV2 GetUserEmailService(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            bool changeFeedEnabled = false)
        {
            messagingVerificationDelegateMock ??= new();
            userProfileDelegateMock ??= new();
            notificationSettingsServiceMock ??= new();
            messageSenderMock ??= new();
            emailQueueServiceMock ??= new();

            return new UserEmailServiceV2(
                new Mock<ILogger<UserEmailServiceV2>>().Object,
                messagingVerificationDelegateMock.Object,
                userProfileDelegateMock.Object,
                emailQueueServiceMock.Object,
                notificationSettingsServiceMock.Object,
                GetConfiguration(changeFeedEnabled),
                messageSenderMock.Object);
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
                            userProfileId,
                            email: email,
                            inviteKey: matchingVerificationInviteKey,
                            verificationAttempts: matchingVerificationAttempts,
                            validated: matchingVerificationValidated,
                            deleted: matchingVerificationDeleted)
                        : null;

                messagingVerificationDelegateMock.Setup(
                        s => s.GetLastByInviteKeyAsync(
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
                            userProfileId,
                            inviteKey: latestVerificationInviteKey,
                            email: email,
                            deleted: false)
                        : null;

                messagingVerificationDelegateMock.Setup(
                        s => s.GetLastForUserAsync(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(latestEmailVerification);
            }

            return messagingVerificationDelegateMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            bool userProfileExists = true,
            DbStatusCode? dbUpdateStatus = null)
        {
            UserProfile? userProfile = userProfileExists ? new() : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();

            userProfileDelegateMock.Setup(
                    u => u.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            if (dbUpdateStatus != null)
            {
                userProfileDelegateMock.Setup(
                        s => s.UpdateAsync(
                            It.IsAny<UserProfile>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(
                        new DbResult<UserProfile>
                            { Status = dbUpdateStatus.Value });
            }

            return userProfileDelegateMock;
        }

        private static VerifyEmailAddressMock SetupVerifyEmailAddressMock(bool changeFeedEnabled)
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Updated);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                messageSenderMock,
                changeFeedEnabled: changeFeedEnabled);

            return new(
                service,
                messageSenderMock,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                userProfileDelegateMock,
                Hdid,
                inviteKey);
        }

        private static VerifyEmailAddressExceptionMock SetupVerifyEmailAddressTooManyAttemptsMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(
                        matchingVerificationInviteKey: inviteKey,
                        matchingVerificationAttempts: 1000000000); // This will cause too many attempts error.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey);
        }

        private static VerifyEmailAddressExceptionMock SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(
                        matchingVerificationInviteKey: inviteKey,
                        matchingVerificationValidated: true); // This will cause an AlreadyExistsException to be thrown.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey);
        }

        private static VerifyEmailAddressInvalidInviteMock SetupVerifyEmailAddressInvalidInviteMock(
            string userProfileId = Hdid,
            bool matchingVerificationDeleted = false,
            bool matchingVerificationExists = true)
        {
            Guid inviteKey = Guid.NewGuid();

            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(
                    matchingVerificationInviteKey: inviteKey,
                    userProfileId: userProfileId, // See if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    matchingVerificationExists: matchingVerificationExists, // if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    matchingVerificationDeleted: matchingVerificationDeleted, // See if (matchingVerification == null || matchingVerification.UserProfileId != hdid || matchingVerification.Deleted)
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                messagingVerificationDelegateMock,
                Hdid,
                inviteKey);
        }

        private static VerifyEmailAddressExceptionMock SetupVerifyEmailAddressThrowsDatabaseExceptionMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Error); // This will cause a DatabaseException to be thrown.

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey);
        }

        private static EmailQueueServiceMock SetupEmailQueueServiceMock(
            string toEmailAddress,
            string? emailAddress = null,
            Guid? inviteKey = null,
            Email? email = null)
        {
            inviteKey ??= Guid.NewGuid();
            Guid emailId = Guid.NewGuid();
            Guid emailTemplateId = Guid.NewGuid();

            email ??= GenerateEmail(emailId, toEmailAddress);
            EmailTemplate emailTemplate = new()
            {
                Id = emailTemplateId,
                Name = EmailTemplateName.RegistrationTemplate,
            };

            Mock<IEmailQueueService> emailQueueServiceMock = new();
            emailQueueServiceMock.Setup(
                    s => s.GetEmailTemplateAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailTemplate);

            emailQueueServiceMock.Setup(
                    s => s.ProcessTemplate(
                        It.IsAny<string>(),
                        It.IsAny<EmailTemplate>(),
                        It.IsAny<Dictionary<string, string>>()))
                .Returns(email);

            return new(emailQueueServiceMock, Hdid, inviteKey.Value, emailAddress);
        }

        private static GenerateMessagingVerificationMock SetupGenerateMessagingVerificationMock(Guid inviteKey, Email? email)
        {
            EmailQueueServiceMock emailQueueServiceMock =
                SetupEmailQueueServiceMock(
                    MainEmailAddress,
                    MainEmailAddress,
                    inviteKey,
                    email);
            IUserEmailServiceV2 service = GetUserEmailService(emailQueueServiceMock: emailQueueServiceMock.Service);

            return new(
                service,
                emailQueueServiceMock.Hdid,
                emailQueueServiceMock.InviteKey,
                emailQueueServiceMock.EmailAddress);
        }

        private static UpdateEmailAddressMock SetupUpdateEmailAddressMock(
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
            EmailQueueServiceMock emailQueueServiceMock = SetupEmailQueueServiceMock(MainEmailAddress, emailAddress);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock: emailQueueServiceMock.Service);

            return new(
                service,
                emailQueueServiceMock.Service,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                userProfileDelegateMock,
                emailQueueServiceMock.Hdid,
                emailQueueServiceMock.EmailAddress);
        }

        private static UpdateEmailAddressExceptionMock SetupUpdateEmailAddressThrowsExceptionMock(
            bool userProfileExists = true, // if false, NotFoundException is thrown
            DbStatusCode updateProfileStatus = DbStatusCode.Updated) // if DbStatusCode.Error, DatabaseException is thrown
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfileExists, updateProfileStatus);
            EmailQueueServiceMock emailQueueServiceMock = SetupEmailQueueServiceMock(MainEmailAddress, MainEmailAddress);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock: emailQueueServiceMock.Service);

            return new(
                service,
                Hdid,
                MainEmailAddress);
        }

        private sealed record EmailQueueServiceMock(
            Mock<IEmailQueueService> Service,
            string Hdid,
            Guid InviteKey,
            string EmailAddress);

        private sealed record VerifyEmailAddressMock(
            IUserEmailServiceV2 Service,
            Mock<IMessageSender> MessageSenderMock,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            string Hdid,
            Guid InviteKey);

        private sealed record VerifyEmailAddressInvalidInviteMock(
            IUserEmailServiceV2 Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            string Hdid,
            Guid InviteKey);

        private sealed record VerifyEmailAddressExceptionMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            Guid InviteKey);

        private sealed record GenerateMessagingVerificationMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            Guid InviteKey,
            string EmailAddress);

        private sealed record UpdateEmailAddressMock(
            IUserEmailServiceV2 Service,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            string Hdid,
            string EmailAddress);

        private sealed record UpdateEmailAddressExceptionMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            string EmailAddress);
    }
}
