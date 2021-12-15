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
namespace HealthGateway.WebClientTests.Services.Test.Mock
{
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;

    /// <summary>
    /// UserProfileServiceTestMock class mock the UserProfileService.
    /// </summary>
    public class UserProfileServiceMock : Mock<IUserProfileService>
    {
        private readonly UserProfileService userProfileService;
        private readonly Mock<IConfigurationService> emptyConfigServiceMock = new();
        private readonly string webClientConfigSection = "WebClient";
        private readonly string userProfileHistoryRecordLimitKey = "UserProfileHistoryRecordLimit";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="hdId">hdId.</param>
        /// <param name="dbResultStatus">db result status.</param>
        /// <param name="userProfileData">user profile data.</param>
        /// <param name="userProfileDbResult">user profile from DbResult.</param>
        /// <param name="readResult">read result.</param>
        /// <param name="termsOfService">terms of service.</param>
        /// <param name="configuration">configuration.</param>
        /// <param name="userProfileHistoryDbResult">user profile history from DbResult.</param>
        public UserProfileServiceMock(string hdId, DBStatusCode dbResultStatus, UserProfile userProfileData, DBResult<UserProfile> userProfileDbResult, DBResult<IEnumerable<UserPreference>> readResult, LegalAgreement termsOfService, IConfiguration configuration, DBResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult)
        {
            int limit = configuration.GetSection(this.webClientConfigSection).GetValue(this.userProfileHistoryRecordLimitKey, 2);

            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                new NotificationSettingsServiceMock().Object,
                new UserProfileDelegateMock(userProfileData, userProfileDbResult, hdId, userProfileHistoryDbResult, limit).Object,
                new UserPreferenceDelegateMock(readResult, hdId).Object,
                new LegalAgreementDelegateMock(termsOfService).Object,
                new MessagingVerificationDelegateMock().Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="userProfileData">user profile data.</param>
        /// <param name="userProfileDbResult">user profile from DbResult.</param>
        /// <param name="registrationStatus">registration status.</param>
        /// <param name="configuration">configuration.</param>
        public UserProfileServiceMock(string message, UserProfile userProfileData, DBResult<UserProfile> userProfileDbResult, string registrationStatus, IConfiguration configuration)
        {
            ExternalConfiguration externalConfiguration = new()
            {
                WebClient = new WebClientConfiguration()
                {
                    RegistrationStatus = registrationStatus,
                },
            };
            this.emptyConfigServiceMock.Setup(s => s.GetConfiguration()).Returns(externalConfiguration);

            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                new NotificationSettingsServiceMock().Object,
                new UserProfileDelegateMock(userProfileData, userProfileDbResult).Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new MessagingVerificationDelegateMock().Object,
                new CryptoDelegateMock(message).Object,
                new Mock<IHttpContextAccessor>().Object,
                configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="hdid">hdid.</param>
        /// <param name="patientModel">patient model.</param>
        /// <param name="minPatientAge">minimum patient age.</param>
        /// <param name="configuration">configuration.</param>
        public UserProfileServiceMock(string hdid, PatientModel patientModel, int minPatientAge, IConfiguration configuration)
        {
            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new PatientServiceMock(hdid, patientModel).Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                new ConfigServiceMock(minPatientAge).Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IUserProfileDelegate>().Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new MessagingVerificationDelegateMock().Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="hdId">hdId.</param>
        /// <param name="userProfileData">user profile data.</param>
        /// <param name="userProfileDbResult">user profile from DbResult.</param>
        /// <param name="readResult">read result.</param>
        /// <param name="messagingVerification">messaging verification.</param>
        /// <param name="termsOfService">terms of service.</param>
        /// <param name="configuration">configuration.</param>
        /// <param name="commit">commit.</param>
        public UserProfileServiceMock(string hdId, UserProfile userProfileData, DBResult<UserProfile> userProfileDbResult, DBResult<IEnumerable<UserPreference>> readResult, MessagingVerification messagingVerification, LegalAgreement termsOfService, IConfiguration configuration, bool commit = true)
        {
            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                this.emptyConfigServiceMock.Object,
                new Mock<IEmailQueueService>().Object,
                new NotificationSettingsServiceMock().Object,
                new UserProfileDelegateMock(hdId, userProfileData, userProfileDbResult, commit).Object,
                new UserPreferenceDelegateMock(readResult, hdId).Object,
                new LegalAgreementDelegateMock(termsOfService).Object,
                new MessagingVerificationDelegateMock(messagingVerification).Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="readResult">read result.</param>
        /// <param name="action">action.</param>
        /// <param name="configuration">configuration.</param>
        public UserProfileServiceMock(DBResult<UserPreference> readResult, string action, IConfiguration configuration)
        {
            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                new Mock<IConfigurationService>().Object,
                new Mock<IEmailQueueService>().Object,
                new Mock<INotificationSettingsService>().Object,
                new Mock<IUserProfileDelegate>().Object,
                new UserPreferenceDelegateMock(readResult, action).Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                new Mock<IHttpContextAccessor>().Object,
                configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileServiceMock"/> class.
        /// </summary>
        /// <param name="hdId">hdId.</param>
        /// <param name="userProfileData">user profile data.</param>
        /// <param name="userProfileDbResult">user profile from DbResult.</param>
        /// <param name="headerDictionary">header dictionary.</param>
        /// <param name="configuration">configuration.</param>
        /// <param name="shouldEmailCommit">should email commit.</param>
        /// <param name="shouldProfileCommit">should profile commit.</param>
        public UserProfileServiceMock(string hdId, UserProfile userProfileData, DBResult<UserProfile> userProfileDbResult, IHeaderDictionary headerDictionary, IConfiguration configuration, bool shouldEmailCommit, bool shouldProfileCommit = true)
        {
            this.userProfileService = new UserProfileService(
                new Mock<ILogger<UserProfileService>>().Object,
                new Mock<IPatientService>().Object,
                new Mock<IUserEmailService>().Object,
                new Mock<IUserSMSService>().Object,
                new Mock<IConfigurationService>().Object,
                new EmailQueueServiceMock(shouldEmailCommit).Object,
                new Mock<INotificationSettingsService>().Object,
                new UserProfileDelegateMock(hdId, userProfileData, userProfileDbResult, shouldProfileCommit).Object,
                new Mock<IUserPreferenceDelegate>().Object,
                new Mock<ILegalAgreementDelegate>().Object,
                new Mock<IMessagingVerificationDelegate>().Object,
                new Mock<ICryptoDelegate>().Object,
                new HttpContextAccessorMock(headerDictionary).Object,
                configuration);
        }

        /// <summary>
        /// GetUserProfileServiceMock.
        /// </summary>
        /// <returns>UserProfileService.</returns>
        public UserProfileService UserProfileServiceMockInstance()
        {
            return this.userProfileService;
        }
    }
}
