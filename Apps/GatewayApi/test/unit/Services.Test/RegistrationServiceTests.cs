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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using FluentValidation;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Microsoft.Extensions.Configuration;
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
        private const string EncryptionKey = "encryption-key";

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
        public async Task ShouldCreateUserProfile(
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            string? jwtEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            // Arrange
            CreateUserRequest createUserRequest = new()
            {
                Profile = new(Hdid, Guid.NewGuid(), requestedSmsNumber, requestedEmailAddress),
            };

            DateTime currentUtcDate = DateTime.UtcNow.Date;

            bool isEmailVerified =
                !string.IsNullOrWhiteSpace(requestedEmailAddress)
                && string.Equals(requestedEmailAddress, jwtEmailAddress, StringComparison.OrdinalIgnoreCase);

            Times expectedSendEmailTimes = ConvertToTimes(!isEmailVerified && !string.IsNullOrWhiteSpace(requestedEmailAddress));
            Times expectedNotifyAccountCreationTimes = ConvertToTimes(accountsChangeFeedEnabled);
            Times expectedNotifyEmailVerificationTimes = ConvertToTimes(isEmailVerified && notificationsChangeFeedEnabled);

            UserProfileModel expected = GenerateUserProfileModel(currentUtcDate, requestedEmailAddress, requestedSmsNumber);

            (IRegistrationService service, Mock<IJobService> jobServiceMock) = SetupCreateUserProfileMock(
                currentUtcDate,
                requestedSmsNumber,
                requestedEmailAddress,
                minPatientAge,
                patientAge,
                accountsChangeFeedEnabled,
                notificationsChangeFeedEnabled);

            // Act
            UserProfileModel actual = await service.CreateUserProfileAsync(
                createUserRequest,
                DateTime.UtcNow,
                jwtEmailAddress);

            // Assert and Verify
            actual.ShouldDeepEqual(expected);

            VerifyPushNotificationSettings(jobServiceMock);
            VerifyNotifyEmailVerification(jobServiceMock, expectedNotifyEmailVerificationTimes);
            VerifyNotifyAccountCreation(jobServiceMock, expectedNotifyAccountCreationTimes);
            VerifySendEmail(jobServiceMock, expectedSendEmailTimes);
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
        public async Task CreateUserProfileThrowsException(
            string? requestedSmsNumber,
            int minPatientAge,
            int patientAge,
            DbStatusCode? profileInsertStatus,
            Type expectedException)
        {
            // Arrange
            CreateUserRequest createUserRequest = new()
            {
                Profile = new(Hdid, Guid.NewGuid(), requestedSmsNumber, EmailAddress),
            };

            IRegistrationService service = SetupRegistrationServiceForCreateUserProfileThrowsException(
                requestedSmsNumber,
                minPatientAge,
                patientAge,
                profileInsertStatus);

            // Act and assert
            await Assert.ThrowsAsync(
                expectedException,
                async () =>
                {
                    await service.CreateUserProfileAsync(
                        createUserRequest,
                        DateTime.Today,
                        EmailAddress);
                });
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

        private static void VerifyPushNotificationSettings(Mock<IJobService> jobServiceMock, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.PushNotificationSettingsToPhsaAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }

        private static void VerifyNotifyAccountCreation(Mock<IJobService> jobServiceMock, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.NotifyAccountCreationAsync(
                    It.IsAny<string>(),
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
            Guid? termsOfServiceId = null,
            string encryptionKey = EncryptionKey)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = hdid,
                TermsOfServiceId = termsOfServiceId ?? TermsOfServiceGuid,
                Email = email,
                SmsNumber = smsNumber,
                ClosedDateTime = closedDateTime,
                EncryptionKey = encryptionKey,
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
                LastLoginClientCode = UserLoginClientType.Ios,
                CreatedBy = hdid,
                UpdatedBy = Hdid,
                BetaFeatureCodes = [],
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
                BetaFeatures = [],
                BlockedDataSources = [],
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
            Mock<IMessagingVerificationService>? messagingVerificationServiceMock = null,
            Mock<IJobService>? jobServiceMock = null,
            Mock<IPatientDetailsService>? patientDetailsServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IUserProfileModelService>? userProfileModelServiceMock = null)
        {
            configurationRoot ??= GetIConfiguration();
            messagingVerificationServiceMock ??= new();
            jobServiceMock ??= new();
            patientDetailsServiceMock ??= new();
            userProfileDelegateMock ??= new();
            userProfileModelServiceMock ??= new();

            return new RegistrationService(
                configurationRoot,
                new Mock<IAuthenticationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                messagingVerificationServiceMock.Object,
                jobServiceMock.Object,
                patientDetailsServiceMock.Object,
                userProfileDelegateMock.Object,
                userProfileModelServiceMock.Object);
        }

        private static Mock<IMessagingVerificationService> SetupMessagingVerificationServiceMock(MessagingVerification emailVerification, MessagingVerification smsVerification)
        {
            Mock<IMessagingVerificationService> messagingVerificationServiceMock = new();

            messagingVerificationServiceMock.Setup(
                    s => s.AddEmailVerificationAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailVerification);

            messagingVerificationServiceMock.Setup(
                    s => s.AddSmsVerificationAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(smsVerification);

            return messagingVerificationServiceMock;
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

        private static Mock<IUserProfileModelService> SetupUserProfileModelServiceMock(UserProfileModel userProfileModel)
        {
            Mock<IUserProfileModelService> userProfileModelServiceMock = new();

            userProfileModelServiceMock.Setup(
                    s => s.BuildUserProfileModelAsync(
                        It.IsAny<UserProfile>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileModel);

            return userProfileModelServiceMock;
        }

        private static UserProfileMock SetupCreateUserProfileMock(
            DateTime currentDateTime,
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            Mock<IJobService> jobServiceMock = new();

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(GenerateBirthDate(patientAge)));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            UserProfile insertUserProfile = GenerateUserProfile(
                loginDate: currentDateTime,
                email: requestedEmailAddress,
                smsNumber: requestedSmsNumber);
            DbResult<UserProfile> insertProfileResult = GenerateUserProfileDbResult(DbStatusCode.Created, insertUserProfile);
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            UserProfileModel userProfileModel = GenerateUserProfileModel(currentDateTime, requestedEmailAddress, requestedSmsNumber);
            Mock<IUserProfileModelService> userProfileModelServiceMock = SetupUserProfileModelServiceMock(userProfileModel);

            MessagingVerification emailVerification = GenerateMessagingVerification(emailAddress: requestedEmailAddress);
            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IMessagingVerificationService> messagingVerificationServiceMock = SetupMessagingVerificationServiceMock(emailVerification, smsVerification);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge,
                accountsChangeFeedEnabled,
                notificationsChangeFeedEnabled);

            IRegistrationService service = GetRegistrationService(
                configuration,
                messagingVerificationServiceMock,
                jobServiceMock,
                patientDetailsServiceMock,
                userProfileDelegateMock,
                userProfileModelServiceMock);

            return new(
                service,
                jobServiceMock);
        }

        private static IRegistrationService SetupRegistrationServiceForCreateUserProfileThrowsException(
            string? requestedSmsNumber,
            int minPatientAge,
            int patientAge,
            DbStatusCode? profileInsertStatus)
        {
            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(GenerateBirthDate(patientAge)));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            DbResult<UserProfile>? insertProfileResult =
                profileInsertStatus != null ? GenerateUserProfileDbResult(profileInsertStatus.Value) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            MessagingVerification emailVerification = GenerateMessagingVerification(emailAddress: EmailAddress);
            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IMessagingVerificationService> messagingVerificationServiceMock = SetupMessagingVerificationServiceMock(emailVerification, smsVerification);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge: minPatientAge);

            return GetRegistrationService(
                configuration,
                messagingVerificationServiceMock,
                patientDetailsServiceMock: patientDetailsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock);
        }

        private sealed record UserProfileMock(
            IRegistrationService Service,
            Mock<IJobService> JobServiceMock);
    }
}
