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
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
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
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using PatientModel = HealthGateway.Common.Models.PatientModel;
    using UserProfileHistory = HealthGateway.Database.Models.UserProfileHistory;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private const string Email = "user@HealthGateway.ca";

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);
        private static readonly string Hdid = Guid.NewGuid().ToString();
        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// GetUserProfileAsync.
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="jwtAuthTimeIsDifferent">The value indicating whether jwt auth time is different from last login or not.</param>
        /// <param name="emailIsVerified">The value indicating whether email is verified or not.</param>
        /// <param name="smsIsVerified">The value indicating whether sms is verified or not.</param>
        /// <param name="tourChangeDateIsLatest">The value indicating whether tour change date is latest or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, true, true, true)]
        [InlineData(true, false, false, true, true)]
        [InlineData(true, false, false, false, true)]
        [InlineData(true, false, false, false, false)]
        [InlineData(false, false, false, false, false)] // Cannot get profile because user profile does not exist
        public async Task ShouldGetUserProfile(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent,
            bool emailIsVerified,
            bool smsIsVerified,
            bool tourChangeDateIsLatest)
        {
            // Arrange
            GetUserProfileMock mock = SetupGetUserProfileMock(
                userProfileExists,
                jwtAuthTimeIsDifferent,
                emailIsVerified,
                smsIsVerified,
                tourChangeDateIsLatest);

            // Act
            RequestResult<UserProfileModel> actual = await mock.Service.GetUserProfileAsync(mock.Hdid, mock.JwtAuthTime);

            // Assert
            mock.Expected.Result.ShouldDeepEqual(actual);

            mock.UserProfileDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesUpdateUserProfile);
        }

        /// <summary>
        /// Validates the Update Accepted Terms of Service.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true)] // Update accepted terms
        [InlineData(DbStatusCode.Updated, false)] // Cannot update because user profile does not exist
        [InlineData(DbStatusCode.Concurrency, true)] // Cannot update due to database error
        [InlineData(DbStatusCode.Error, true)] // Cannot update due to database error
        public async Task ShouldUpdateAcceptedTermsAsync(DbStatusCode updatedStatus, bool userProfileExists)
        {
            // Arrange
            UpdateAcceptedTermsMock mock = SetupUpdateAcceptedTermsMock(updatedStatus, userProfileExists);

            // Act
            RequestResult<UserProfileModel> actual = await mock.Service.UpdateAcceptedTermsAsync(mock.Hdid, mock.TermsOfServiceId);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// CreateUserProfileAsync.
        /// </summary>
        /// <param name="insertedStatus">The status to return from the mock db delegate after the insert.</param>
        /// <param name="accountsFeedEnabled">The value indicates whether accounts change feed has been enabled or not.</param>
        /// <param name="notificationsFeedEnabled">The value indicates whether notification change feed has been enabled or not.</param>
        /// <param name="smsIsValid">The value indicates whether sms in the request is valid or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created, true, true, true)] // Inserted profile with change feed enabled
        [InlineData(DbStatusCode.Created, false, false, true)] // Inserted profile with change feed disabled
        [InlineData(DbStatusCode.Created, false, false, false)] // Cannot insert due to sms validation error
        [InlineData(DbStatusCode.Concurrency, false, false, true)] // Cannot insert due to database error
        [InlineData(DbStatusCode.Error, false, false, true)] // Cannot insert due to database error
        public async Task ShouldICreateUserProfileAsync(DbStatusCode insertedStatus, bool accountsFeedEnabled, bool notificationsFeedEnabled, bool smsIsValid)
        {
            // Arrange
            CreateUserProfileMock mock = SetupCreateUserProfileMock(insertedStatus, accountsFeedEnabled, notificationsFeedEnabled, smsIsValid);

            // Act
            RequestResult<UserProfileModel> actual = await mock.Service.CreateUserProfileAsync(mock.CreateUserRequest, mock.JwtAuthTime, mock.JwtEmailAddress);

            // Assert
            mock.Expected.Result.ShouldDeepEqual(actual);

            mock.UserEmailServiceMock.Verify(
                v => v.CreateUserEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesSendEmail);

            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is AccountCreatedEvent),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesSendAccountCreatedEvent);

            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesSendNotificationChannelVerifiedEvent);

            mock.NotificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(It.IsAny<NotificationSettingsRequest>(), It.IsAny<CancellationToken>()),
                mock.Expected.TimesQueueNotificationSettings);
        }

        /// <summary>
        /// ShouldCloseUserProfile.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <param name="profileClosed">The value indicates whether the user profile is closed or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true, false)] // Close
        [InlineData(DbStatusCode.Updated, true, true)] // Already closed
        [InlineData(DbStatusCode.Updated, false, false)] // Cannot close because profile does not exist
        [InlineData(DbStatusCode.Concurrency, true, false)] // Cannot close because database error
        [InlineData(DbStatusCode.Error, true, false)] // Cannot close because database error
        public async Task ShouldCloseUserProfile(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            // Arrange
            CloseUserProfileMock mock = SetupCloseUerProfileMock(updatedStatus, userProfileExists, profileClosed);

            // Act
            RequestResult<UserProfileModel> actual = await mock.Service.CloseUserProfileAsync(mock.Hdid, mock.UserId);

            // Assert
            mock.Expected.Result.ShouldDeepEqual(actual);

            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == EmailTemplateName.AccountClosedTemplate),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<bool>(x => x == true),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesSendEmail);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <param name="updatedStatus">The status to return from the mock db delegate after the update.</param>
        /// <param name="userProfileExists">The value indicates whether there is a user profile or not.</param>
        /// <param name="profileClosed">The value indicates whether the user profile is closed or not .</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated, true, true)] // Recover
        [InlineData(DbStatusCode.Updated, true, false)] // Already recovered
        [InlineData(DbStatusCode.Updated, false, true)] // Cannot recover because profile does not exist
        [InlineData(DbStatusCode.Concurrency, true, true)] // Cannot recover because database error
        [InlineData(DbStatusCode.Error, true, true)] // Cannot recover because database error
        public async Task ShouldRecoverUserProfile(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            // Arrange
            RecoverUserProfileMock mock = SetupRecoverUserProfileMock(updatedStatus, userProfileExists, profileClosed);

            // Act
            RequestResult<UserProfileModel> actual = await mock.Service.RecoverUserProfileAsync(mock.Hdid);

            // Assert
            mock.Expected.Result.ShouldDeepEqual(actual);

            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == EmailTemplateName.AccountRecoveredTemplate),
                    It.IsAny<Dictionary<string, string>>(),
                    It.Is<bool>(x => x == true),
                    It.IsAny<CancellationToken>()),
                mock.Expected.TimesSendEmail);
        }

        private static PatientModel GeneratePatientModel(DateTime birthDate)
        {
            return new()
            {
                HdId = Hdid,
                Birthdate = birthDate,
            };
        }

        private static UserProfile GenerateUserProfile(DateTime? loginDate = null, int daysFromLoginDate = 0, string? email = null, string? smsNumber = null)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = email,
                SmsNumber = smsNumber,
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
            };
        }

        private static UserProfileHistory GenerateUserProfileHistory(DateTime? loginDate = null, int daysFromLoginDate = 0)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = Hdid,
                Id = Guid.NewGuid(),
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
            };
        }

        private static IConfigurationRoot GetIConfiguration(
            int minPatientAge = 12,
            int profileHistoryRecordLimit = 2,
            bool accountFeedEnabled = false,
            bool notificationFeedEnabled = false)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
                { "WebClient:UserProfileHistoryRecordLimit", profileHistoryRecordLimit.ToString(CultureInfo.InvariantCulture) },
                { "ChangeFeed:Accounts:Enabled", accountFeedEnabled.ToString() },
                { "ChangeFeed:Notifications:Enabled", notificationFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static CreateUserProfileMock SetupCreateUserProfileMock(
            DbStatusCode insertedStatus,
            bool accountsFeedEnabled,
            bool notificationsFeedEnabled,
            bool smsIsValid)
        {
            const int patientAge = 15;
            const int minPatientAge = 10;
            const string jwtEmail = "user@healthgateway.ca";
            const string requestedEmail = "user@healthgateway.ca";
            const string smsVerificationCode = "12345";
            string requestedSms = smsIsValid ? "2505556000" : "0000000000";

            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            DateTime birthDate = currentUtcDate.AddYears(-patientAge).Date;

            PatientModel patientModel = GeneratePatientModel(birthDate);

            CreateUserRequest createUserRequest = new()
            {
                Profile = new(patientModel.HdId, Guid.NewGuid(), requestedSms, requestedEmail),
            };

            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = patientModel,
            };

            UserProfile userProfile = GenerateUserProfile(currentUtcDate);

            DbResult<UserProfile> insertResult = new()
            {
                Payload = userProfile,
                Status = insertedStatus,
                Message = insertedStatus != DbStatusCode.Created ? "DB ERROR" : string.Empty,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.InsertUserProfileAsync(It.Is<UserProfile>(x => x.HdId == Hdid), It.Is<bool>(x => x == !accountsFeedEnabled), It.IsAny<CancellationToken>()))
                .ReturnsAsync(insertResult);

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<PatientIdentifierType>(x => x == PatientIdentifierType.Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IUserEmailService> userEmailServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            MessagingVerification messagingVerification = new()
            {
                Id = Guid.NewGuid(),
                SmsNumber = createUserRequest.Profile.SmsNumber,
                SmsValidationCode = smsVerificationCode,
            };

            Mock<IUserSmsService> userSmsServiceMock = new();
            userSmsServiceMock.Setup(
                    s => s.CreateUserSmsAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == createUserRequest.Profile.SmsNumber),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(messagingVerification);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentUtcDate);

            RequestResult<UserProfileModel>? validationResult = smsIsValid
                ? null
                : new()
                {
                    ResultStatus = ResultType.Error,
                    ResultError = new()
                    {
                        ErrorCode = ErrorTranslator.InternalError(ErrorType.SmsInvalid),
                        ResultMessage = "Profile values entered are invalid",
                    },
                };
            Mock<IUserProfileValidatorService> userProfileValidatorServiceMock = new();
            userProfileValidatorServiceMock.Setup(
                    s => s.ValidateUserProfileAsync(
                        It.IsAny<CreateUserRequest>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge,
                accountFeedEnabled: accountsFeedEnabled,
                notificationFeedEnabled: notificationsFeedEnabled);

            IUserProfileService service = GetUserProfileService(
                patientServiceMock,
                userEmailServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userSmsServiceMock: userSmsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                userProfileValidatorServiceMock: userProfileValidatorServiceMock,
                messageSenderMock: messageSenderMock);

            RequestResult<UserProfileModel> userProfileModelResult = new()
            {
                ResourcePayload = insertResult.Status == DbStatusCode.Created && smsIsValid
                    ? new UserProfileModel
                    {
                        HdId = userProfile.HdId,
                        TermsOfServiceId = TermsOfServiceGuid,
                        Email = requestedEmail,
                        AcceptedTermsOfService = true,
                        IsEmailVerified = true,
                        SmsNumber = requestedSms,
                        HasTermsOfServiceUpdated = true,
                        HasTourUpdated = false,
                        LastLoginDateTime = currentUtcDate,
                    }
                    : null,
                ResultStatus = insertResult.Status == DbStatusCode.Created && smsIsValid ? ResultType.Success : ResultType.Error,
                ResultError = insertResult.Status != DbStatusCode.Created || !smsIsValid
                    ? new()
                    {
                        ErrorCode = smsIsValid ? ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database) : ErrorTranslator.InternalError(ErrorType.SmsInvalid),
                        ResultMessage = smsIsValid ? insertResult.Message : "Profile values entered are invalid",
                    }
                    : null,
            };

            Times timesSendEmail = insertedStatus == DbStatusCode.Created && smsIsValid ? Times.Once() : Times.Never();
            Times timesSendAccountsFeed = insertedStatus == DbStatusCode.Created && smsIsValid && accountsFeedEnabled ? Times.Once() : Times.Never();
            Times timesSendNotificationFeed = insertedStatus == DbStatusCode.Created && smsIsValid && notificationsFeedEnabled ? Times.Once() : Times.Never();
            Times timesQueueNotificationSettings = insertedStatus == DbStatusCode.Created && smsIsValid ? Times.Once() : Times.Never();

            CreatedUserProfileResult expected = new(userProfileModelResult, timesSendEmail, timesSendAccountsFeed, timesSendNotificationFeed, timesQueueNotificationSettings);
            return new(service, userEmailServiceMock, notificationSettingsServiceMock, messageSenderMock, expected, createUserRequest, DateTime.Today, jwtEmail);
        }

        private static GetUserProfileMock SetupGetUserProfileMock(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent,
            bool emailIsVerified,
            bool smsIsVerified,
            bool tourChangeDateIsLatest)
        {
            const int patientAge = 15;
            const int minPatientAge = 10;
            string? smsNumber = smsIsVerified ? "2505556000" : null;
            string? email = emailIsVerified ? Email : null;

            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            DateTime birthDate = currentUtcDate.AddYears(-patientAge).Date;
            DateTime jwtAuthTime = jwtAuthTimeIsDifferent ? currentUtcDate.AddHours(1) : currentUtcDate;
            DateTime latestTourChangeDateTime = tourChangeDateIsLatest ? currentUtcDate : currentUtcDate.AddDays(-5);

            UserProfile? userProfile = userProfileExists ? GenerateUserProfile(currentUtcDate, email: email, smsNumber: smsNumber) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserClientType())
                .Returns(UserLoginClientType.Web);

            PatientModel patientModel = GeneratePatientModel(birthDate);
            RequestResult<PatientModel> patientResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = patientModel,
            };

            Mock<IPatientService> patientServiceMock = new();
            patientServiceMock.Setup(
                    s => s.GetPatientAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<PatientIdentifierType>(x => x == PatientIdentifierType.Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(patientResult);

            IList<UserProfileHistory> userProfileHistoryList =
            [
                GenerateUserProfileHistory(currentUtcDate, 1),
                GenerateUserProfileHistory(currentUtcDate, 2),
            ];

            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileHistoryListAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<int>(x => x == 2),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileHistoryList);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IUserEmailService> userEmailServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourChangeDateTime);

            MessagingVerification emailInvite = new()
            {
                Email = new()
                {
                    Id = Guid.NewGuid(),
                    To = Email,
                },
            };

            MessagingVerification smsInvite = new()
            {
                SmsNumber = smsNumber,
            };

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == MessagingVerificationType.Email),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailInvite);
            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<string>(x => x == MessagingVerificationType.Sms),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(smsInvite);

            IConfigurationRoot configuration = GetIConfiguration(minPatientAge: minPatientAge);

            IUserProfileService service = GetUserProfileService(
                patientServiceMock,
                userEmailServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                authenticationDelegateMock: authenticationDelegateMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                messageSenderMock: messageSenderMock);

            RequestResult<UserProfileModel> userProfileModelResult = new()
            {
                ResourcePayload = userProfileExists
                    ? new UserProfileModel
                    {
                        HdId = Hdid,
                        TermsOfServiceId = TermsOfServiceGuid,
                        Email = email,
                        AcceptedTermsOfService = true,
                        IsEmailVerified = emailIsVerified,
                        IsSmsNumberVerified = smsIsVerified,
                        SmsNumber = smsNumber,
                        HasTermsOfServiceUpdated = true,
                        HasTourUpdated = tourChangeDateIsLatest,
                        LastLoginDateTime = jwtAuthTimeIsDifferent ? jwtAuthTime : currentUtcDate,
                        LastLoginDateTimes =
                        [
                            jwtAuthTimeIsDifferent ? jwtAuthTime : currentUtcDate,
                            userProfileHistoryList[0].LastLoginDateTime,
                            userProfileHistoryList[1].LastLoginDateTime,
                        ],
                    }
                    : new(),
                ResultStatus = ResultType.Success,
            };

            Times timeUpdateUserProfile = jwtAuthTimeIsDifferent ? Times.Once() : Times.Never();

            GetUserProfileResult expected = new(userProfileModelResult, timeUpdateUserProfile);
            return new(service, userProfileDelegateMock, expected, Hdid, jwtAuthTime);
        }

        private static UpdateAcceptedTermsMock SetupUpdateAcceptedTermsMock(DbStatusCode updatedStatus, bool userProfileExists)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile userProfile = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                LastLoginDateTime = lastLoginDateTime,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileExists ? userProfile : null);

            DbResult<UserProfile> updateResult = new()
            {
                Payload = userProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<DataSource>());

            IUserProfileService service = GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);

            RequestResult<UserProfileModel> expected = new()
            {
                ResourcePayload = updateResult.Status == DbStatusCode.Updated && userProfileExists
                    ? new UserProfileModel
                    {
                        HdId = userProfile.HdId,
                        TermsOfServiceId = TermsOfServiceGuid,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = lastLoginDateTime,
                        HasTourUpdated = false,
                    }
                    : null,
                ResultStatus = updateResult.Status != DbStatusCode.Updated || !userProfileExists ? ResultType.Error : ResultType.Success,
                ResultError = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? "Unable to retrieve user profile" : "Unable to update the terms of service: DB Error",
                    }
                    : null,
            };

            return new(service, expected, Hdid, TermsOfServiceGuid);
        }

        private static CloseUserProfileMock SetupCloseUerProfileMock(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            Guid userId = Guid.NewGuid();
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime closedDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile? readUserProfile = userProfileExists
                ? new()
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    ClosedDateTime = profileClosed ? closedDateTime : null,
                    LastLoginDateTime = lastLoginDateTime,
                    Email = "user@healthgateway.ca",
                }
                : null;

            UserProfile updatedUserProfile = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                ClosedDateTime = closedDateTime,
                LastLoginDateTime = lastLoginDateTime,
                IdentityManagementId = userId,
                Email = readUserProfile?.Email,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(readUserProfile);

            DbResult<UserProfile> updateResult = new()
            {
                Payload = updatedUserProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<DataSource>());

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileService service = GetUserProfileService(
                emailQueueServiceMock: emailQueueServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);

            RequestResult<UserProfileModel> userProfileModel = new()
            {
                ResourcePayload = profileClosed || (updateResult.Status == DbStatusCode.Updated && userProfileExists)
                    ? new UserProfileModel
                    {
                        HdId = updatedUserProfile.HdId,
                        TermsOfServiceId = updatedUserProfile.TermsOfServiceId,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = updatedUserProfile.LastLoginDateTime,
                        HasTourUpdated = false,
                        IsEmailVerified = true,
                        Email = updatedUserProfile.Email,
                        ClosedDateTime = updatedUserProfile.ClosedDateTime,
                    }
                    : null,
                ResultStatus = updateResult.Status != DbStatusCode.Updated || !userProfileExists ? ResultType.Error : ResultType.Success,
                ResultError = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? ErrorMessages.UserProfileNotFound : updateResult.Message,
                    }
                    : null,
            };

            Times timesSendEmail = userProfileExists && updatedStatus == DbStatusCode.Updated && !profileClosed ? Times.Once() : Times.Never();
            UserProfileModelResult expected = new(userProfileModel, timesSendEmail);
            return new(service, emailQueueServiceMock, expected, Hdid, userId);
        }

        private static RecoverUserProfileMock SetupRecoverUserProfileMock(DbStatusCode updatedStatus, bool userProfileExists, bool profileClosed)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime latestTourDate = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime closedDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTime lastLoginDateTime = new(2024, 4, 15, 0, 0, 0, DateTimeKind.Utc);

            UserProfile? readUserProfile = userProfileExists
                ? new()
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    ClosedDateTime = profileClosed ? closedDateTime : null,
                    LastLoginDateTime = lastLoginDateTime,
                    Email = "user@healthgateway.ca",
                }
                : null;

            UserProfile updatedUserProfile = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                ClosedDateTime = null,
                IdentityManagementId = null,
                LastLoginDateTime = lastLoginDateTime,
                Email = readUserProfile?.Email,
            };

            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.Is<string>(x => x == Hdid),
                        It.Is<bool>(x => x == false),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(readUserProfile);

            DbResult<UserProfile> updateResult = new()
            {
                Payload = updatedUserProfile,
                Status = updatedStatus,
                Message = updatedStatus != DbStatusCode.Updated ? "DB ERROR" : string.Empty,
            };

            userProfileDelegateMock.Setup(
                    s => s.UpdateAsync(
                        It.Is<UserProfile>(x => x.HdId == Hdid),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResult);

            Mock<ILegalAgreementService> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourDate);

            Mock<IUserPreferenceService> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(
                    s => s.GetDataSourcesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<DataSource>());

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileService service = GetUserProfileService(
                emailQueueServiceMock: emailQueueServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                patientRepositoryMock: patientRepositoryMock);

            RequestResult<UserProfileModel> userProfileModel = new()
            {
                ResourcePayload = !profileClosed || (updateResult.Status == DbStatusCode.Updated && userProfileExists)
                    ? new UserProfileModel
                    {
                        HdId = updatedUserProfile.HdId,
                        TermsOfServiceId = updatedUserProfile.TermsOfServiceId,
                        AcceptedTermsOfService = true,
                        HasTermsOfServiceUpdated = true,
                        LastLoginDateTime = updatedUserProfile.LastLoginDateTime,
                        HasTourUpdated = false,
                        IsEmailVerified = true,
                        Email = updatedUserProfile.Email,
                        ClosedDateTime = updatedUserProfile.ClosedDateTime,
                    }
                    : null,
                ResultStatus = updateResult.Status != DbStatusCode.Updated || !userProfileExists ? ResultType.Error : ResultType.Success,
                ResultError = updateResult.Status != DbStatusCode.Updated || !userProfileExists
                    ? new()
                    {
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                        ResultMessage = !userProfileExists ? ErrorMessages.UserProfileNotFound : updateResult.Message,
                    }
                    : null,
            };

            Times timesSendEmail = userProfileExists && updatedStatus == DbStatusCode.Updated && profileClosed ? Times.Once() : Times.Never();
            UserProfileModelResult expected = new(userProfileModel, timesSendEmail);
            return new(service, emailQueueServiceMock, expected, Hdid);
        }

        private static IUserProfileService GetUserProfileService(
            Mock<IPatientService>? patientServiceMock = null,
            Mock<IUserEmailService>? userEmailServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserSmsService>? userSmsServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<ILegalAgreementService>? legalAgreementServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserPreferenceService>? userPreferenceServiceMock = null,
            IConfigurationRoot? configurationRoot = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IApplicationSettingsService>? applicationSettingsServiceMock = null,
            Mock<IUserProfileValidatorService>? userProfileValidatorServiceMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IMessageSender>? messageSenderMock = null)
        {
            patientServiceMock = patientServiceMock ?? new();
            userEmailServiceMock = userEmailServiceMock ?? new();
            emailQueueServiceMock = emailQueueServiceMock ?? new();
            notificationSettingsServiceMock = notificationSettingsServiceMock ?? new();
            userSmsServiceMock = userSmsServiceMock ?? new();
            userProfileDelegateMock = userProfileDelegateMock ?? new();
            legalAgreementServiceMock = legalAgreementServiceMock ?? new();
            messagingVerificationDelegateMock = messagingVerificationDelegateMock ?? new();
            userPreferenceServiceMock = userPreferenceServiceMock ?? new();
            configurationRoot = configurationRoot ?? GetIConfiguration();
            authenticationDelegateMock = authenticationDelegateMock ?? new();
            applicationSettingsServiceMock = applicationSettingsServiceMock ?? new();
            userProfileValidatorServiceMock = userProfileValidatorServiceMock ?? new();
            patientRepositoryMock = patientRepositoryMock ?? new();
            messageSenderMock = messageSenderMock ?? new();

            return new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                patientServiceMock.Object,
                userEmailServiceMock.Object,
                userSmsServiceMock.Object,
                emailQueueServiceMock.Object,
                notificationSettingsServiceMock.Object,
                userProfileDelegateMock.Object,
                userPreferenceServiceMock.Object,
                legalAgreementServiceMock.Object,
                messagingVerificationDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object,
                configurationRoot,
                MappingService,
                authenticationDelegateMock.Object,
                applicationSettingsServiceMock.Object,
                userProfileValidatorServiceMock.Object,
                patientRepositoryMock.Object,
                messageSenderMock.Object);
        }

        private sealed record CreateUserProfileMock(
            IUserProfileService Service,
            Mock<IUserEmailService> UserEmailServiceMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IMessageSender> MessageSenderMock,
            CreatedUserProfileResult Expected,
            CreateUserRequest CreateUserRequest,
            DateTime JwtAuthTime,
            string? JwtEmailAddress);

        private sealed record CreatedUserProfileResult(
            RequestResult<UserProfileModel> Result,
            Times TimesSendEmail,
            Times TimesSendAccountCreatedEvent,
            Times TimesSendNotificationChannelVerifiedEvent,
            Times TimesQueueNotificationSettings);

        private sealed record GetUserProfileMock(
            IUserProfileService Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            GetUserProfileResult Expected,
            string Hdid,
            DateTime JwtAuthTime);

        private sealed record GetUserProfileResult(
            RequestResult<UserProfileModel> Result,
            Times TimesUpdateUserProfile);

        private sealed record UpdateAcceptedTermsMock(
            IUserProfileService Service,
            RequestResult<UserProfileModel> Expected,
            string Hdid,
            Guid TermsOfServiceId);

        private sealed record CloseUserProfileMock(
            IUserProfileService Service,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            UserProfileModelResult Expected,
            string Hdid,
            Guid UserId);

        private sealed record RecoverUserProfileMock(
            IUserProfileService Service,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            UserProfileModelResult Expected,
            string Hdid);

        private sealed record UserProfileModelResult(
            RequestResult<UserProfileModel> Result,
            Times TimesSendEmail);
    }
}
