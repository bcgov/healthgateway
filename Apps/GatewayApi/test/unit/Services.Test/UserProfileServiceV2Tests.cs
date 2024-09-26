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
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.ErrorHandling.Exceptions;
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
    /// UserProfileServiceV2's Unit Tests.
    /// </summary>
    public class UserProfileServiceV2Tests
    {
        private const string AuthenticatedUserId = "d45acc23-ab01-4f7d-a5b9-1076a20f3a5a";
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";

        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// CloseUserProfileAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCloseUserProfile()
        {
            // Arrange
            const string expectedEmailTemplateName = EmailTemplateName.AccountClosedTemplate;
            const bool expectedCommit = true;

            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(profileUpdateStatus: DbStatusCode.Updated);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
                VerifySendEmail(mock.JobServiceMock, expectedEmailTemplateName, expectedCommit);
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
            const string expectedEmailTemplateName = EmailTemplateName.AccountClosedTemplate;
            const bool expectedCommit = true;

            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(closedDateTime: DateTime.UtcNow);

            if (baseMock is CloseUserProfileMock mock)
            {
                // Act
                await mock.Service.CloseUserProfileAsync(Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock, Times.Never());
                VerifySendEmail(mock.JobServiceMock, expectedEmailTemplateName, expectedCommit, Times.Never());
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
        public async Task CloseUserProfileThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus, Type expectedException)
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupCloseUserProfileMock(userProfileExists, profileUpdateStatus: profileUpdateStatus);

            if (baseMock is CloseUserProfileExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync(
                    expectedException,
                    async () => { await mock.Service.CloseUserProfileAsync(Hdid); });
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true)] // User profile exists and jwt auth time is different
        [InlineData(true, false)] // User profile exists and jwt auth time is not different
        [InlineData(false, false)] // Cannot get profile because user profile does not exist
        public async Task ShouldGetUserProfile(
            bool userProfileExists,
            bool jwtAuthTimeIsDifferent)
        {
            // Arrange
            Times expectedUserProfileUpdate = ConvertToTimes(jwtAuthTimeIsDifferent);

            DateTime currentDateTime = DateTime.UtcNow.Date;
            DateTime jwtAuthTime = jwtAuthTimeIsDifferent
                ? currentDateTime.AddHours(1)
                : currentDateTime;

            UserProfileModel expected = userProfileExists
                ? GenerateUserProfileModel(currentDateTime)
                : new();

            UserProfileMock mock = SetupUserProfileMock(
                currentDateTime,
                userProfileExists);

            // Act
            UserProfileModel actual = await mock.Service.GetUserProfileAsync(Hdid, jwtAuthTime);

            // Assert and Verify
            actual.ShouldDeepEqual(expected);

            VerifyUserProfileUpdate(mock.UserProfileDelegateMock, expectedUserProfileUpdate);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRecoverUserProfile()
        {
            // Arrange
            const string expectedEmailTemplateName = EmailTemplateName.AccountRecoveredTemplate;
            const bool expectedCommit = true;

            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(
                profileClosedDateTime: DateTime.UtcNow,
                profileUpdateStatus: DbStatusCode.Updated);

            if (baseMock is RecoverUserProfileMock mock)
            {
                // Act
                await mock.Service.RecoverUserProfileAsync(Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock);
                VerifySendEmail(mock.JobServiceMock, expectedEmailTemplateName, expectedCommit);
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
            const string expectedEmailTemplateName = EmailTemplateName.AccountRecoveredTemplate;
            const bool expectedCommit = true;

            BaseUserProfileServiceMock baseMock = SetupRecoverUserProfileMock(profileClosedDateTime: null);

            if (baseMock is RecoverUserProfileMock mock)
            {
                // Act
                await mock.Service.RecoverUserProfileAsync(Hdid);

                // Verify
                VerifyUserProfileUpdate(mock.UserProfileDelegateMock, Times.Never());
                VerifySendEmail(mock.JobServiceMock, expectedEmailTemplateName, expectedCommit, Times.Never());
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
        public async Task RecoverUserProfileThrowsException(bool userProfileExists, DbStatusCode? profileUpdateStatus, Type expected)
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
                    async () => { await mock.Service.RecoverUserProfileAsync(Hdid); });
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
        public async Task ShouldUpdateAcceptedTerms()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupUpdateAcceptedTermsMock(DbStatusCode.Updated);

            if (baseMock is UpdateAcceptedTermsMock mock)
            {
                // Act
                await mock.Service.UpdateAcceptedTermsAsync(Hdid, TermsOfServiceGuid);

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
        public async Task UpdateAcceptedTermsThrowsDatabaseException()
        {
            // Arrange
            BaseUserProfileServiceMock baseMock = SetupUpdateAcceptedTermsMock(DbStatusCode.Error);

            if (baseMock is UpdateAcceptedTermsExceptionMock mock)
            {
                // Act and Assert
                await Assert.ThrowsAsync<DatabaseException>(
                    async () => { await mock.Service.UpdateAcceptedTermsAsync(Hdid, TermsOfServiceGuid); });
            }
            else
            {
                Assert.Fail("Expected UpdateAcceptedTermsThrowsExceptionMock but got a different type.");
            }
        }

        private static void VerifySendEmail(Mock<IJobService> jobServiceMock, string emailTemplate, bool shouldCommit, Times? times = null)
        {
            jobServiceMock.Verify(
                v => v.SendEmailAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == emailTemplate),
                    It.Is<bool>(x => x == shouldCommit),
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

        private static UserProfileModel GenerateUserProfileModel(DateTime lastLoginDateTime)
        {
            return new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = EmailAddress,
                AcceptedTermsOfService = true,
                IsEmailVerified = true,
                IsSmsNumberVerified = true,
                SmsNumber = SmsNumber,
                HasTermsOfServiceUpdated = true,
                HasTourUpdated = true,
                LastLoginDateTime = lastLoginDateTime,
                LastLoginDateTimes =
                [
                    lastLoginDateTime,
                ],
                BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                BlockedDataSources = [DataSource.Immunization],
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
            Mock<IJobService>? jobServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IAuthenticationDelegate>? authenticationDelegateMock = null,
            Mock<IUserProfileModelService>? userProfileModelServiceMock = null,
            IConfigurationRoot? configurationRoot = null)
        {
            patientDetailsServiceMock ??= new();
            jobServiceMock ??= new();
            userProfileDelegateMock ??= new();
            authenticationDelegateMock ??= new();
            userProfileModelServiceMock ??= new();
            configurationRoot ??= GetIConfiguration();

            return new UserProfileServiceV2(
                new Mock<ILogger<UserProfileServiceV2>>().Object,
                patientDetailsServiceMock.Object,
                userProfileDelegateMock.Object,
                configurationRoot,
                authenticationDelegateMock.Object,
                userProfileModelServiceMock.Object,
                jobServiceMock.Object);
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
            UserProfile? userProfile)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(
                    s => s.GetUserProfileAsync(
                        It.IsAny<string>(),
                        It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfile);

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

        private static Mock<IUserProfileModelService> SetupUserProfileModelServiceMock(UserProfileModel userProfileModel, int profileHistoryRecordLimit)
        {
            Mock<IUserProfileModelService> userProfileModelServiceMock = new();
            userProfileModelServiceMock.Setup(
                    s => s.BuildUserProfileModelAsync(
                        It.IsAny<UserProfile>(),
                        It.Is<int>(x => x == profileHistoryRecordLimit),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileModel);

            return userProfileModelServiceMock;
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
                    userProfileDelegateMock);
            }

            return new UpdateAcceptedTermsExceptionMock(service);
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
            Mock<IJobService> jobServiceMock = new();

            IUserProfileServiceV2 service = GetUserProfileService(
                userProfileDelegateMock: userProfileDelegateMock,
                authenticationDelegateMock: authenticationDelegateMock,
                jobServiceMock: jobServiceMock);

            if (closedDateTime != null || profileUpdateStatus == DbStatusCode.Updated)
            {
                return new CloseUserProfileMock(
                    service,
                    userProfileDelegateMock,
                    jobServiceMock);
            }

            return new CloseUserProfileExceptionMock(service);
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

            Mock<IJobService> jobServiceMock = new();

            IUserProfileServiceV2 service = GetUserProfileService(
                jobServiceMock: jobServiceMock,
                userProfileDelegateMock: userProfileDelegateMock);

            if (userProfileExists)
            {
                if (profileUpdateStatus == DbStatusCode.Error)
                {
                    return new RecoverUserProfileExceptionMock(service);
                }

                return new RecoverUserProfileMock(
                    service,
                    userProfileDelegateMock,
                    jobServiceMock);
            }

            return new RecoverUserProfileExceptionMock(service);
        }

        private static UserProfileMock SetupUserProfileMock(
            DateTime currentDateTime,
            bool userProfileExists = true)
        {
            DateTime birthDate = GenerateBirthDate();
            const int profileHistoryRecordLimit = 2;

            UserProfile? userProfile = userProfileExists
                ? GenerateUserProfile(loginDate: currentDateTime, email: EmailAddress, smsNumber: SmsNumber)
                : null;

            Mock<IUserProfileDelegate> userProfileDelegateMock = SetupUserProfileDelegateMock(userProfile);
            Mock<IAuthenticationDelegate> authenticationDelegateMock = SetupAuthenticationDelegateMock();

            PatientDetails patientDetails = GeneratePatientDetails(birthDate: DateOnly.FromDateTime(birthDate));
            Mock<IPatientDetailsService> patientDetailsServiceMock = SetupPatientDetailsServiceMock(patientDetails);

            UserProfileModel userProfileModel = userProfileExists ? GenerateUserProfileModel(currentDateTime) : new();
            Mock<IUserProfileModelService> userProfileModelServiceMock = SetupUserProfileModelServiceMock(userProfileModel, profileHistoryRecordLimit);

            IUserProfileServiceV2 service = GetUserProfileService(
                patientDetailsServiceMock,
                userProfileDelegateMock: userProfileDelegateMock,
                authenticationDelegateMock: authenticationDelegateMock,
                userProfileModelServiceMock: userProfileModelServiceMock,
                configurationRoot: GetIConfiguration(profileHistoryRecordLimit: profileHistoryRecordLimit));

            return new(
                service,
                userProfileDelegateMock);
        }

        private abstract record BaseUserProfileServiceMock;

        private sealed record UserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock);

        private sealed record UpdateAcceptedTermsMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock) : BaseUserProfileServiceMock;

        private sealed record UpdateAcceptedTermsExceptionMock(
            IUserProfileServiceV2 Service) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<IJobService> JobServiceMock) : BaseUserProfileServiceMock;

        private sealed record CloseUserProfileExceptionMock(
            IUserProfileServiceV2 Service) : BaseUserProfileServiceMock;

        private sealed record RecoverUserProfileMock(
            IUserProfileServiceV2 Service,
            Mock<IUserProfileDelegate> UserProfileDelegateMock,
            Mock<IJobService> JobServiceMock) : BaseUserProfileServiceMock;

        private sealed record RecoverUserProfileExceptionMock(
            IUserProfileServiceV2 Service) : BaseUserProfileServiceMock;
    }
}
