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
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);
        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// CloseUserProfileAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCloseUserProfileAsync()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(profileUpdateStatus: DbStatusCode.Updated);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(mock.Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
                VerifyQueueNewEmailByTemplate(mock.EmailQueueServiceMock);
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
        public async Task CloseUserProfileAsyncAlreadyClosed()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(closedDateTime: DateTime.UtcNow);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(mock.Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock, Times.Never());
                VerifyQueueNewEmailByTemplate(mock.EmailQueueServiceMock, Times.Never());
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
        /// <param name="expectedException">The expected exception type to be thrown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(false, null, typeof(NotFoundException))]
        [InlineData(true, DbStatusCode.Error, typeof(DatabaseException))]
        public async Task CloseUserProfileAsyncThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus, Type expectedException)
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(userProfileExists, profileUpdateStatus: profileUpdateStatus);

            if (baseMock is CloseUserProfileExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    expectedException,
                    async () => { await mock.Service.CloseUserProfileAsync(mock.Hdid); });
            }
            else
            {
                Assert.Fail("Expected CloseUserProfileThrowsExceptionMock but got a different type.");
            }
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
        public async Task ShouldGetUserProfileAsync(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent,
            bool emailAddressExists,
            bool smsNumberExists,
            bool tourChangeDateIsLatest)
        {
            // Arrange
            Times expectedUserProfileUpdate = ConvertToTimes(jwtAuthTimeIsDifferent);

            DateTime currentDateTime = DateTime.UtcNow.Date;
            DateTime jwtAuthTime = jwtAuthTimeIsDifferent
                ? currentDateTime.AddHours(1)
                : currentDateTime;

            UserProfileHistory historyMinus1 = GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 1);
            UserProfileHistory historyMinus2 = GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 2);

            UserProfileModel expected = userProfileExists
                ? new UserProfileModel
                {
                    HdId = Hdid,
                    TermsOfServiceId = TermsOfServiceGuid,
                    Email = EmailAddress,
                    AcceptedTermsOfService = true,
                    IsEmailVerified = userProfileExists && emailAddressExists,
                    IsSmsNumberVerified = userProfileExists && smsNumberExists,
                    SmsNumber = SmsNumber,
                    HasTermsOfServiceUpdated = true,
                    HasTourUpdated = tourChangeDateIsLatest,
                    LastLoginDateTime = jwtAuthTime,
                    LastLoginDateTimes =
                    [
                        jwtAuthTime,
                        historyMinus1.LastLoginDateTime,
                        historyMinus2.LastLoginDateTime,
                    ],
                    BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                }
                : new();

            UserProfileMock mock = SetupUserProfileMock(
                currentDateTime,
                jwtAuthTime,
                userProfileExists,
                emailAddressExists,
                smsNumberExists,
                tourChangeDateIsLatest);

            // Act
            UserProfileModel actual = await mock.Service.GetUserProfileAsync(mock.Hdid, mock.JwtAuthTime);

            // Assert and Verify
            actual.ShouldDeepEqual(expected);

            VerifyUserProfileUpdate(mock.UserProfileDelegateMock, expectedUserProfileUpdate);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRecoverUserProfileAsync()
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
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
                VerifyQueueNewEmailByTemplate(mock.EmailQueueServiceMock);
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
        public async Task RecoverUserProfileAsyncAlreadyRecovered()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(profileClosedDateTime: null);

            if (baseMock is RecoverUserProfileMock mock)
            {
                // Act
                await mock.Service.RecoverUserProfileAsync(mock.Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock, Times.Never());
                VerifyQueueNewEmailByTemplate(mock.EmailQueueServiceMock, Times.Never());
            }
            else
            {
                Assert.Fail("Expected RecoverUserProfileMock but got a different type.");
            }
        }

        /// <summary>
        /// RecoverUserProfileAsync already recovered.
        /// </summary>
        /// <param name="userProfileExists">The value indicating whether user profile exists or not.</param>
        /// <param name="profileUpdateStatus">The db status returned when user profile is updated in the database.</param>
        /// <param name="expected">The expected exception type to be thrown.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [InlineData(false, null, typeof(NotFoundException))]
        [InlineData(true, DbStatusCode.Error, typeof(DatabaseException))]
        [Theory]
        public async Task RecoverUserProfileAsyncThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus, Type expected)
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(
                userProfileExists,
                DateTime.UtcNow,
                profileUpdateStatus);

            if (baseMock is RecoverUserProfileExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    expected,
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
        public async Task ShouldUpdateAcceptedTermsAsync()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupUpdateAcceptedTermsMock(DbStatusCode.Updated);

            if (baseMock is UpdateAcceptedTermsMock mock)
            {
                // Act
                await mock.Service.UpdateAcceptedTermsAsync(mock.Hdid, mock.TermsOfServiceId);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
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

            if (baseMock is UpdateAcceptedTermsExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync<DatabaseException>(
                    async () => { await mock.Service.UpdateAcceptedTermsAsync(mock.Hdid, mock.TermsOfServiceId); });
            }
            else
            {
                Assert.Fail("Expected UpdateAcceptedTermsThrowsExceptionMock but got a different type.");
            }
        }

        private static void VerifyQueueNewEmailByTemplate(Mock<IEmailQueueService> emailQueueServiceMock, Times? times = null)
        {
            emailQueueServiceMock.Verify(
                v => v.QueueNewEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<bool>(),
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
                .AddInMemoryCollection([.. myConfiguration])
                .Build();
        }

        private static IUserProfileServiceV2 GetUserProfileService(
            Mock<IPatientDetailsService>? patientDetailsServiceMock = null,
            Mock<IEmailQueueService>? emailQueueServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IUserPreferenceServiceV2>? userPreferenceServiceMock = null,
            Mock<ILegalAgreementServiceV2>? legalAgreementServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IApplicationSettingsService>? applicationSettingsServiceMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            IConfigurationRoot? configurationRoot = null)
        {
            patientDetailsServiceMock ??= new();
            emailQueueServiceMock ??= new();
            userProfileDelegateMock ??= new();
            userPreferenceServiceMock ??= new();
            legalAgreementServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();
            authenticationDelegateMock ??= new();
            applicationSettingsServiceMock ??= new();
            patientRepositoryMock ??= new();
            configurationRoot ??= GetIConfiguration();

            return new UserProfileServiceV2(
                new Mock<ILogger<UserProfileServiceV2>>().Object,
                patientDetailsServiceMock.Object,
                emailQueueServiceMock.Object,
                userProfileDelegateMock.Object,
                userPreferenceServiceMock.Object,
                legalAgreementServiceMock.Object,
                messagingVerificationDelegateMock.Object,
                configurationRoot,
                MappingService,
                authenticationDelegateMock.Object,
                applicationSettingsServiceMock.Object,
                patientRepositoryMock.Object);
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

        private static void SetupUserProfileDelegateMock(
            Mock<IUserProfileDelegate> userProfileDelegateMock,
            DbResult<UserProfile>? insertProfileResult = null,
            DbResult<UserProfile>? updateProfileResult = null)
        {
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
        }

        private static BaseUserProfileServiceMock SetupUpdateAcceptedTermsMock(DbStatusCode profileUpdateStatus)
        {
            UserProfile userProfile = GenerateUserProfile();
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile: userProfile);

            DbResult<UserProfile> updateProfileResult = GenerateUserProfileDbResult(profileUpdateStatus, userProfile);
            SetupUserProfileDelegateMock(userProfileDelegateMock, updateProfileResult: updateProfileResult);

            IUserProfileServiceV2 service = GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock);

            if (profileUpdateStatus == DbStatusCode.Updated)
            {
                return new UpdateAcceptedTermsMock(
                    service,
                    userProfileDelegateMock,
                    Hdid,
                    TermsOfServiceGuid);
            }

            return new UpdateAcceptedTermsExceptionMock(service, Hdid, TermsOfServiceGuid);
        }

        private static BaseUserProfileServiceMock SetupCloseUserProfileMock(
            bool userProfileExists = true,
            DateTime? closedDateTime = null,
            DbStatusCode? profileUpdateStatus = null)
        {
            UserProfile? userProfile =
                userProfileExists ? GenerateUserProfile(closedDateTime: closedDateTime, email: EmailAddress) : null;
            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile: userProfile);

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
                return new CloseUserProfileMock(
                    service,
                    userProfileDelegateMock,
                    emailQueueServiceMock,
                    Hdid);
            }

            return new CloseUserProfileExceptionMock(service, Hdid);
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

            if (userProfileExists)
            {
                if (profileUpdateStatus == DbStatusCode.Error)
                {
                    return new RecoverUserProfileExceptionMock(service, Hdid);
                }

                return new RecoverUserProfileMock(
                    service,
                    userProfileDelegateMock,
                    emailQueueServiceMock,
                    Hdid);
            }

            return new RecoverUserProfileExceptionMock(service, Hdid);
        }

        private static UserProfileMock SetupUserProfileMock(
            DateTime currentDateTime,
            DateTime jwtAuthTime,
            bool userProfileExists = true,
            bool profileEmailAddressExists = true,
            bool profileSmsNumberExists = true,
            bool tourChangeDateIsLatest = true)
        {
            string? smsNumber = profileSmsNumberExists ? SmsNumber : null;
            string? emailAddress = profileEmailAddressExists ? EmailAddress : null;

            Guid latestTermsOfServiceId = Guid.NewGuid();
            DateTime birthDate = GenerateBirthDate();

            DateTime latestTourChangeDateTime = tourChangeDateIsLatest
                ? currentDateTime
                : currentDateTime.AddDays(-5);

            UserProfile? userProfile = userProfileExists
                ? GenerateUserProfile(loginDate: currentDateTime, email: emailAddress, smsNumber: smsNumber)
                : null;

            IList<UserProfileHistory> userProfileHistoryList =
            [
                GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 1),
                GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 2),
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

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                legalAgreementServiceMock: legalAgreementServiceMock,
                messagingVerificationDelegateMock: messagingVerificationDelegateMock,
                userPreferenceServiceMock: userPreferenceServiceMock,
                configurationRoot: configuration,
                authenticationDelegateMock: authenticationDelegateMock,
                applicationSettingsServiceMock: applicationSettingsServiceMock);

            return new(
                service,
                userProfileDelegateMock,
                Hdid,
                jwtAuthTime);
        }

        private abstract record BaseUserProfileServiceMock;

        private sealed record UserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            string Hdid,
            DateTime JwtAuthTime);

        private sealed record UpdateAcceptedTermsMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            string Hdid,
            Guid TermsOfServiceId) : BaseUserProfileServiceMock;

        private sealed record UpdateAcceptedTermsExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid,
            Guid TermsOfServiceId) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            string Hdid) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid) : BaseUserProfileServiceMock;

        private sealed record RecoverUserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<IEmailQueueService> EmailQueueServiceMock,
            string Hdid) : BaseUserProfileServiceMock;

        private sealed record RecoverUserProfileExceptionMock(
            IUserProfileServiceV2 Service,
            string Hdid) : BaseUserProfileServiceMock;
    }
}
