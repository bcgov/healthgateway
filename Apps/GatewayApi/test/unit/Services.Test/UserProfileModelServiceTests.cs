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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Xunit;
    using UserProfileHistory = HealthGateway.Database.Models.UserProfileHistory;

    /// <summary>
    /// UserProfileModelService's Unit Tests.
    /// </summary>
    public class UserProfileModelServiceTests
    {
        private const string Hdid = "hdid-mock";
        private const string EmailAddress = "user@HealthGateway.ca";
        private const string SmsNumber = "2505556000";
        private const string SmsVerificationCode = "12345";

        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);
        private static readonly Guid TermsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// BuildUserProfileModelAsync.
        /// </summary>
        /// <param name="emailAddressExists">The value indicating whether email address exists or not.</param>
        /// <param name="smsNumberExists">The value indicating whether sms number exists or not.</param>
        /// <param name="tourChangeDateIsLatest">The value indicating whether tour change date is latest or not.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true, true, true)] // Email and sms exist and app tour changes are latest
        [InlineData(true, true, false)] // Email and sms exist and app tour changes are not latest
        [InlineData(false, false, true)] // Profile email and sms do not exist; look at messaging verification
        [InlineData(false, false, false)] // Profile email and sms do not exist and tour changes are not latest
        public async Task ShouldBuildUserProfileModel(
            bool emailAddressExists,
            bool smsNumberExists,
            bool tourChangeDateIsLatest)
        {
            // Arrange
            DateTime currentDateTime = DateTime.UtcNow.Date;
            IList<DataSource> blockedDataSources = [DataSource.Immunization];

            UserProfileHistory historyMinus1 = GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 1);
            UserProfileHistory historyMinus2 = GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 2);

            UserProfileModel expected = new()
            {
                HdId = Hdid,
                TermsOfServiceId = TermsOfServiceGuid,
                Email = EmailAddress,
                AcceptedTermsOfService = true,
                IsEmailVerified = emailAddressExists,
                IsSmsNumberVerified = smsNumberExists,
                SmsNumber = SmsNumber,
                HasTermsOfServiceUpdated = true,
                HasTourUpdated = tourChangeDateIsLatest,
                LastLoginDateTime = currentDateTime,
                LastLoginDateTimes =
                [
                    currentDateTime,
                    historyMinus1.LastLoginDateTime,
                    historyMinus2.LastLoginDateTime,
                ],
                BetaFeatures = [GatewayApi.Constants.BetaFeature.Salesforce],
                BlockedDataSources = blockedDataSources,
                NotificationSettings = [],
            };

            IUserProfileModelService service = SetupUserProfileModelServiceForBuildUserProfileModel(
                currentDateTime,
                blockedDataSources,
                tourChangeDateIsLatest);

            string? smsNumber = emailAddressExists ? SmsNumber : null;
            string? emailAddress = smsNumberExists ? EmailAddress : null;
            UserProfile userProfile = GenerateUserProfile(loginDate: currentDateTime, email: emailAddress, smsNumber: smsNumber, betaFeature: BetaFeature.Salesforce);

            // Act
            UserProfileModel actual = await service.BuildUserProfileModelAsync(userProfile, 2);

            // Assert
            actual.ShouldDeepEqual(expected);
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

        private static IUserProfileModelService GetUserProfileModelService(
            Mock<IApplicationSettingsService>? applicationSettingsServiceMock = null,
            Mock<ILegalAgreementServiceV2>? legalAgreementServiceMock = null,
            Mock<IMessagingVerificationDelegate>? messagingVerificationDelegateMock = null,
            Mock<IPatientRepository>? patientRepositoryMock = null,
            Mock<IUserPreferenceServiceV2>? userPreferenceServiceMock = null,
            Mock<IUserProfileDelegate>? userProfileDelegateMock = null,
            Mock<IUserProfileNotificationSettingService>? userProfileNotificationSettingServiceMock = null)
        {
            userProfileDelegateMock ??= new();
            userPreferenceServiceMock ??= new();
            legalAgreementServiceMock ??= new();
            messagingVerificationDelegateMock ??= new();
            applicationSettingsServiceMock ??= new();
            patientRepositoryMock ??= new();
            userProfileNotificationSettingServiceMock ??= new();

            return new UserProfileModelService(
                applicationSettingsServiceMock.Object,
                legalAgreementServiceMock.Object,
                MappingService,
                messagingVerificationDelegateMock.Object,
                patientRepositoryMock.Object,
                userPreferenceServiceMock.Object,
                userProfileDelegateMock.Object,
                userProfileNotificationSettingServiceMock.Object);
        }

        private static Mock<IApplicationSettingsService> SetupApplicationSettingsServiceMock(DateTime latestTourChangeDateTime)
        {
            Mock<IApplicationSettingsService> applicationSettingsServiceMock = new();
            applicationSettingsServiceMock.Setup(s => s.GetLatestTourChangeDateTimeAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(latestTourChangeDateTime);

            return applicationSettingsServiceMock;
        }

        private static Mock<ILegalAgreementServiceV2> SetupLegalAgreementServiceMock(Guid latestTermsOfServiceId)
        {
            Mock<ILegalAgreementServiceV2> legalAgreementServiceMock = new();
            legalAgreementServiceMock.Setup(s => s.GetActiveLegalAgreementId(
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

            messagingVerificationDelegateMock.Setup(s => s.GetLastForUserAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == MessagingVerificationType.Email),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emailAddressInvite);

            messagingVerificationDelegateMock.Setup(s => s.GetLastForUserAsync(
                    It.IsAny<string>(),
                    It.Is<string>(x => x == MessagingVerificationType.Sms),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(smsNumberInvite);

            return messagingVerificationDelegateMock;
        }

        private static Mock<IPatientRepository> SetupPatientRepositoryMock(IEnumerable<DataSource> dataSources)
        {
            Mock<IPatientRepository> patientRepositoryMock = new();
            patientRepositoryMock.Setup(s => s.GetDataSourcesAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSources);

            return patientRepositoryMock;
        }

        private static Mock<IUserPreferenceServiceV2> SetupUserPreferenceServiceMock(
            Dictionary<string, UserPreferenceModel> userPreferences)
        {
            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = new();
            userPreferenceServiceMock.Setup(s => s.GetUserPreferencesAsync(
                    It.Is<string>(x => x == Hdid),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userPreferences);

            return userPreferenceServiceMock;
        }

        private static Mock<IUserProfileDelegate> SetupUserProfileDelegateMock(
            IList<UserProfileHistory> userProfileHistoryList)
        {
            Mock<IUserProfileDelegate> userProfileDelegateMock = new();
            userProfileDelegateMock.Setup(s => s.GetUserProfileHistoryListAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(userProfileHistoryList);

            return userProfileDelegateMock;
        }

        private static Mock<IUserProfileNotificationSettingService> SetupUserProfileNotificationSettingServiceMock(IList<UserProfileNotificationSettingModel>? notificationSettingModels = null)
        {
            Mock<IUserProfileNotificationSettingService> notificationSettingServiceMock = new();

            notificationSettingServiceMock.Setup(s => s.GetAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificationSettingModels ?? []);

            return notificationSettingServiceMock;
        }

        private static IUserProfileModelService SetupUserProfileModelServiceForBuildUserProfileModel(
            DateTime currentDateTime,
            IEnumerable<DataSource> blockedDataSources,
            bool tourChangeDateIsLatest = true)
        {
            Guid latestTermsOfServiceId = Guid.NewGuid();

            DateTime latestTourChangeDateTime = tourChangeDateIsLatest
                ? currentDateTime
                : currentDateTime.AddDays(-5);

            IList<UserProfileHistory> userProfileHistoryList =
            [
                GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 1),
                GenerateUserProfileHistory(loginDate: currentDateTime, daysFromLoginDate: 2),
            ];

            Mock<IUserProfileDelegate> userProfileDelegateMock =
                SetupUserProfileDelegateMock(
                    userProfileHistoryList);

            Mock<IUserPreferenceServiceV2> userPreferenceServiceMock = SetupUserPreferenceServiceMock([]);

            Mock<IApplicationSettingsService> applicationSettingsServiceMock =
                SetupApplicationSettingsServiceMock(latestTourChangeDateTime);

            Mock<ILegalAgreementServiceV2> legalAgreementServiceMock =
                SetupLegalAgreementServiceMock(latestTermsOfServiceId);

            MessagingVerification emailAddressVerification = GenerateMessagingVerification(emailAddress: EmailAddress);
            MessagingVerification smsNumberVerification = GenerateMessagingVerification(smsNumber: SmsNumber);
            Mock<IMessagingVerificationDelegate> messagingVerificationDelegateMock =
                SetupMessagingVerificationDelegateMock(emailAddressVerification, smsNumberVerification);
            Mock<IPatientRepository> patientRepositoryMock = SetupPatientRepositoryMock(blockedDataSources);
            Mock<IUserProfileNotificationSettingService> userProfileNotificationSettingServiceMock = SetupUserProfileNotificationSettingServiceMock();

            return GetUserProfileModelService(
                applicationSettingsServiceMock,
                legalAgreementServiceMock,
                messagingVerificationDelegateMock,
                patientRepositoryMock,
                userPreferenceServiceMock,
                userProfileDelegateMock,
                userProfileNotificationSettingServiceMock);
        }
    }
}
