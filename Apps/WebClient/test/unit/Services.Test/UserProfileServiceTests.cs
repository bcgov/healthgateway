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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Constants;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";
        private readonly Mock<IConfigurationService> emptyConfigServiceMock = new Mock<IConfigurationService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceTests"/> class.
        /// </summary>
        public UserProfileServiceTests()
        {
            var externalConfiguration = new ExternalConfiguration()
            {
                WebClient = new WebClientConfiguration()
                {
                    RegistrationStatus = RegistrationStatus.Open,
                },
            };
            this.emptyConfigServiceMock.Setup(s => s.GetConfiguration()).Returns(externalConfiguration);
        }

        /// <summary>
        /// GetUserProfile - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetUserProfile()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteGetUserProfile(DBStatusCode.Read, DateTime.Today);
            var actualResult = result.Item1;
            var expectedRecord = result.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(this.hdid, expectedRecord.HdId);
        }

        /// <summary>
        /// GetUserProfile - Happy Path With Terms of Service Updated.
        /// </summary>
        [Fact]
        public void ShouldGetUserProfileWithTermsOfServiceUpdated()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteGetUserProfile(DBStatusCode.Read, DateTime.Today);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload?.HasTermsOfServiceUpdated);
        }

        /// <summary>
        /// GetUserProfile - Database Error.
        /// </summary>
        [Fact]
        public void ShouldGetUserProfileWithDBError()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteGetUserProfile(DBStatusCode.Error, DateTime.Today);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// GetUserProfile - Not Found Error.
        /// </summary>
        [Fact]
        public void ShouldGetUserProfileWithProfileNotFoundError()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteGetUserProfile(DBStatusCode.NotFound, DateTime.Today);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.HdId);
        }

        /// <summary>
        /// InsertUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertUserProfile()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = await this.ExecuteInsertUserProfile(DBStatusCode.Created, RegistrationStatus.Open).ConfigureAwait(true);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
        }

        /// <summary>
        /// InsertUserProfile - Closed Registration Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertUserProfileWithClosedRegistration()
        {
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = await this.ExecuteInsertUserProfile(DBStatusCode.Error, RegistrationStatus.Closed).ConfigureAwait(true);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.InternalError(ErrorType.InvalidState), actualResult.ResultError?.ErrorCode);
            Assert.Equal("Registration is closed", actualResult.ResultError!.ResultMessage);
        }

        /// <summary>
        /// CreateUserProfile - Happy Path with notification update.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldQueueNotificationUpdate()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = "1234567890123456789012345678901234567890123456789012",
                AcceptedTermsOfService = true,
                Email = string.Empty,
            };

            DBResult<UserProfile> insertResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = DBStatusCode.Created,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.InsertUserProfile(It.Is<UserProfile>(x => x.HdId == userProfile.HdId))).Returns(insertResult);

            Mock<IMessagingVerificationDelegate> emailInviteDelegateMock = new Mock<IMessagingVerificationDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(new MessagingVerification());

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns("abc");

            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            notificationServiceMock.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            // Execute
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, DateTime.Today, It.IsAny<string>()).ConfigureAwait(true);

            // Verify
            notificationServiceMock.Verify(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()), Times.Once());
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expected.HdId, actualResult.ResourcePayload?.HdId);
            Assert.Equal(expected.Email, actualResult.ResourcePayload?.Email);
        }

        /// <summary>
        /// ValidateMinimumAge - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldValidateAgeAsync()
        {
            PatientModel patientModel = new PatientModel()
            {
                Birthdate = DateTime.Now.AddYears(-15),
            };
            Mock<IPatientService> patientServiceMock = new Mock<IPatientService>();
            patientServiceMock
                .Setup(s => s.GetPatient(this.hdid, PatientIdentifierType.HDID))
                .ReturnsAsync(new RequestResult<PatientModel>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = patientModel,
                });
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration() { WebClient = new WebClientConfiguration() { MinPatientAge = 19 } });

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                patientServiceMock.Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                configServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object);

            PrimitiveRequestResult<bool> expected = new PrimitiveRequestResult<bool>() { ResultStatus = ResultType.Success, ResourcePayload = false };
            PrimitiveRequestResult<bool> actualResult = await service.ValidateMinimumAge(this.hdid).ConfigureAwait(true);
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expected.ResourcePayload, actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldGetUserPreference()
        {
            Tuple<RequestResult<Dictionary<string, UserPreferenceModel>>, List<UserPreferenceModel>> result = this.ExecuteGetUserPreference(DBStatusCode.Read);
            var actualResult = result.Item1;
            var expectedRecord = result.Item2;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(actualResult.ResourcePayload?.Count, expectedRecord.Count);
            Assert.Equal(actualResult.ResourcePayload?["TutorialPopover"].Value, expectedRecord[0].Value);
        }

        /// <summary>
        /// GetUserPreference - Database Error.
        /// </summary>
        [Fact]
        public void ShouldGetUserPreferenceWithDBError()
        {
            Tuple<RequestResult<Dictionary<string, UserPreferenceModel>>, List<UserPreferenceModel>> result = this.ExecuteGetUserPreference(DBStatusCode.Error);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// CreateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldCreateUserPreference()
        {
            RequestResult<UserPreferenceModel> result = this.ExecuteCreateUserPreference(DBStatusCode.Created);

            Assert.Equal(ResultType.Success, result.ResultStatus);
        }

        /// <summary>
        /// CreateUserPreference - Database Error.
        /// </summary>
        [Fact]
        public void ShouldCreateUserPreferenceWithDBError()
        {
            RequestResult<UserPreferenceModel> actualResult = this.ExecuteCreateUserPreference(DBStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// UpdateUserPreference - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreference()
        {
            RequestResult<UserPreferenceModel> result = this.ExecuteUpdateUserPreference(DBStatusCode.Updated);

            Assert.Equal(ResultType.Success, result.ResultStatus);
        }

        /// <summary>
        /// UpdateUserPreference - Database Error.
        /// </summary>
        [Fact]
        public void ShouldUpdateUserPreferenceWithDBError()
        {
            RequestResult<UserPreferenceModel> actualResult = this.ExecuteUpdateUserPreference(DBStatusCode.Error);

            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldCloseUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
            };
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteCloseUserProfile(userProfile, DBStatusCode.Read);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path (Update Closed DateTime).
        /// </summary>
        [Fact]
        public void PreviouslyClosedUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                ClosedDateTime = DateTime.Today,
            };
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteCloseUserProfile(userProfile, DBStatusCode.Read);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(userProfile.ClosedDateTime, actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path with email notificaition.
        /// </summary>
        [Fact]
        public void ShouldCloseUserProfileAndQueueNewEmail()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                Email = "unit.test@hgw.ca",
            };
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteCloseUserProfile(userProfile, DBStatusCode.Read);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldRecoverUserProfile()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                ClosedDateTime = DateTime.Today,
                Email = "unit.test@hgw.ca",
            };
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteRecoverUserProfile(userProfile, DBStatusCode.Read);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// RecoverUserProfile - Alraedy active happy Path.
        /// </summary>
        [Fact]
        public void ShouldRecoverUserProfileAlreadyActive()
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                ClosedDateTime = null,
            };
            Tuple<RequestResult<UserProfileModel>, UserProfileModel> result = this.ExecuteRecoverUserProfile(userProfile, DBStatusCode.Read);
            var actualResult = result.Item1;

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        private async Task<Tuple<RequestResult<UserProfileModel>, UserProfileModel>> ExecuteInsertUserProfile(
            DBStatusCode dbResultStatus,
            string registrationStatus)
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                Email = "unit.test@hgw.ca",
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.InsertUserProfile(It.Is<UserProfile>(x => x.HdId == userProfile.HdId))).Returns(new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = dbResultStatus,
            });

            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            configServiceMock.Setup(s => s.GetConfiguration()).Returns(new ExternalConfiguration() { WebClient = new WebClientConfiguration() { RegistrationStatus = registrationStatus } });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.GenerateKey()).Returns("abc");

            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            notificationServiceMock.Setup(s => s.QueueNotificationSettings(It.IsAny<NotificationSettingsRequest>()));

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                configServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, DateTime.Today, It.IsAny<string>()).ConfigureAwait(true);

            return new Tuple<RequestResult<UserProfileModel>, UserProfileModel>(actualResult, expected);
        }

        private Tuple<RequestResult<UserProfileModel>, UserProfileModel> ExecuteRecoverUserProfile(
            UserProfile userProfile,
            DBStatusCode dbResultStatus = DBStatusCode.Read)
        {
            DBResult<UserProfile> userProfileDBResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = dbResultStatus,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(userProfileDBResult);
            profileDelegateMock.Setup(s => s.Update(userProfile, true)).Returns(userProfileDBResult);

            Mock<IEmailQueueService> emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock
                .Setup(s => s.QueueNewEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), false));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("referer", "http://localhost/");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                new Mock<IConfigurationService>().Object,
                emailQueueServiceMock.Object,
                new Mock<INotificationSettingsService>().Object,
                profileDelegateMock.Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                httpContextAccessorMock.Object);

            RequestResult<UserProfileModel> actualResult = service.RecoverUserProfile(this.hdid);

            return new Tuple<RequestResult<UserProfileModel>, UserProfileModel>(actualResult, expected);
        }

        private RequestResult<UserPreferenceModel> ExecuteCreateUserPreference(DBStatusCode dbResultStatus = DBStatusCode.Created)
        {
            UserPreferenceModel userPreferenceModel = new UserPreferenceModel
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            DBResult<UserPreference> readResult = new DBResult<UserPreference>
            {
                Payload = userPreferenceModel.ToDbModel(),
                Status = dbResultStatus,
            };
            Mock<IUserPreferenceDelegate> preferenceDelegateMock = new Mock<IUserPreferenceDelegate>();

            preferenceDelegateMock.Setup(s => s.CreateUserPreference(It.IsAny<UserPreference>(), It.IsAny<bool>())).Returns(readResult);

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();
            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            Mock<IMessagingVerificationDelegate> messageVerificationDelegateMock = new Mock<IMessagingVerificationDelegate>();

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                configServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                preferenceDelegateMock.Object,
                new Mock<ILegalAgreementDelegate>().Object,
                messageVerificationDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            return service.CreateUserPreference(userPreferenceModel);
        }

        private Tuple<RequestResult<Dictionary<string, UserPreferenceModel>>, List<UserPreferenceModel>> ExecuteGetUserPreference(
    DBStatusCode dbResultStatus = DBStatusCode.Read)
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
            };

            DBResult<UserProfile> userProfileDBResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = dbResultStatus,
            };

            LegalAgreement termsOfService = new LegalAgreement()
            {
                Id = Guid.NewGuid(),
                LegalText = string.Empty,
                EffectiveDate = DateTime.Now,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(userProfileDBResult);
            profileDelegateMock.Setup(s => s.Update(userProfile, true)).Returns(userProfileDBResult);

            UserPreferenceModel userPreferenceModel = new UserPreferenceModel
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = true.ToString(),
            };

            List<UserPreferenceModel> userPreferences = new List<UserPreferenceModel>();
            userPreferences.Add(userPreferenceModel);

            List<UserPreference> dbUserPreferences = new List<UserPreference>();
            dbUserPreferences.Add(userPreferenceModel.ToDbModel());

            DBResult<IEnumerable<UserPreference>> readResult = new DBResult<IEnumerable<UserPreference>>
            {
                Payload = dbUserPreferences,
                Status = dbResultStatus,
            };
            Mock<IUserPreferenceDelegate> preferenceDelegateMock = new Mock<IUserPreferenceDelegate>();
            preferenceDelegateMock.Setup(s => s.GetUserPreferences(this.hdid)).Returns(readResult);

            Mock<IMessagingVerificationDelegate> emailInviteDelegateMock = new Mock<IMessagingVerificationDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(new MessagingVerification());

            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new Mock<ILegalAgreementDelegate>();
            legalAgreementDelegateMock
                .Setup(s => s.GetActiveByAgreementType(LegalAgreementType.TermsofService))
                .Returns(new DBResult<LegalAgreement>() { Payload = termsOfService });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            Mock<IMessagingVerificationDelegate> messageVerificationDelegateMock = new Mock<IMessagingVerificationDelegate>();

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                preferenceDelegateMock.Object,
                new Mock<ILegalAgreementDelegate>().Object,
                messageVerificationDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            RequestResult<Dictionary<string, UserPreferenceModel>> actualResult = service.GetUserPreferences(this.hdid);

            return new Tuple<RequestResult<Dictionary<string, UserPreferenceModel>>, List<UserPreferenceModel>>(actualResult, userPreferences);
        }

        private RequestResult<UserPreferenceModel> ExecuteUpdateUserPreference(DBStatusCode dbResultStatus = DBStatusCode.Updated)
        {
            UserPreferenceModel userPreferenceModel = new UserPreferenceModel
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            DBResult<UserPreference> readResult = new DBResult<UserPreference>
            {
                Payload = userPreferenceModel.ToDbModel(),
                Status = dbResultStatus,
            };
            Mock<IUserPreferenceDelegate> preferenceDelegateMock = new Mock<IUserPreferenceDelegate>();

            preferenceDelegateMock.Setup(s => s.UpdateUserPreference(It.IsAny<UserPreference>(), It.IsAny<bool>())).Returns(readResult);

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            Mock<IConfigurationService> configServiceMock = new Mock<IConfigurationService>();

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            Mock<IMessagingVerificationDelegate> messageVerificationDelegateMock = new Mock<IMessagingVerificationDelegate>();

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                configServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                preferenceDelegateMock.Object,
                new Mock<ILegalAgreementDelegate>().Object,
                messageVerificationDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            return service.UpdateUserPreference(userPreferenceModel);
        }

        private Tuple<RequestResult<UserProfileModel>, UserProfileModel> ExecuteCloseUserProfile(
            UserProfile userProfile,
            DBStatusCode dbResultStatus = DBStatusCode.Read)
        {
            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            DBResult<UserProfile> userProfileDBResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = dbResultStatus,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(userProfileDBResult);
            profileDelegateMock.Setup(s => s.Update(userProfile, true)).Returns(userProfileDBResult);

            Mock<IEmailQueueService> emailQueueServiceMock = new Mock<IEmailQueueService>();
            emailQueueServiceMock
                .Setup(s => s.QueueNewEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), false));

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("referer", "http://localhost/");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);
            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                new Mock<IConfigurationService>().Object,
                emailQueueServiceMock.Object,
                new Mock<INotificationSettingsService>().Object,
                profileDelegateMock.Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                httpContextAccessorMock.Object);

            RequestResult<UserProfileModel> actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

            return new Tuple<RequestResult<UserProfileModel>, UserProfileModel>(actualResult, expected);
        }

        private Tuple<RequestResult<UserProfileModel>, UserProfileModel> ExecuteGetUserProfile(DBStatusCode dbResultStatus, DateTime lastLoginDateTime)
        {
            UserProfile userProfile = new UserProfile
            {
                HdId = this.hdid,
                AcceptedTermsOfService = true,
                LastLoginDateTime = lastLoginDateTime,
            };

            DBResult<UserProfile> userProfileDBResult = new DBResult<UserProfile>
            {
                Payload = userProfile,
                Status = dbResultStatus,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);
            expected.HasTermsOfServiceUpdated = true;

            LegalAgreement termsOfService = new LegalAgreement()
            {
                Id = Guid.NewGuid(),
                LegalText = string.Empty,
                EffectiveDate = DateTime.Now,
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(this.hdid)).Returns(userProfileDBResult);
            profileDelegateMock.Setup(s => s.Update(userProfile, true)).Returns(userProfileDBResult);

            UserPreference dbUserPreference = new UserPreference
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = true.ToString(),
            };
            List<UserPreference> userPreferences = new List<UserPreference>();
            userPreferences.Add(dbUserPreference);
            DBResult<IEnumerable<UserPreference>> readResult = new DBResult<IEnumerable<UserPreference>>
            {
                Payload = userPreferences,
                Status = DBStatusCode.Read,
            };
            Mock<IUserPreferenceDelegate> preferenceDelegateMock = new Mock<IUserPreferenceDelegate>();
            preferenceDelegateMock.Setup(s => s.GetUserPreferences(this.hdid)).Returns(readResult);

            Mock<IMessagingVerificationDelegate> emailInviteDelegateMock = new Mock<IMessagingVerificationDelegate>();
            emailInviteDelegateMock.Setup(s => s.GetLastByInviteKey(It.IsAny<Guid>())).Returns(new MessagingVerification());

            Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new Mock<ILegalAgreementDelegate>();
            legalAgreementDelegateMock
                .Setup(s => s.GetActiveByAgreementType(LegalAgreementType.TermsofService))
                .Returns(new DBResult<LegalAgreement>() { Payload = termsOfService });

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            Mock<INotificationSettingsService> notificationServiceMock = new Mock<INotificationSettingsService>();
            Mock<IMessagingVerificationDelegate> messageVerificationDelegateMock = new Mock<IMessagingVerificationDelegate>();

            IUserProfileService service = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                notificationServiceMock.Object,
                profileDelegateMock.Object,
                preferenceDelegateMock.Object,
                legalAgreementDelegateMock.Object,
                messageVerificationDelegateMock.Object,
                cryptoDelegateMock.Object,
                new Mock<IHttpContextAccessor>().Object);

            RequestResult<UserProfileModel> actualResult = service.GetUserProfile(this.hdid, DateTime.Now);

            return new Tuple<RequestResult<UserProfileModel>, UserProfileModel>(actualResult, expected);
        }
    }
}
