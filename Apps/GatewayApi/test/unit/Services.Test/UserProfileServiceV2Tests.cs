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
    using FluentValidation;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
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
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using UserProfileHistory = HealthGateway.Database.Models.UserProfileHistory;

    /// <summary>
    /// UserProfileServiceV2's Unit Tests.
    /// </summary>
    public class UserProfileServiceV2Tests
    {
        private const string AuthenticatedUserId = "d45acc23-ab01-4f7d-a5b9-1076a20f3a5a";
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";
        private const string InvalidSmsNumber = "xxx000xxxx";

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);
        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// CloseUserProfileAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CloseUserProfileAsync()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(profileUpdateStatus: DbStatusCode.Updated);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(mock.Hdid);

                // Verify
                Verify(mock.Verify);
            }
            else
            {
                Assert.Fail("Expected CloseUserProfileMock but got a different type.");
            }
        }

        /// <summary>
        /// CloseUserProfileAsync when user profile is already closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CloseUserProfileAlreadyClosed()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(closedDateTime: DateTime.UtcNow);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(mock.Hdid);

                // Verify
                Verify(mock.Verify);
            }
            else
            {
                Assert.Fail("Expected CloseUserProfileMock but got a different type.");
            }
        }

        /// <summary>
        /// CloseUserProfileAsync throws NotFoundException
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="profileUpdateStatus">The db status returned when user profile is updated in the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, null)] // NotFoundException
        [InlineData(true, DbStatusCode.Error)] // DatabaseException
        public async Task CloseUserProfileThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus)
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(userProfileExists, profileUpdateStatus: profileUpdateStatus);

            if (baseMock is CloseUserProfileThrowsExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    mock.Expected,
                    async () => { await mock.Service.CloseUserProfileAsync(mock.Hdid); });
            }
            else
            {
                Assert.Fail("Expected CloseUserProfileThrowsExceptionMock but got a different type.");
            }
        }

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
        public async Task CreateUserProfileAsync(
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            string? jwtEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            // Arrange
            CreateUserProfileMock mock = SetupCreateUserProfileMock(
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
            actual.ShouldDeepEqual(mock.Expected);
            Verify(mock.Verify);
        }

        /// <summary>
        /// CreateUserProfileAsync throws Exception.
        /// </summary>
        /// <param name="requestedSmsNumber">The value representing the requested sms number.</param>
        /// <param name="minPatientAge">The value representing the valid minimum age to create a profile.</param>
        /// <param name="patientAge">The value representing the patient's age.</param>
        /// <param name="profileInsertStatus">The db status returned when user profile is inserted in the database </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(InvalidSmsNumber, 18, 18, null)] // ValidationException
        [InlineData(SmsNumber, 18, 17, null)] // ValidationException
        [InlineData(SmsNumber, 18, 18, DbStatusCode.Error)] // DatabaseException
        public async Task CreateUserProfileAsyncThrowsException(
            string? requestedSmsNumber,
            int minPatientAge,
            int patientAge,
            DbStatusCode? profileInsertStatus)
        {
            // Arrange
            CreateUserProfileThrowsExceptionMock mock = SetupCreateUserProfileThrowsExceptionMock(
                requestedSmsNumber,
                minPatientAge,
                patientAge,
                profileInsertStatus);

            // Act and assert
            await Assert.ThrowsAsync(
                mock.Expected,
                async () =>
                {
                    await mock.Service.CreateUserProfileAsync(
                        mock.CreateProfileRequest,
                        mock.JwtAuthTime,
                        mock.JwtEmailAddress);
                });
        }

        /// <summary>
        /// GetUserProfileAsync.
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="jwtAuthTimeIsDifferent">The value indicating whether jwt auth time is different from last login or not.</param>
        /// <param name="emailAddressExists">The value indicating whether email address exists or not.</param>
        /// <param name="smsNumberExists">The value indicating whether sms number exists or not.</param>
        /// <param name="tourChangeDateIsLatest">The value indicating whether tour change date is latest or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, true, true, true)] // Happy path
        [InlineData(true, false, true, true, true)] // jwt auth time is not different
        [InlineData(true, true, true, true, false)] // Tour change is not latest
        [InlineData(true, false, false, false, true)] // Profile email and sms do not exist; look at messaging verification
        [InlineData(false, false, false, false, false)] // Cannot get profile because user profile does not exist
        public async Task GetUserProfileAsync(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent,
            bool emailAddressExists,
            bool smsNumberExists,
            bool tourChangeDateIsLatest)
        {
            // Arrange
            UserProfileMock mock = SetupUserProfileMock(
                userProfileExists,
                jwtAuthTimeIsDifferent,
                emailAddressExists,
                smsNumberExists,
                tourChangeDateIsLatest);

            // Act
            UserProfileModel actual = await mock.Service.GetUserProfileAsync(mock.Hdid, mock.JwtAuthTime);

            // Assert and Verify
            actual.ShouldDeepEqual(mock.Expected);
            Verify(mock.Verify);
        }

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("3345678901")]
        [InlineData("2507001000")]
        [Theory]
        public async Task PhoneNumberIsValidAsync(string phoneNumber)
        {
            // Arrange
            PhoneNumberValidMock mock = SetupPhoneNumberValidMock(phoneNumber, true);

            // Act
            bool actual = await mock.Service.IsPhoneNumberValidAsync(mock.PhoneNumber);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        /// <summary>
        /// IsPhoneNumberValidAsync.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData("xxx3277465")]
        [InlineData("abc")]
        [Theory]
        public async Task PhoneNumberIsNotValidAsync(string phoneNumber)
        {
            // Arrange
            PhoneNumberValidMock mock = SetupPhoneNumberValidMock(phoneNumber, false);

            // Act
            bool actual = await mock.Service.IsPhoneNumberValidAsync(mock.PhoneNumber);

            // Assert
            Assert.Equal(mock.Expected, actual);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RecoverUserProfile()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(
                profileClosedDateTime: DateTime.UtcNow,
                profileUpdateStatus: DbStatusCode.Updated);

            if (baseMock is RecoverUserProfileMock mock)
            {
                // Act
                await mock.Service.RecoverUserProfileAsync(mock.Hdid);

                // Verify
                Verify(mock.Verify);
            }
            else
            {
                Assert.Fail("Expected RecoverUserProfileMock but got a different type.");
            }
        }

        /// <summary>
        /// RecoverUserProfile already recovered.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RecoverUserProfileAlreadyRecovered()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(profileClosedDateTime: null);

            if (baseMock is RecoverUserProfileMock mock)
            {
                // Act
                await mock.Service.RecoverUserProfileAsync(mock.Hdid);

                // Verify
                Verify(mock.Verify);
            }
            else
            {
                Assert.Fail("Expected RecoverUserProfileMock but got a different type.");
            }
        }

        /// <summary>
        /// RecoverUserProfile already recovered.
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="profileUpdateStatus">The db status returned when user profile is updated in the database.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(false, null)] // NotFoundException
        [InlineData(true, DbStatusCode.Error)] // DatabaseException
        [Theory]
        public async Task RecoverUserProfileThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus)
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(
                userProfileExists,
                DateTime.UtcNow,
                profileUpdateStatus);

            if (baseMock is RecoverUserProfileThrowsExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    mock.Expected,
                    async () => { await mock.Service.RecoverUserProfileAsync(mock.Hdid); });
            }
            else
            {
                Assert.Fail("Expected RecoverUserProfileThrowsExceptionMock but got a different type.");
            }
        }

        /// <summary>
        /// UpdateAcceptedTermsAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAcceptedTermsAsync()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupUpdateAcceptedTermsMock(DbStatusCode.Updated);

            if (baseMock is UpdateAcceptedTermsMock mock)
            {
                // Act
                await mock.Service.UpdateAcceptedTermsAsync(mock.Hdid, mock.TermsOfServiceId);

                // Verify
                Verify(mock.Verify);
            }
            else
            {
                Assert.Fail("Expected UpdateAcceptedTermsMock but got a different type.");
            }
        }

        /// <summary>
        /// UpdateAcceptedTermsAsync throws DatabaseException
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateAcceptedTermsAsyncThrowsDatabaseException()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupUpdateAcceptedTermsMock(DbStatusCode.Error);

            if (baseMock is UpdateAcceptedTermsThrowsExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    mock.Expected,
                    async () => { await mock.Service.UpdateAcceptedTermsAsync(mock.Hdid, mock.TermsOfServiceId); });
            }
            else
            {
                Assert.Fail("Expected UpdateAcceptedTermsThrowsExceptionMock but got a different type.");
            }
        }

        /// <summary>
        /// ValidateEligibilityAsync.
        /// </summary>
        /// <param name="minPatientAge">The minimum patient age to validate against.</param>
        /// <param name="patientAge">The patient age to validate.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(0, 0)]
        [InlineData(19, 19)]
        [InlineData(19, 20)]
        [InlineData(19, 18)]
        [Theory]
        public async Task ValidateEligibilityAsync(int minPatientAge, int patientAge)
        {
            // Arrange
            ValidateEligibilityMock mock = SetupValidateEligibilityMock(minPatientAge, patientAge);

            // Act
            bool actual = await mock.Service.ValidateEligibilityAsync(mock.Hdid);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        private static void Verify(VerifyMock mock)
        {
            mock.MessagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => !string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email == null
                             && string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedSmsVerificationInsertTimes);

            mock.MessagingVerificationDelegateMock.Verify(
                v => v.InsertAsync(
                    It.Is<MessagingVerification>(
                        x => string.IsNullOrWhiteSpace(x.SmsNumber)
                             && x.Email != null
                             && !string.IsNullOrWhiteSpace(x.EmailAddress)),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedEmailVerificationInsertTimes);

            mock.UserProfileDelegateMock.Verify(
                v => v.UpdateAsync(
                    It.IsAny<UserProfile>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedUserProfileUpdateTimes);

            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedQueueNewEmailByTemplateTimes);

            mock.EmailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedQueueNewEmailByEntityTimes);

            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is AccountCreatedEvent),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedSendAccountCreatedEventTimes);

            mock.MessageSenderMock.Verify(
                v => v.SendAsync(
                    It.Is<IEnumerable<MessageEnvelope>>(
                        envelopes => envelopes.First().Content is NotificationChannelVerifiedEvent),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedSendNotificationChannelVerifiedEventTimes);

            mock.NotificationSettingsServiceMock.Verify(
                v => v.QueueNotificationSettingsAsync(
                    It.IsAny<NotificationSettingsRequest>(),
                    It.IsAny<CancellationToken>()),
                mock.ExpectedQueueNotificationSettingsTimes);
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

        private static UserProfileHistory GenerateUserProfileHistory(
            string hdid = Hdid,
            DateTime? loginDate = null,
            int daysFromLoginDate = 0)
        {
            DateTime lastLoginDateTime = loginDate?.Date ?? DateTime.UtcNow.Date;

            return new()
            {
                HdId = hdid,
                Id = Guid.NewGuid(),
                LastLoginDateTime = lastLoginDateTime.AddDays(-daysFromLoginDate),
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

        private static IConfigurationRoot GetIConfiguration(
            int minPatientAge = 12,
            int profileHistoryRecordLimit = 2,
            bool accountsChangeFeedEnabled = false,
            bool notificationsChangeFeedEnabled = false)
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "WebClient:MinPatientAge", minPatientAge.ToString(CultureInfo.InvariantCulture) },
                { "WebClient:UserProfileHistoryRecordLimit", profileHistoryRecordLimit.ToString(CultureInfo.InvariantCulture) },
                { "ChangeFeed:Accounts:Enabled", accountsChangeFeedEnabled.ToString() },
                { "ChangeFeed:Notifications:Enabled", notificationsChangeFeedEnabled.ToString() },
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }

        private static Mock<IApplicationSettingsService> SetupApplicationSettingsServiceMock(DateTime latestTourChangeDateTime)
        {
            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(
                    s => s.GetLatestTourChangeDateTimeAsync(
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourChangeDateTime);

            return applicationSettingsServiceMock;
        }

        private static Mock<IAuthenticationDelegate> SetupAuthenticationDelegateMock(
            UserLoginClientType userLoginClientType = UserLoginClientType.Web,
            string authenticatedUserId = AuthenticatedUserId)
        {
            Mock<IAuthenticationDelegate> authenticationDelegateMock = new();

            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserClientType())
                .Returns(userLoginClientType);

            authenticationDelegateMock.Setup(
                    s => s.FetchAuthenticatedUserId())
                .Returns(authenticatedUserId);

            return authenticationDelegateMock;
        }

        private static Mock<ILegalAgreementServiceV2> SetupLegalAgreementServiceMock(Guid latestTermsOfServiceId)
        {
            Mock<ILegalAgreementServiceV2> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(
                    s => s.GetActiveLegalAgreementId(
                        It.Is<LegalAgreementType>(x => x == LegalAgreementType.TermsOfService),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTermsOfServiceId);

            return legalAgreementServiceMock;
        }

        private static Mock<IMessagingVerificationDelegate> SetupMessagingVerificationDelegateMock(
            MessagingVerification emailAddressInvite,
            MessagingVerification smsNumberInvite)
        {
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();

            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.IsAny<string>(),
                        It.Is<string>(x => x == MessagingVerificationType.Email),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailAddressInvite);

            messagingVerificationDelegateMock.Setup(
                    s => s.GetLastForUserAsync(
                        It.IsAny<string>(),
                        It.Is<string>(x => x == MessagingVerificationType.Sms),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(smsNumberInvite);

            return messagingVerificationDelegateMock;
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

        private static Mock<IUserPreferenceServiceV2> SetupUserPreferenceServiceMock(
            Dictionary<string, UserPreferenceModel> userPreferences)
        {
            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.Is<string>(x => x == Hdid),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPreferences);

            return userPreferenceServiceMock;
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

        private static Mock<IUserPreferenceServiceV2> SetupUserPreferencesServiceMock()
        {
            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(
                    s => s.GetUserPreferencesAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            return userPreferenceServiceMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            UserProfile? userProfile,
            IList<UserProfileHistory>? userProfileHistoryList = null)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

            if (userProfile != null)
            {
                userProfileHistoryList ??=
                [
                    GenerateUserProfileHistory(loginDate: userProfile.LastLoginDateTime, daysFromLoginDate: 1),
                    GenerateUserProfileHistory(loginDate: userProfile.LastLoginDateTime, daysFromLoginDate: 2),
                ];

                userProfileDelegateMock.Setup(
                        s => s.GetUserProfileHistoryListAsync(
                            It.IsAny<string>(),
                            It.IsAny<int>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(userProfileHistoryList);
            }

            return userProfileDelegateMock;
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

        private static BaseUserProfileServiceMock SetupUpdateAcceptedTermsMock(DbStatusCode profileUpdateStatus)
        {
            UserProfile userProfile = GenerateUserProfile();
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile);

            DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(profileUpdateStatus, userProfile);
            SetupUserProfileDelegateMock(userProfileDelegateMock, updateProfileResult: updateProfileResult);

            IUserProfileServiceV2 service = GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock);

            if (profileUpdateStatus == DbStatusCode.Updated)
            {
                VerifyMock verifyMock = SetupVerifyMock(
                    userProfileDelegateMock: userProfileDelegateMock,
                    expectUserProfileUpdate: true);

                return new UpdateAcceptedTermsMock(service, Hdid, TermsOfServiceGuid, verifyMock);
            }

            return new UpdateAcceptedTermsThrowsExceptionMock(service, Hdid, TermsOfServiceGuid, typeof(DatabaseException));
        }

        private static BaseUserProfileServiceMock SetupCloseUserProfileMock(
            bool userProfileExists = true,
            DateTime? closedDateTime = null,
            DbStatusCode? profileUpdateStatus = null)
        {
            UserProfile? userProfile =
                userProfileExists ? GenerateUserProfile(closedDateTime: closedDateTime, email: EmailAddress) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile);

            if (profileUpdateStatus != null)
            {
                DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(profileUpdateStatus.Value, userProfile);
                SetupUserProfileDelegateMock(userProfileDelegateMock, updateProfileResult: updateProfileResult);
            }

            Mock<IAuthenticationDelegate> authenticationDelegateMock = SetupAuthenticationDelegateMock();
            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileServiceV2 service = GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock,
                authenticationDelegateMock: authenticationDelegateMock,
                emailQueueServiceMock: emailQueueServiceMock);

            if (closedDateTime != null || profileUpdateStatus == DbStatusCode.Updated)
            {
                VerifyMock verifyMock = SetupVerifyMock(
                    userProfileDelegateMock: userProfileDelegateMock,
                    emailQueueServiceMock: emailQueueServiceMock,
                    expectUserProfileUpdate: closedDateTime == null,
                    expectQueueNewEmailByTemplate: closedDateTime == null);

                return new CloseUserProfileMock(service, Hdid, verifyMock);
            }

            Type expectedType = userProfileExists ? typeof(DatabaseException) : typeof(NotFoundException);

            return new CloseUserProfileThrowsExceptionMock(service, Hdid, expectedType);
        }

        private static CreateUserProfileMock SetupCreateUserProfileMock(
            string? requestedSmsNumber,
            string? requestedEmailAddress,
            string? jwtEmailAddress,
            int minPatientAge,
            int patientAge,
            bool accountsChangeFeedEnabled,
            bool notificationsChangeFeedEnabled)
        {
            DateTime currentUtcDate = DateTime.UtcNow.Date;

            CreateUserRequest createUserRequest = new()
            {
                Profile = new(Hdid, Guid.NewGuid(), requestedSmsNumber, requestedEmailAddress),
            };

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(GenerateBirthDate(patientAge)));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            MessagingVerification emailVerification = GenerateMessagingVerification(emailAddress: requestedEmailAddress);
            Mock<IUserEmailServiceV2> userEmailServiceMock = SetupUserEmailServiceMock(emailVerification);

            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IUserSmsServiceV2> userSmsServiceMock = SetupUserSmsServiceMock(smsVerification);

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            UserProfile insertUserProfile = GenerateUserProfile(
                loginDate: currentUtcDate,
                email: requestedEmailAddress,
                smsNumber: requestedSmsNumber);
            DbResult<UserProfile> insertProfileResult = GenerateUserProfileDbResult(DbStatusCode.Created, insertUserProfile);
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = SetupUserPreferencesServiceMock();

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge,
                accountsChangeFeedEnabled: accountsChangeFeedEnabled,
                notificationsChangeFeedEnabled: notificationsChangeFeedEnabled);

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                userSmsServiceMock: userSmsServiceMock,
                userEmailServiceMock: userEmailServiceMock,
                emailQueueServiceMock: emailQueueServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                messageSenderMock: messageSenderMock,
                configurationRoot: configuration);

            UserProfileModel expected = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = requestedEmailAddress,
                IsEmailVerified = !string.IsNullOrWhiteSpace(requestedEmailAddress),
                AcceptedTermsOfService = true,
                SmsNumber = requestedSmsNumber,
                IsSmsNumberVerified = !string.IsNullOrWhiteSpace(requestedSmsNumber),
                HasTermsOfServiceUpdated = true,
                HasTourUpdated = false,
                LastLoginDateTime = currentUtcDate,
                LastLoginDateTimes = [currentUtcDate],
                BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
            };

            bool isEmailVerified =
                !string.IsNullOrWhiteSpace(requestedEmailAddress)
                && string.Equals(requestedEmailAddress, jwtEmailAddress, StringComparison.OrdinalIgnoreCase);

            VerifyMock verifyMock = SetupVerifyMock(
                messagingVerificationDelegateMock,
                userProfileDelegateMock,
                notificationSettingsServiceMock,
                emailQueueServiceMock,
                messageSenderMock,
                !string.IsNullOrWhiteSpace(requestedSmsNumber),
                !string.IsNullOrWhiteSpace(requestedEmailAddress),
                expectQueueNotificationSettings: true,
                expectQueueNewEmailByEntity: !isEmailVerified && !string.IsNullOrWhiteSpace(requestedEmailAddress),
                expectSendAccountCreated: accountsChangeFeedEnabled,
                expectSendNotificationChannelVerifiedEvent: isEmailVerified && notificationsChangeFeedEnabled);

            return new(service, createUserRequest, DateTime.Today, jwtEmailAddress, expected, verifyMock);
        }

        private static CreateUserProfileThrowsExceptionMock SetupCreateUserProfileThrowsExceptionMock(
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

            MessagingVerification smsVerification = GenerateMessagingVerification(smsNumber: requestedSmsNumber);
            Mock<IUserSmsServiceV2> userSmsServiceMock = SetupUserSmsServiceMock(smsVerification);

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            UserProfile? insertUserProfile = profileInsertStatus == DbStatusCode.Created ? GenerateUserProfile() : null;
            DbResult<UserProfile>? insertProfileResult =
                profileInsertStatus != null ? GenerateUserProfileDbResult(profileInsertStatus.Value, insertUserProfile) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(insertProfileResult: insertProfileResult);

            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = SetupUserPreferencesServiceMock();

            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();

            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge: minPatientAge);

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                userSmsServiceMock: userSmsServiceMock,
                userEmailServiceMock: userEmailServiceMock,
                emailQueueServiceMock: emailQueueServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                messageSenderMock: messageSenderMock,
                configurationRoot: configuration);

            Type expected = profileInsertStatus == DbStatusCode.Error ? typeof(DatabaseException) : typeof(ValidationException);

            return new(service, createUserRequest, DateTime.Today, EmailAddress, expected);
        }

        private static PhoneNumberValidMock SetupPhoneNumberValidMock(string phoneNumber, bool valid)
        {
            IUserProfileServiceV2 service = GetUserProfileService(configurationRoot: GetIConfiguration());
            return new(service, phoneNumber, valid);
        }

        private static BaseUserProfileServiceMock SetupRecoverUserProfileMock(
            bool userProfileExists = true,
            DateTime? profileClosedDateTime = null,
            DbStatusCode? profileUpdateStatus = null)
        {
            UserProfile? userProfile = userProfileExists
                ? GenerateUserProfile(closedDateTime: profileClosedDateTime, email: EmailAddress)
                : null;

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile: userProfile);

            if (profileUpdateStatus != null)
            {
                DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(profileUpdateStatus.Value, userProfile);
                SetupUserProfileDelegateMock(userProfileDelegateMock, updateProfileResult: updateProfileResult);
            }

            Mock<IEmailQueueService> emailQueueServiceMock = new();

            IUserProfileServiceV2 service = GetUserProfileService(
                emailQueueServiceMock: emailQueueServiceMock,
                userProfileDelegateMock: userProfileDelegateMock);

            VerifyMock verifyMock = SetupVerifyMock(
                userProfileDelegateMock: userProfileDelegateMock,
                emailQueueServiceMock: emailQueueServiceMock,
                expectUserProfileUpdate: profileUpdateStatus == DbStatusCode.Updated,
                expectQueueNewEmailByTemplate: profileUpdateStatus == DbStatusCode.Updated);

            if (userProfileExists)
            {
                if (profileUpdateStatus == DbStatusCode.Error)
                {
                    return new RecoverUserProfileThrowsExceptionMock(service, Hdid, typeof(DatabaseException));
                }

                return new RecoverUserProfileMock(service, Hdid, verifyMock);
            }

            return new RecoverUserProfileThrowsExceptionMock(service, Hdid, typeof(NotFoundException));
        }

        private static UserProfileMock SetupUserProfileMock(
            bool userProfileExists = true,
            bool jwtAuthTimeIsDifferent = true,
            bool profileEmailAddressExists = true,
            bool profileSmsNumberExists = true,
            bool tourChangeDateIsLatest = true)
        {
            string? smsNumber = profileSmsNumberExists ? SmsNumber : null;
            string? emailAddress = profileEmailAddressExists ? EmailAddress : null;

            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime currentUtcDate = DateTime.UtcNow.Date;
            DateTime birthDate = GenerateBirthDate();

            DateTime jwtAuthTime = jwtAuthTimeIsDifferent
                ? currentUtcDate.AddHours(1)
                : currentUtcDate;

            DateTime latestTourChangeDateTime = tourChangeDateIsLatest
                ? currentUtcDate
                : currentUtcDate.AddDays(-5);

            UserProfile? userProfile = userProfileExists
                ? GenerateUserProfile(loginDate: currentUtcDate, email: emailAddress, smsNumber: smsNumber)
                : null;

            IList<UserProfileHistory> userProfileHistoryList =
            [
                GenerateUserProfileHistory(loginDate: currentUtcDate, daysFromLoginDate: 1),
                GenerateUserProfileHistory(loginDate: currentUtcDate, daysFromLoginDate: 2),
            ];

            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(
                    userProfile,
                    userProfileHistoryList);

            Mock<IAuthenticationDelegate> authenticationDelegateMock = SetupAuthenticationDelegateMock();

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(birthDate));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = SetupUserPreferenceServiceMock([]);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock =
                SetupApplicationSettingsServiceMock(latestTourChangeDateTime);

            Mock<ILegalAgreementServiceV2> legalAgreementServiceMock =
                SetupLegalAgreementServiceMock(latestTermsOfServiceId);

            MessagingVerification emailAddressVerification = GenerateMessagingVerification(emailAddress: EmailAddress);
            MessagingVerification smsNumberVerification = GenerateMessagingVerification(smsNumber: SmsNumber);
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(emailAddressVerification, smsNumberVerification);

            IConfigurationRoot configuration = GetIConfiguration();

            Mock<IUserEmailServiceV2> userEmailServiceMock = new();
            Mock<INotificationSettingsService> notificationSettingsServiceMock = new();
            Mock<IMessageSender> messageSenderMock = new();

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                userEmailServiceMock,
                notificationSettingsServiceMock: notificationSettingsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                authenticationDelegateMock: authenticationDelegateMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock,
                messageSenderMock: messageSenderMock);

            UserProfileModel expected = userProfileExists
                ? new UserProfileModel
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    Email = EmailAddress,
                    AcceptedTermsOfService = true,
                    IsEmailVerified = emailAddress != null,
                    IsSmsNumberVerified = smsNumber != null,
                    SmsNumber = SmsNumber,
                    HasTermsOfServiceUpdated = true,
                    HasTourUpdated = tourChangeDateIsLatest,
                    LastLoginDateTime = jwtAuthTimeIsDifferent ? jwtAuthTime : currentUtcDate,
                    LastLoginDateTimes =
                    [
                        jwtAuthTimeIsDifferent ? jwtAuthTime : currentUtcDate,
                        userProfileHistoryList[0].LastLoginDateTime,
                        userProfileHistoryList[1].LastLoginDateTime,
                    ],
                    BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                }
                : new();

            VerifyMock verifyMock = SetupVerifyMock(
                userProfileDelegateMock: userProfileDelegateMock,
                expectUserProfileUpdate: jwtAuthTimeIsDifferent);

            return new(service, Hdid, jwtAuthTime, expected, verifyMock);
        }

        private static ValidateEligibilityMock SetupValidateEligibilityMock(
            int minPatientAge = 0,
            int patientAge = 0)
        {
            IConfigurationRoot configuration = GetIConfiguration(
                minPatientAge: minPatientAge);

            DateTime birthDate = GenerateBirthDate(patientAge);
            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(birthDate));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                configurationRoot: configuration);

            bool expected = minPatientAge == 0 || patientAge >= minPatientAge;

            return new(service, Hdid, expected);
        }

        private static VerifyMock SetupVerifyMock(
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            bool expectSmsVerificationInsert = false,
            bool expectEmailVerificationInsert = false,
            bool expectUserProfileUpdate = false,
            bool expectQueueNotificationSettings = false,
            bool expectQueueNewEmailByTemplate = false,
            bool expectQueueNewEmailByEntity = false,
            bool expectSendAccountCreated = false,
            bool expectSendNotificationChannelVerifiedEvent = false)
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
                Convert(expectSmsVerificationInsert), // ExpectedSmsVerificationInsertTime
                Convert(expectEmailVerificationInsert), // ExpectedEmailVerificationInsertTime
                Convert(expectUserProfileUpdate), // ExpectedUserProfileUpdateTimes
                Convert(expectQueueNotificationSettings), // ExpectedQueueNotificationSettingsTimes
                Convert(expectQueueNewEmailByTemplate), // ExpectedQueueNewEmailByTemplateTimes
                Convert(expectQueueNewEmailByEntity), // ExpectedQueueNewEmailByEntityTimes
                Convert(expectSendAccountCreated), // ExpectedSendAccountCreatedEventTimes
                Convert(expectSendNotificationChannelVerifiedEvent)); // ExpectedSendNotificationChannelVerifiedEventTimes

            static Times Convert(bool expect)
            {
                return expect ? Times.Once() : Times.Never();
            }
        }

        private static IUserProfileServiceV2 GetUserProfileService(
            Mock<IPatientDetailsService>? patientDetailsServiceMock = null,
            Mock<IUserEmailServiceV2>? userEmailServiceMock = null,
            Mock<IUserSmsServiceV2>? userSmsServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<INotificationSettingsService>? notificationSettingsServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IUserPreferenceServiceV2>? userPreferenceServiceMock = null,
            Mock<ILegalAgreementServiceV2>? legalAgreementServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IApplicationSettingsService>? applicationSettingsServiceMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IMessageSender>? messageSenderMock = null,
            IConfigurationRoot? configurationRoot = null)
        {
            patientDetailsServiceMock = patientDetailsServiceMock ?? new();
            userEmailServiceMock = userEmailServiceMock ?? new();
            userSmsServiceMock = userSmsServiceMock ?? new();
            emailQueueServiceMock = emailQueueServiceMock ?? new();
            notificationSettingsServiceMock = notificationSettingsServiceMock ?? new();
            userProfileDelegateMock = userProfileDelegateMock ?? new();
            userPreferenceServiceMock = userPreferenceServiceMock ?? new();
            legalAgreementServiceMock = legalAgreementServiceMock ?? new();
            messagingVerificationDelegateMock = messagingVerificationDelegateMock ?? new();
            authenticationDelegateMock = authenticationDelegateMock ?? new();
            applicationSettingsServiceMock = applicationSettingsServiceMock ?? new();
            patientRepositoryMock = patientRepositoryMock ?? new();
            messageSenderMock = messageSenderMock ?? new();
            configurationRoot = configurationRoot ?? GetIConfiguration();

            return new UserProfileServiceV2(
                new Mock<ILogger<UserProfileServiceV2>>().Object,
                patientDetailsServiceMock.Object,
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
                patientRepositoryMock.Object,
                messageSenderMock.Object);
        }

        private abstract record BaseUserProfileServiceMock;

        private sealed record UserProfileMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            DateTime JwtAuthTime,
            UserProfileModel Expected,
            VerifyMock Verify);

        private sealed record CreateUserProfileMock(
            IUserProfileServiceV2 Service,
            CreateUserRequest CreateProfileRequest,
            DateTime JwtAuthTime,
            string? JwtEmailAddress,
            UserProfileModel Expected,
            VerifyMock Verify);

        private sealed record CreateUserProfileThrowsExceptionMock(
            IUserProfileServiceV2 Service,
            CreateUserRequest CreateProfileRequest,
            DateTime JwtAuthTime,
            string? JwtEmailAddress,
            Type Expected);

        private sealed record UpdateAcceptedTermsMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            Guid TermsOfServiceId,
            VerifyMock Verify) : BaseUserProfileServiceMock;

        private sealed record UpdateAcceptedTermsThrowsExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            Guid TermsOfServiceId,
            Type Expected) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            VerifyMock Verify) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileThrowsExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            Type Expected) : BaseUserProfileServiceMock;

        private sealed record PhoneNumberValidMock(
            IUserProfileServiceV2 Service,
            string PhoneNumber,
            bool Expected);

        private sealed record RecoverUserProfileMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            VerifyMock Verify) : BaseUserProfileServiceMock;

        private sealed record ValidateEligibilityMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            bool Expected);

        private sealed record RecoverUserProfileThrowsExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            Type Expected) : BaseUserProfileServiceMock;

        private sealed record VerifyMock(
            Mock<IMessagingVerificationDelegate> MessagingVerificationDelegateMock,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<INotificationSettingsService> NotificationSettingsServiceMock,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            Mock<IMessageSender> MessageSenderMock,
            Times ExpectedSmsVerificationInsertTimes,
            Times ExpectedEmailVerificationInsertTimes,
            Times ExpectedUserProfileUpdateTimes,
            Times ExpectedQueueNotificationSettingsTimes,
            Times ExpectedQueueNewEmailByTemplateTimes,
            Times ExpectedQueueNewEmailByEntityTimes,
            Times ExpectedSendAccountCreatedEventTimes,
            Times ExpectedSendNotificationChannelVerifiedEventTimes);
    }
}
