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
namespace HealthGateway.GatewayApiTests.Services.Test.Mock
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using PatientModel = HealthGateway.Common.Models.PatientModel;

    /// <summary>
    /// UserProfileServiceTestMock class mock the UserProfileService.
    /// </summary>
    public class UserProfileServiceMock : Mock<IUserProfileService>
    {
        private readonly IConfiguration configuration;
        private readonly string userProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";
        private readonly string webClientConfigSection = "WebClient";

        private readonly Mock<ILogger<UserProfileService>> loggerMock = new();
        private readonly Mock<IUserEmailService> userEmailServiceMock = new();
        private readonly Mock<IUserSmsService> userSmsServiceMock = new();
        private readonly Mock<IAuthenticationDelegate> authenticationDelegateMock = new();
        private readonly Mock<IApplicationSettingsDelegate> applicationSettingsDelegateMock = new();
        private readonly Mock<ICacheProvider> cacheProviderMock = new();
        private readonly IMapper autoMapper = MapperUtil.InitializeAutoMapper();
        private readonly Mock<INotificationSettingsService> notificationSettingsServiceMock = new NotificationSettingsServiceMock();

        private Mock<IPatientService> patientServiceMock = new();
        private Mock<IMessagingVerificationDelegate> messageVerificationDelegateMock = new MessagingVerificationDelegateMock();
        private Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
        private Mock<IEmailQueueService> emailQueueServiceMock = new();
        private Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        private Mock<IUserProfileDelegate> userProfileDelegateMock = new();
        private Mock<ICryptoDelegate> cryptoDelegateMock = new();
        private Mock<ILegalAgreementDelegate> legalAgreementDelegateMock = new LegalAgreementDelegateMock(
            new LegalAgreement
            {
                Id = Guid.Empty,
                CreatedBy = "MockData",
                CreatedDateTime = DateTime.UtcNow,
                EffectiveDate = DateTime.UtcNow,
                LegalAgreementCode = LegalAgreementType.TermsOfService,
                LegalText = "Mock Terms of Service",
            });

        private Mock<IPatientRepository> patientRepositoryMock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="configuration">The configuration that will be used by the UserProfileService.</param>
        public UserProfileServiceMock(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Generates the final UserProfileService Instance with mocked resources.
        /// This should be done only after all mocks are setup.
        /// </summary>
        /// <returns>UserProfileService.</returns>
        public UserProfileService UserProfileServiceMockInstance()
        {
            return new UserProfileService(
                this.loggerMock.Object,
                this.patientServiceMock.Object,
                this.userEmailServiceMock.Object,
                this.userSmsServiceMock.Object,
                this.emailQueueServiceMock.Object,
                this.notificationSettingsServiceMock.Object,
                this.userProfileDelegateMock.Object,
                this.userPreferenceDelegateMock.Object,
                this.legalAgreementDelegateMock.Object,
                this.messageVerificationDelegateMock.Object,
                this.cryptoDelegateMock.Object,
                this.httpContextAccessorMock.Object,
                this.configuration,
                this.autoMapper,
                this.authenticationDelegateMock.Object,
                this.applicationSettingsDelegateMock.Object,
                this.cacheProviderMock.Object,
                this.patientRepositoryMock.Object);
        }

        /// <summary>
        /// Setup a return value for the GetOrSet method for <see cref="ICacheProvider"/>.
        /// </summary>
        /// <param name="returnValue">Value to be returned by the GetOrSet method of <see cref="ICacheProvider"/>.</param>
        /// <param name="cacheKeyPattern">Cache pattern used to identify the desired mock result.</param>
        /// <typeparam name="T">Type of the value stored in cache.</typeparam>
        /// <exception cref="Exception">Will throw an exception if called after the UserProfileServiceMock is generated.</exception>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupGetOrSetCache<T>(T returnValue, string cacheKeyPattern)
        {
            this.cacheProviderMock.Setup(
                    cp => cp.GetOrSet(
                        It.Is<string>(key => key.Contains(cacheKeyPattern)),
                        It.IsAny<Func<T>>(),
                        It.IsAny<TimeSpan?>()))
                .Returns(returnValue);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="PatientRepositoryMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="hdid">User profile hdid to query and returned.</param>
        /// <param name="dataSources">The mocked list of <see cref="DataSource"/> to be returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupPatientRepository(string hdid, IEnumerable<DataSource> dataSources)
        {
            this.patientRepositoryMock = new PatientRepositoryMock(hdid, dataSources);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="UserProfileDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="hdid">User profile hdid to be returned.</param>
        /// <param name="userProfileData">The mocked user profile linked with the hdid.</param>
        /// <param name="userProfileDbResult">the mocked result from the database.</param>
        /// <param name="shouldProfileCommit">mock commit db.</param>
        /// <param name="userProfileDbResultUpdate">Optional update db result, fallback is userProfileDbResult.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupUserProfileDelegateMockGetAndUpdate(
            string hdid,
            UserProfile userProfileData,
            DbResult<UserProfile> userProfileDbResult,
            bool shouldProfileCommit = true,
            DbResult<UserProfile>? userProfileDbResultUpdate = null)
        {
            this.userProfileDelegateMock = new UserProfileDelegateMock(hdid, userProfileData, userProfileDbResult, shouldProfileCommit, userProfileDbResultUpdate);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="UserProfileDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="userProfileData">The mocked user profile linked with the hdid.</param>
        /// <param name="userProfileDbResult">the mocked result from the database.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupUserProfileDelegateMockInsert(UserProfile userProfileData, DbResult<UserProfile> userProfileDbResult)
        {
            this.userProfileDelegateMock = new UserProfileDelegateMock(userProfileData, userProfileDbResult);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="UserProfileDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="hdid">User profile hdid to be returned.</param>
        /// <param name="userProfileData">The mocked user profile linked with the hdid.</param>
        /// <param name="userProfileDbResult">The mocked result from teh database.</param>
        /// <param name="userProfileHistoryDbResult">Mocked user profile history.</param>
        /// <param name="limitRecords">Limit the number of records, if empty with extract from config, fallback is 2.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupUserProfileDelegateMockGetUpdateAndHistory(
            string hdid,
            UserProfile userProfileData,
            DbResult<UserProfile> userProfileDbResult,
            DbResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult,
            int? limitRecords = null)
        {
            int limit = limitRecords ?? this.configuration.GetSection(this.webClientConfigSection).GetValue(this.userProfileHistoryRecordLimitKey, 2);
            this.userProfileDelegateMock = new UserProfileDelegateMock(userProfileData, userProfileDbResult, hdid, userProfileHistoryDbResult, limit);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="CryptoDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="message">Mocked key returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupCryptoDelegateMockGenerateKey(string message)
        {
            this.cryptoDelegateMock = new CryptoDelegateMock(message);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="HttpContextAccessorMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="headerDictionary">Mocked headers for a request.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupHttpAccessorMockCustomHeaders(IHeaderDictionary headerDictionary)
        {
            this.httpContextAccessorMock = new HttpContextAccessorMock(headerDictionary);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="LegalAgreementDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="legalAgreement">The mocked legal agreement to be returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupLegalAgreementDelegateMock(LegalAgreement legalAgreement)
        {
            this.legalAgreementDelegateMock = new LegalAgreementDelegateMock(legalAgreement);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="EmailQueueServiceMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="shouldCommit">Does the code expect to commit.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupEmailQueueServiceMock(bool shouldCommit)
        {
            this.emailQueueServiceMock = new EmailQueueServiceMock(shouldCommit);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="UserPreferenceDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="hdid">HDID of the user which owns the preference.</param>
        /// <param name="readResult">Mock preferences to be returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupUserPreferenceDelegateMockReturnPreferences(string hdid, DbResult<IEnumerable<UserPreference>> readResult)
        {
            this.userPreferenceDelegateMock = new UserPreferenceDelegateMock(readResult, hdid);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="UserPreferenceDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="action">Action to be performed against user preference table.</param>
        /// <param name="readResult">Mock preference to be returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupUserPreferenceDelegateMockActions(string action, DbResult<UserPreference> readResult)
        {
            this.userPreferenceDelegateMock = new UserPreferenceDelegateMock(readResult, action);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="MessagingVerificationDelegateMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="messagingVerification">Mock MessageVerification.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupMessagingVerificationDelegateMockCustomMessage(MessagingVerification messagingVerification)
        {
            this.messageVerificationDelegateMock = new MessagingVerificationDelegateMock(messagingVerification);
            return this;
        }

        /// <summary>
        /// Setup the <see cref="PatientServiceMock"/> that will be used for by the UserProfileService.
        /// </summary>
        /// <param name="hdid">HDID of patient to return.</param>
        /// <param name="patient">Mock of patient to be returned.</param>
        /// <returns>UserProfileServiceMock.</returns>
        public UserProfileServiceMock SetupPatientServiceMockCustomPatient(string hdid, PatientModel patient)
        {
            this.patientServiceMock = new PatientServiceMock(hdid, patient);
            return this;
        }
    }
}
