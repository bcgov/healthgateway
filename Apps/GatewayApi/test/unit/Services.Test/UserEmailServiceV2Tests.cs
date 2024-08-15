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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
        public async Task ShouldVerifyEmailAddress(bool changeFeedEnabled)
        {
            // Arrange
            Guid inviteKey = Guid.NewGuid();
            Times expectedNotificationChannelVerifiedEventTimes = ConvertToTimes(changeFeedEnabled);

            EmailAddressMock mock = SetupVerifyEmailAddressMock(inviteKey, changeFeedEnabled);

            // Act
            bool actual = await mock.Service.VerifyEmailAddressAsync(Hdid, inviteKey, CancellationToken.None);

            // Assert and Verify
            Assert.True(actual);

            VerifyVerificationUpdateValidatedTrue(mock.MessagingVerificationDelegateMock);
            VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
            VerifyPushNotificationSettingsToPhsa(mock.JobServiceMock);
            VerifyNotifyEmailVerification(mock.JobServiceMock, expectedNotificationChannelVerifiedEventTimes);
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
            await Assert.ThrowsAsync<AlreadyExistsException>(
                async () => { await service.VerifyEmailAddressAsync(Hdid, inviteKey); });
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
            await Assert.ThrowsAsync<DatabaseException>(
                async () => { await service.VerifyEmailAddressAsync(Hdid, inviteKey); });
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
            Times expectedVerificationExpireTimes = ConvertToTimes(latestVerificationExists);
            Times expectedVerificationInsertTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));
            Times expectedQueueNewEmailByEntityTimes = ConvertToTimes(!string.IsNullOrEmpty(emailAddress));

            EmailAddressMock mock = SetupUpdateEmailAddressMock(
                Hdid,
                latestVerificationExists,
                emailAddress);

            // Act and Verify
            await mock.Service.UpdateEmailAddressAsync(Hdid, emailAddress);

            VerifyVerificationExpire(mock.MessagingVerificationDelegateMock, expectedVerificationExpireTimes);
            VerifyVerificationInsert(mock.MessagingVerificationDelegateMock, expectedVerificationInsertTimes);
            VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
            VerifyPushNotificationSettingsToPhsa(mock.JobServiceMock);
            VerifySendEmail(mock.JobServiceMock, expectedQueueNewEmailByEntityTimes);
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

        private static void VerifyUserProfileUpdate(Mock<IUserProfileDelegate> userProfileDelegateMock, Times? times = null)
        {
            userProfileDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifySendEmail(Mock<IJobService> jobServiceMock, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.SendEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyNotifyEmailVerification(Mock<IJobService> jobServiceMock, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.NotifyEmailVerificationAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
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

        private static void VerifyPushNotificationSettingsToPhsa(Mock<IJobService> jobServiceMock, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.PushNotificationSettingsToPhsaAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
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

        private static IUserEmailServiceV2 GetUserEmailService(
            Mock<IJobService>? jobServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IMessagingVerificationService>? messagingVerificationServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            bool changeFeedEnabled = false)
        {
            jobServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();
            messagingVerificationServiceMock ??= new();
            userProfileDelegateMock ??= new();

            return new UserEmailServiceV2(
                new Mock<ILogger<UserEmailServiceV2>>().Object,
                messagingVerificationDelegateMock.Object,
                messagingVerificationServiceMock.Object,
                userProfileDelegateMock.Object,
                jobServiceMock.Object,
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
                            MessagingVerificationType.Email,
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

        private static Mock<IMessagingVerificationService> SetupMessagingVerificationServiceMock(MessagingVerification messagingVerification)
        {
            Mock<IMessagingVerificationService> messagingVerificationMock = new();

            if (messagingVerification.VerificationType == MessagingVerificationType.Email)
            {
                messagingVerificationMock.Setup(
                        s => s.GenerateMessagingVerificationAsync(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<Guid>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(messagingVerification);
            }

            return messagingVerificationMock;
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

        private static EmailAddressMock SetupVerifyEmailAddressMock(Guid inviteKey, bool changeFeedEnabled)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(matchingVerificationInviteKey: inviteKey);
            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Updated);
            Mock<IJobService> jobServiceMock = new();

            IUserEmailServiceV2 service = GetUserEmailService(
                jobServiceMock,
                messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock,
                changeFeedEnabled: changeFeedEnabled);

            return new(
                service,
                jobServiceMock,
                messagingVerificationDelegateMock,
                userProfileDelegateMock);
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
                SetupUserProfileDelegateMock(dbUpdateStatus: DbStatusCode.Error); // This will cause a DatabaseException to be thrown.

            return GetUserEmailService(
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userProfileDelegateMock: userProfileDelegateMock);
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
            Mock<IJobService> jobServiceMock = new();

            Mock<IMessagingVerificationService>? messagingVerificationServiceMock = null;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                Email email = GenerateEmail(toEmailAddress: emailAddress);
                MessagingVerification messagingVerification = GenerateMessagingVerification(MessagingVerificationType.Email, hdid, email: email);
                messagingVerificationServiceMock = SetupMessagingVerificationServiceMock(messagingVerification);
            }

            IUserEmailServiceV2 service = GetUserEmailService(
                jobServiceMock,
                messagingVerificationDelegateMock,
                messagingVerificationServiceMock,
                userProfileDelegateMock);

            return new(
                service,
                jobServiceMock,
                messagingVerificationDelegateMock,
                userProfileDelegateMock);
        }

        private static IUserEmailServiceV2 SetupUpdateEmailAddressThrowsExceptionMock(
            bool userProfileExists = true, // if false, NotFoundException is thrown
            DbStatusCode updateProfileStatus = DbStatusCode.Updated) // if DbStatusCode.Error, DatabaseException is thrown
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(
                    setupMatchingVerification: false,
                    setupLatestVerification: true);

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfileExists, updateProfileStatus);
            Mock<IJobService> jobServiceMock = new();
            Mock<IMessagingVerificationService> messagingVerificationServiceMock = new();

            return GetUserEmailService(
                jobServiceMock,
                messagingVerificationDelegateMock,
                messagingVerificationServiceMock,
                userProfileDelegateMock);
        }

        private sealed record EmailAddressMock(
            IUserEmailServiceV2 Service,
            Mock<IJobService> JobServiceMock,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock);

        private sealed record VerifyEmailAddressInvalidInviteMock(
            IUserEmailServiceV2 Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock);
    }
}
