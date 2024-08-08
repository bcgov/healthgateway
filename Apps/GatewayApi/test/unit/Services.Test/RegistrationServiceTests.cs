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
namespace HealthGateway.GatewayApiTests.Services.Test
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using FluentValidation;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Common.Messaging;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// RegistrationService's Unit Tests.
    /// </summary>
    public class RegistrationServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";
        private const string InvalidSmsNumber = "xxx000xxxx";

        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// CreateUserProfileAsync.
        /// </summary>
        /// <param name="requestedSmsNumber">The value representing the requested sms number.</param>
        /// <param name="requestedEmailAddress">The value representing the requested email address.</param>
        /// <param name="jwtEmailAddress">The value representing the jwt email address.</param>
        /// <param name="minPatientAge">The value representing the valid minimum age to create a profile.</param>
        /// <param name="patientAge">The value representing the patient's age.</param>
        /// <param name="accountsChangeFeedEnabled">The value indicates whether accounts change feed has been enabled or not.</param>
        /// <param name="notificationsChangeFeedEnabled">
        /// The value indicates whether notification change feed has been enabled or
        /// not.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(SmsNumber, EmailAddress, EmailAddress, 18, 18, true, true)] // Happy path
        [InlineData(SmsNumber, EmailAddress, EmailAddress, 18, 19, false, false)] // Happy path
        [InlineData(null, null, EmailAddress, 18, 18, true, true)] // Both sms and email are null in request
        [InlineData("", "", EmailAddress, 18, 18, true, true)] // Both sms and email are empty string in request
        [InlineData(SmsNumber, EmailAddress, null, 18, 18, true, true)] // Jwt email address is null
        [InlineData(SmsNumber, EmailAddress, "", 18, 18, true, true)] // Jwt email address is empty string
        public async Task ShouldCreateUserProfileAsync(
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            string? jwtEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            // Arrange
            DateTime currentUtcDate = DateTime.UtcNow.Date;

            bool isEmailVerified =
                !string.IsNullOrWhiteSpace(requestedEmailAddress)
                && string.Equals(requestedEmailAddress, jwtEmailAddress, StringComparison.OrdinalIgnoreCase);

            Times expectedVerificationSmsInsertTimes = ConvertToTimes(!string.IsNullOrWhiteSpace(requestedSmsNumber));
            Times expectedVerificationEmailInsertTimes = ConvertToTimes(!string.IsNullOrWhiteSpace(requestedEmailAddress));
            Times expectedQueueNewEmailByEntityTimes = ConvertToTimes(!isEmailVerified && !string.IsNullOrWhiteSpace(requestedEmailAddress));
            Times expectedMessageAccountCreatedEventTimes = ConvertToTimes(accountsChangeFeedEnabled);
            Times expectedNotificationChannelVerifiedEventTimes = ConvertToTimes(isEmailVerified && notificationsChangeFeedEnabled);

            UserProfileModel expected = GenerateUserProfileModel(currentUtcDate, requestedEmailAddress, requestedSmsNumber);

            CreateUserProfileMock mock = SetupCreateUserProfileMock(
                currentUtcDate,
                requestedSmsNumber,
                requestedEmailAddress,
                jwtEmailAddress,
                minPatientAge,
                patientAge,
                accountsChangeFeedEnabled,
                notificationsChangeFeedEnabled);

            // Act
            UserProfileModel actual = await mock.Service.CreateUserProfileAsync(
                mock.CreateProfileRequest,
                mock.JwtAuthTime,
                mock.JwtEmailAddress);

            // Assert and Verify
            actual.ShouldDeepEqual(expected);

            VerifyVerificationSmsInsert(mock.MessagingVerificationDelegateMock, expectedVerificationSmsInsertTimes);
            VerifyVerificationEmailInsert(mock.MessagingVerificationDelegateMock, expectedVerificationEmailInsertTimes);
            VerifyQueueNotificationSettingsRequest(mock.NotificationSettingsServiceMock);
            VerifyQueueNewEmailByEntity(mock.EmailQueueServiceMock, expectedQueueNewEmailByEntityTimes);
            VerifyMessageAccountCreatedEvent(mock.MessageSenderMock, expectedMessageAccountCreatedEventTimes);
            VerifyNotificationChannelVerifiedEvent(mock.MessageSenderMock, expectedNotificationChannelVerifiedEventTimes);
        }

        /// <summary>
        /// CreateUserProfileAsync throws Exception.
        /// </summary>
        /// <param name="requestedSmsNumber">The value representing the requested sms number.</param>
        /// <param name="minPatientAge">The value representing the valid minimum age to create a profile.</param>
        /// <param name="patientAge">The value representing the patient's age.</param>
        /// <param name="profileInsertStatus">The db status returned when user profile is inserted in the database </param>
        /// <param name="expectedException">The expected exception type to be thrown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(InvalidSmsNumber, 18, 18, null, typeof(ValidationException))]
        [InlineData(SmsNumber, 18, 17, null, typeof(ValidationException))]
        [InlineData(SmsNumber, 18, 18, DbStatusCode.Error, typeof(DatabaseException))]
        public async Task CreateUserProfileAsyncThrowsException(
            string? requestedSmsNumber,
            int minPatientAge,
            int patientAge,
            DbStatusCode? profileInsertStatus,
            Type expectedException)
        {
            // Arrange
            CreateUserProfileExceptionMock mock = SetupCreateUserProfileThrowsExceptionMock(
                requestedSmsNumber,
                minPatientAge,
                patientAge,
                profileInsertStatus);

            // Act and assert
            await Assert.ThrowsAsync(
                expectedException,
                async () =>
                {
                    await mock.Service.CreateUserProfileAsync(
                        mock.CreateProfileRequest,
                        mock.JwtAuthTime,
                        mock.JwtEmailAddress);
                });
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

        private static void VerifyQueueNewEmailByEntity(Mock<IEmailQueueService> emailQueueServiceMock, Times? times = null)
        {
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<bool>(),
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

        private static void VerifyMessageAccountCreatedEvent(Mock<IMessageSender> messageSenderMock, Times? times = null)
        {
            messageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is AccountCreatedEvent),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationEmailInsert(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email != null
                             && !string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyVerificationSmsInsert(Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock, Times? times = null)
        {
            messagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => !string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email == null
                             && string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static Times ConvertToTimes(bool expected)
        {
            return expected ? Times.Once() : Times.Never();
        }

        private static DateTime GenerateBirthDate(int patientAge = 18)
        {
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            return currentUtcDate.AddYears(-patientAge);
        }

        private static Email GenerateEmail(Guid? emailId = null, string toEmailAddress = EmailAddress)
        {
            return new()
            {
                Id = emailId ?? Guid.NewGuid(),
                To = toEmailAddress,
            };
        }

        private static MessagingVerification GenerateMessagingVerification(
            string smsVerificationCode = SmsVerificationCode,
            bool validated = true,
            Guid? inviteKey = null,
            string? emailAddress = null,
            string? smsNumber = null)
        {
            return new()
            {
                Id = Guid.NewGuid(),
                InviteKey = inviteKey ?? Guid.NewGuid(),
                SmsNumber = smsNumber,
                SmsValidationCode = smsVerificationCode,
                EmailAddress = emailAddress,
                Validated = validated,
                Email = emailAddress != null ? GenerateEmail(toEmailAddress: emailAddress) : null,
            };
        }

        private static PatientDetails GeneratePatientDetails(string hdid = Hdid, DateOnly? birthDate = null)
        {
            return new()
            {
                HdId = hdid,
                Birthdate = birthDate ?? DateOnly.FromDateTime(GenerateBirthDate()),
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
                TermsOfServiceId = TermsOfServiceGuid,
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

        private static UserProfileModel GenerateUserProfileModel(DateTime currentUtcDate, string? email = EmailAddress, string? smsNumber = SmsNumber)
        {
            return new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = email,
                IsEmailVerified = !string.IsNullOrWhiteSpace(email),
                AcceptedTermsOfService = true,
                SmsNumber = smsNumber,
                IsSmsNumberVerified = !string.IsNullOrWhiteSpace(smsNumber),
                HasTermsOfServiceUpdated = true,
                HasTourUpdated = false,
                LastLoginDateTime = currentUtcDate,
                LastLoginDateTimes = [currentUtcDate],
                BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
            };
        }

        private static IConfigurationRoot GetIConfiguration(
            int minPatientAge = 12,
            bool accountsChangeFeedEnabled = false,
            bool notificationsChangeFeedEnabled = false)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
                { "ChangeFeed:Accounts:Enabled", accountsChangeFeedEnabled.ToString() },
                { "ChangeFeed:Notifications:Enabled", notificationsChangeFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IRegistrationService GetRegistrationService(
            IConfigurationRoot? configurationRoot = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IPatientDetailsService>? patientDetailsServiceMock = null,
            Mock<IUserEmailServiceV2>? userEmailServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IUserProfileServiceV2>? userProfileServiceMock = null,
            Mock<IUserSmsServiceV2>? userSmsServiceMock = null)
        {
            configurationRoot ??= GetIConfiguration();
            authenticationDelegateMock ??= new();
            emailQueueServiceMock ??= new();
            messageSenderMock ??= new();
            messagingVerificationDelegateMock ??= new();
            notificationSettingsServiceMock ??= new();
            patientDetailsServiceMock ??= new();
            userEmailServiceMock ??= new();
            userProfileDelegateMock ??= new();
            userProfileServiceMock ??= new();
            userSmsServiceMock ??= new();

            return new RegistrationService(
                configurationRoot,
                new Mock<ILogger<RegistrationService>>().Object,
                authenticationDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                emailQueueServiceMock.Object,
                messageSenderMock.Object,
                messagingVerificationDelegateMock.Object,
                notificationSettingsServiceMock.Object,
                patientDetailsServiceMock.Object,
                userEmailServiceMock.Object,
                userSmsServiceMock.Object,
                userProfileDelegateMock.Object,
                userProfileServiceMock.Object);
        }

        private static Mock<IPatientDetailsService> SetupPatientDetailsServiceMock(PatientDetails patientDetails)
        {
            Mock<IPatientDetailsService> patientDetailsServiceMock = new();
            patientDetailsServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<PatientIdentifierType>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientDetails);

            return patientDetailsServiceMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            DbResult<UserProfile>? insertProfileResult = null,
            DbResult<UserProfile>? updateProfileResult = null)
        {
            userProfileDelegateMock ??= new();

            if (insertProfileResult != null)
            {
                userProfileDelegateMock.Setup(
                        s => s.InsertUserProfileAsync(
                            It.IsAny<UserProfile>(),
                            It.IsAny<bool>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(insertProfileResult);
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

        private static Mock<IUserProfileServiceV2> SetupUserProfileServiceMock(UserProfileModel userProfileModel)
        {
            Mock<IUserProfileServiceV2> userProfileServiceMock = new();

            userProfileServiceMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileModel);

            return userProfileServiceMock;
        }

        private static Mock<IUserEmailServiceV2> SetupUserEmailServiceMock(MessagingVerification messagingVerification)
        {
            Mock<IUserEmailServiceV2> userEmailServiceMock = new();

            userEmailServiceMock.Setup(
                    s => s.GenerateMessagingVerificationAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Guid>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);

            return userEmailServiceMock;
        }

        private static Mock<IUserSmsServiceV2> SetupUserSmsServiceMock(MessagingVerification messagingVerification)
        {
            Mock<IUserSmsServiceV2> userSmsServiceMock = new();

            userSmsServiceMock.Setup(
                    s => s.GenerateMessagingVerification(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                .Returns(messagingVerification);

            return userSmsServiceMock;
        }

        private static CreateUserProfileMock SetupCreateUserProfileMock(
            DateTime currentDateTime,
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            string? jwtEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            CreateUserRequest createUserRequest = new()
            {
                Profile = new(Hdid, Guid.NewGuid(), requestedSmsNumber, requestedEmailAddress),
            };

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(GenerateBirthDate(patientAge)));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            MessagingVerification emailVerification = GenerateMessagingVerification(emailAddress: requestedEmailAddress);
            Mock<IUserEmailServiceV2> userEmailServiceMock = SetupUserEmailServiceMock(emailVerification);

            UserProfile insertUserProfile = GenerateUserProfile(
                loginDate: currentDateTime,
                email: requestedEmailAddress,
                smsNumber: requestedSmsNumber);
            DbResult<UserProfile> insertProfileResult = GenerateUserProfileDbResult(DbStatusCode.Created, insertUserProfile);
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IUserSmsServiceV2> userSmsServiceMock = SetupUserSmsServiceMock(smsVerification);

            UserProfileModel userProfileModel = GenerateUserProfileModel(currentDateTime, requestedEmailAddress, requestedSmsNumber);
            Mock<IUserProfileServiceV2> userProfileServiceMock = SetupUserProfileServiceMock(userProfileModel);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge,
                accountsChangeFeedEnabled,
                notificationsChangeFeedEnabled);

            IRegistrationService service = GetRegistrationService(
                configuration,
                emailQueueServiceMock: emailQueueServiceMock,
                messageSenderMock: messageSenderMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                patientDetailsServiceMock: patientDetailsServiceMock,
                userEmailServiceMock: userEmailServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                userSmsServiceMock: userSmsServiceMock,
                userProfileServiceMock: userProfileServiceMock);

            return new(
                service,
                messagingVerificationDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock,
                messageSenderMock,
                createUserRequest,
                DateTime.UtcNow,
                jwtEmailAddress);
        }

        private static CreateUserProfileExceptionMock SetupCreateUserProfileThrowsExceptionMock(
            string? requestedSmsNumber,
            int minPatientAge,
            int patientAge,
            DbStatusCode? profileInsertStatus)
        {
            CreateUserRequest createUserRequest = new()
            {
                Profile = new(Hdid, Guid.NewGuid(), requestedSmsNumber, EmailAddress),
            };

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(GenerateBirthDate(patientAge)));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            MessagingVerification emailVerification = GenerateMessagingVerification(emailAddress: EmailAddress);
            Mock<IUserEmailServiceV2> userEmailServiceMock = SetupUserEmailServiceMock(emailVerification);

            DbResult<UserProfile>? insertProfileResult =
                profileInsertStatus != null ? GenerateUserProfileDbResult(profileInsertStatus.Value) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IUserSmsServiceV2> userSmsServiceMock = SetupUserSmsServiceMock(smsVerification);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge: minPatientAge);

            IRegistrationService service = GetRegistrationService(
                configuration,
                emailQueueServiceMock: new(),
                messageSenderMock: new(),
                messagingVerificationDelegateMock: new(),
                notificationSettingsServiceMock: new(),
                patientDetailsServiceMock: patientDetailsServiceMock,
                userEmailServiceMock: userEmailServiceMock,
                userSmsServiceMock: userSmsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                userProfileServiceMock: new());

            return new(service, createUserRequest, DateTime.Today, EmailAddress);
        }

        private sealed record CreateUserProfileMock(
            IRegistrationService Service,
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            Mock<IMessageSender> MessageSenderMock,
            CreateUserRequest CreateProfileRequest,
            DateTime JwtAuthTime,
            string? JwtEmailAddress);

        private sealed record CreateUserProfileExceptionMock(
            IRegistrationService Service,
            CreateUserRequest CreateProfileRequest,
            DateTime JwtAuthTime,
            string? JwtEmailAddress);
    }
}
