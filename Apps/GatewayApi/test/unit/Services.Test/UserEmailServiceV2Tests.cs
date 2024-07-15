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
        /// ValidateEmailAsync - Happy path scenario.
        /// </summary>
        /// <param name="changeFeedEnabled">
        /// The bool value indicating whether change feed on notifications is enabled or not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ValidateEmail(bool changeFeedEnabled)
        {
            // Arrange
            VerifyEmailAddressMock mock = SetupVerifyEmailAddressMock(changeFeedEnabled);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey, CancellationToken.None);

            // Assert and Verify
            actual.ShouldDeepEqual(mock.Expected);
            Verify(mock.Verify);
        }

        /// <summary>
        /// ValidateEmailAsync - Too many attempts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailTooManyAttempts()
        {
            // Arrange
            VerifyEmailAddressMock mock = SetupVerifyEmailAddressTooManyAttemptsMock();

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey, CancellationToken.None);

            // Assert and Verify
            actual.ShouldDeepEqual(mock.Expected);
            Verify(mock.Verify);
        }

        /// <summary>
        /// ValidateEmailAsync - Already validated.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailAlreadyValidated()
        {
            // Arrange
            VerifyEmailAddressThrowsExceptionMock mock = SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock();

            // Act and assert
            await Assert.ThrowsAsync(
                mock.Expected, // AlreadyExistsException
                async () => { await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey); });
        }

        /// <summary>
        /// ValidateEmailAsync - invalid invite.
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
        public async Task InvalidInviteLastSent(string hdid, bool verificationExists, bool deleted)
        {
            // Arrange
            VerifyEmailAddressMock mock = SetupVerifyEmailAddressInvalidInviteMock(
                hdid,
                deleted,
                verificationExists);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(mock.Hdid, mock.InviteKey);

            // Assert and Verify
            actual.ShouldDeepEqual(mock.Expected);
            Verify(mock.Verify);
        }

        /// <summary>
        /// ValidateEmailAsync - Update UserProfile database exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ValidateEmailThrowsDatabaseException()
        {
            // Arrange
            VerifyEmailAddressThrowsExceptionMock mock = SetupVerifyEmailAddressThrowsDatabaseExceptionMock();

            // Act and Assert
            await Assert.ThrowsAsync(
                mock.Expected,
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
        public async Task GenerateMessagingVerificationAsync(bool isVerified)
        {
            // Arrange
            GenerateMessagingVerificationMock mock = SetupGenerateMessagingVerificationMock(isVerified);

            // Act
            MessagingVerification actual = await mock.Service.GenerateMessagingVerificationAsync(mock.Hdid, mock.EmailAddress, mock.InviteKey, isVerified);

            // Assert
            Assert.Equal(mock.Expected.InviteKey, actual.InviteKey);
            Assert.Equal(mock.Expected.UserProfileId, actual.UserProfileId);
            Assert.Equal(mock.Expected.Validated, actual.Validated);
            Assert.Equal(mock.Expected.EmailAddress, actual.EmailAddress);
            Assert.Equal(
                TruncateToSeconds(mock.Expected.ExpireDate),
                TruncateToSeconds(actual.ExpireDate)
            );
            actual.Email.ShouldDeepEqual(mock.Expected.Email);
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
                    dateTime.Kind
                );
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
        public async Task UpdateEmailAddressAsync(bool latestVerificationExists, string? emailAddress)
        {
            // Arrange
            UpdateEmailAddressMock mock = SetupUpdateEmailAddressMock(
                latestVerificationExists,
                emailAddress: emailAddress);

            // Act and Verify
            await mock.Service.UpdateEmailAddressAsync(mock.Hdid, mock.EmailAddress);
            Verify(mock.Verify);
        }

        /// <summary>
        /// UpdateEmailAddressAsync throws exception.
        /// </summary>
        /// <param name="userProfileExists">
        /// The bool value indicating whether the user profile exists or not.
        /// </param>
        /// <param name="userProfileUpdateStatus">The status returned when user profile is updated.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, DbStatusCode.Updated)] // Throws NotFoundException
        [InlineData(true, DbStatusCode.Error)] // Throws DatabaseException
        public async Task UpdateEmailAddressThrowsExceptionAsync(bool userProfileExists, DbStatusCode userProfileUpdateStatus)
        {
            // Arrange
            UpdateEmailAddressThrowsExceptionMock mock = SetupUpdateEmailAddressThrowsExceptionMock(
                userProfileExists,
                userProfileUpdateStatus);

            // Act and Assert
            await Assert.ThrowsAsync(
                mock.Expected,
                async () => { await mock.Service.UpdateEmailAddressAsync(mock.Hdid, mock.EmailAddress); });
        }

        private static void Verify(VerifyMock mock)
        {
            mock.MessagingVerificationDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MessagingVerification>(
                        mv => mv.Validated == true),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedValidVerificationUpdateTimes);

            mock.MessagingVerificationDelegateMock.Verify(
                s => s.UpdateAsync(
                    It.Is<MessagingVerification>(
                        mv => mv.Validated == false &&
                              mv.VerificationAttempts == 1),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedInvalidVerificationUpdateTimes);

            mock.MessagingVerificationDelegateMock.Verify(
                s => s.ExpireAsync(
                    It.IsAny<MessagingVerification>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedVerificationExpireTimes);

            mock.MessagingVerificationDelegateMock.Verify(
                s => s.InsertAsync(
                    It.IsAny<MessagingVerification>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedVerificationInsertTimes);

            mock.UserProfileDelegateMock
                .Verify(
                    s => s.UpdateAsync(
                        It.IsAny<UserProfile>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()),
                    mock.ExpectedUserProfileUpdateTimes);

            mock.NotificationSettingsServiceMock
                .Verify(
                    s => s.QueueNotificationSettingsAsync(
                        It.IsAny<NotificationSettingsRequest>(),
                        It.IsAny<CancellationToken>()),
                    mock.ExpectedQueueNotificationSettingsTimes);

            mock.MessageSenderMock.Verify(
                m => m.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    CancellationToken.None),
                mock.ExpectedMessageSenderSendTimes);

            mock.EmailQueueServiceMock
                .Verify(
                    s => s.QueueNewEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()),
                    mock.ExpectedQueueNewEmailTimes);
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
                .AddInMemoryCollection(myConfiguration.ToList())
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

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(bool userProfileExists = true, DbStatusCode? dbUpdateStatus = null)
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

        private static VerifyMock SetupVerifyMock(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            bool expectValidVerificationUpdate = false,
            bool expectInvalidVerificationUpdate = false,
            bool expectVerificationExpire = false,
            bool expectVerificationInsert = false,
            bool expectUserProfileUpdate = false,
            bool expectQueueNotificationSettings = false,
            bool expectQueueNewEmail = false,
            bool expectMessageSenderSend = false)
        {
            messagingVerificationDelegateMock ??= new();
            userProfileDelegateMock ??= new();
            notificationSettingsServiceMock ??= new();
            emailQueueServiceMock ??= new();
            messageSenderMock ??= new();

            return new(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock,
                messageSenderMock,
                Convert(expectValidVerificationUpdate), // ExpectedValidVerificationUpdateTimes
                Convert(expectInvalidVerificationUpdate), // ExpectedInvalidVerificationUpdateTimes
                Convert(expectVerificationExpire), // ExpectedVerificationExpireTimes
                Convert(expectVerificationInsert), // ExpectedVerificationInsertTime
                Convert(expectUserProfileUpdate), // ExpectedUserProfileUpdateTimes
                Convert(expectQueueNotificationSettings), // ExpectedQueueNotificationSettingsTimes
                Convert(expectQueueNewEmail), // ExpectedQueueNewEmailTimes
                Convert(expectMessageSenderSend)); // ExpectedMessageSenderSendTimes

            static Times Convert(bool expect)
            {
                return expect ? Times.Once() : Times.Never();
            }
        }

        private static VerifyEmailAddressMock SetupVerifyEmailAddressMock(bool changeFeedEnabled)
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Updated);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                messageSenderMock,
                changeFeedEnabled: changeFeedEnabled);

            VerifyMock verifyMock = SetupVerifyMock(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                messageSenderMock: messageSenderMock,
                expectValidVerificationUpdate: true,
                expectUserProfileUpdate: true,
                expectQueueNotificationSettings: true,
                expectMessageSenderSend: changeFeedEnabled);

            return new(
                service,
                Hdid,
                inviteKey,
                true, // Valid email
                verifyMock);
        }

        private static VerifyEmailAddressMock SetupVerifyEmailAddressTooManyAttemptsMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey, matchingVerificationAttempts: 1000000000); // This will cause too many attempts error.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            VerifyMock verifyMock = SetupVerifyMock(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey,
                false, // Invalid email
                verifyMock);
        }

        private static VerifyEmailAddressThrowsExceptionMock SetupVerifyEmailAddressThrowsAlreadyExistsExceptionMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate>
                messagingVerificationDelegateMock =
                    SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey, matchingVerificationValidated: true); // This will cause an AlreadyExistsException to be thrown.
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey,
                typeof(AlreadyExistsException)); // Invalid email - exception is thrown
        }

        private static VerifyEmailAddressMock SetupVerifyEmailAddressInvalidInviteMock(
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

            VerifyMock verifyMock = SetupVerifyMock(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                expectInvalidVerificationUpdate: true);

            return new(
                service,
                Hdid,
                inviteKey,
                false, // Invalid email
                verifyMock);
        }

        private static VerifyEmailAddressThrowsExceptionMock SetupVerifyEmailAddressThrowsDatabaseExceptionMock()
        {
            Guid inviteKey = Guid.NewGuid();
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Error); // This will cause a DatabaseException to be thrown.

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock);

            return new(
                service,
                Hdid,
                inviteKey,
                typeof(DatabaseException)); // Invalid email - exception is thrown
        }

        private static EmailQueueServiceMock SetupEmailQueueServiceMock(
            string toEmailAddress,
            string? emailAddress = null,
            bool isVerified = true)
        {
            Guid inviteKey = Guid.NewGuid();
            Guid emailId = Guid.NewGuid();
            Guid emailTemplateId = Guid.NewGuid();

            Email email = GenerateEmail(emailId, toEmailAddress);
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

            MessagingVerification expected = GenerateMessagingVerification(
                inviteKey: inviteKey,
                validated: isVerified,
                email: email);

            return new(emailQueueServiceMock, Hdid, inviteKey, emailAddress, expected);
        }

        private static GenerateMessagingVerificationMock SetupGenerateMessagingVerificationMock(bool isVerified)
        {
            EmailQueueServiceMock emailQueueServiceMock = SetupEmailQueueServiceMock(MainEmailAddress, MainEmailAddress, isVerified);
            IUserEmailServiceV2 service = GetUserEmailService(emailQueueServiceMock: emailQueueServiceMock.Service);

            return new(
                service,
                emailQueueServiceMock.Hdid,
                emailQueueServiceMock.InviteKey,
                emailQueueServiceMock.EmailAddress,
                emailQueueServiceMock.Expected);
        }

        private static UpdateEmailAddressMock SetupUpdateEmailAddressMock(
            bool latestVerificationExists,
            DbStatusCode userProfileUpdateStatus = DbStatusCode.Updated,
            string? emailAddress = null)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true,
                    latestVerificationExists: latestVerificationExists,
                    latestVerificationEmailAddress: MainEmailAddress);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(dbUpdateStatus: userProfileUpdateStatus);
            EmailQueueServiceMock emailQueueServiceMock = SetupEmailQueueServiceMock(MainEmailAddress, emailAddress);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock: emailQueueServiceMock.Service);

            VerifyMock verifyMock = SetupVerifyMock(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock.Service,
                expectVerificationExpire: latestVerificationExists,
                expectVerificationInsert: !string.IsNullOrEmpty(emailAddress),
                expectUserProfileUpdate: true,
                expectQueueNotificationSettings: true,
                expectQueueNewEmail: !string.IsNullOrEmpty(emailAddress));

            return new(
                service,
                emailQueueServiceMock.Hdid,
                emailQueueServiceMock.EmailAddress,
                verifyMock);
        }

        private static UpdateEmailAddressThrowsExceptionMock SetupUpdateEmailAddressThrowsExceptionMock(
            bool userProfileExists = true, // if false, NotFoundException is thrown
            DbStatusCode userProfileUpdateStatus = DbStatusCode.Updated) // if DbStatusCode.Error, DatabaseException is thrown
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfileExists, userProfileUpdateStatus);
            EmailQueueServiceMock emailQueueServiceMock = SetupEmailQueueServiceMock(MainEmailAddress, MainEmailAddress);
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock: emailQueueServiceMock.Service);

            Type expected = !userProfileExists ? typeof(NotFoundException) : typeof(DatabaseException);

            return new(
                service,
                Hdid,
                MainEmailAddress,
                expected);
        }

        private sealed record EmailQueueServiceMock(
            Mock<IEmailQueueService> Service,
            string Hdid,
            Guid InviteKey,
            string EmailAddress,
            MessagingVerification Expected);

        private sealed record VerifyEmailAddressMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            Guid InviteKey,
            bool Expected,
            VerifyMock Verify);

        private sealed record VerifyEmailAddressThrowsExceptionMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            Guid InviteKey,
            Type Expected);

        private sealed record GenerateMessagingVerificationMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            Guid InviteKey,
            string EmailAddress,
            MessagingVerification Expected);

        private sealed record UpdateEmailAddressMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            string EmailAddress,
            VerifyMock Verify);

        private sealed record UpdateEmailAddressThrowsExceptionMock(
            IUserEmailServiceV2 Service,
            string Hdid,
            string EmailAddress,
            Type Expected);

        private sealed record VerifyMock(
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            Mock<IMessageSender> MessageSenderMock,
            Times ExpectedValidVerificationUpdateTimes,
            Times ExpectedInvalidVerificationUpdateTimes,
            Times ExpectedVerificationExpireTimes,
            Times ExpectedVerificationInsertTimes,
            Times ExpectedUserProfileUpdateTimes,
            Times ExpectedQueueNotificationSettingsTimes,
            Times ExpectedQueueNewEmailTimes,
            Times ExpectedMessageSenderSendTimes);
    }
}
