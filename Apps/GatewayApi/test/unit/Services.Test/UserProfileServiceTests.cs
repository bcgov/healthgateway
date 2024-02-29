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
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Events;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Constants;
    using HealthGateway.GatewayApiTests.Services.Test.Mock;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        private readonly string hdid = Guid.NewGuid().ToString();
        private readonly Guid termsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// GetUserProfile call - test for status Read, Error and NotFound.
        /// </summary>
        /// <param name="expectedHasTourChanged">Used to test if the app tour has changes.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldGetUserProfile(bool expectedHasTourChanged = false)
        {
            DateTime newLoginDateTime = DateTime.Today;
            DateTime lastTourChangeDateTime = newLoginDateTime.AddDays(-1).AddMinutes(expectedHasTourChanged ? 10 : -10);

            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                LastLoginDateTime = newLoginDateTime.AddDays(-1),
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            DbResult<UserProfile> updatedUserProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Updated,
            };

            UserProfileHistory userProfileHistoryMinus1 = new()
            {
                HdId = Guid.NewGuid().ToString(),
                Id = Guid.Parse(this.hdid),
                LastLoginDateTime = newLoginDateTime.AddDays(-1),
            };

            UserProfileHistory userProfileHistoryMinus2 = new()
            {
                HdId = Guid.NewGuid().ToString(),
                Id = Guid.Parse(this.hdid),
                LastLoginDateTime = newLoginDateTime.AddDays(-2),
            };

            IList<UserProfileHistory> userProfileHistories = new List<UserProfileHistory>
            {
                // Number of User Profile History records should match UserProfileHistoryRecordLimit value in UnitTest.json
                userProfileHistoryMinus1,
                userProfileHistoryMinus2,
            };

            LegalAgreement termsOfService = new()
            {
                Id = Guid.NewGuid(),
                LegalText = string.Empty,
                EffectiveDate = DateTime.Now,
            };

            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-20),
            };

            UserPreference dbUserPreference = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = true.ToString(),
            };

            List<UserPreference> userPreferences = [dbUserPreference];

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            UserProfileModel expected = MappingService.MapToUserProfileModel(userProfile, Guid.Empty);
            UserProfileServiceMock mockService = new(GetIConfigurationRoot(null));
            mockService.SetupGetOrSetCache<DateTime?>(lastTourChangeDateTime, $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}")
                .SetupPatientRepository(this.hdid, dataSources)
                .SetupUserProfileDelegateMockGetUpdateAndHistory(this.hdid, userProfile, userProfileDbResult, userProfileHistories, updatedUserProfileDbResult)
                .SetupLegalAgreementDelegateMock(termsOfService)
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel)
                .SetupUserPreferenceDelegateMockReturnPreferences(this.hdid, userPreferences);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.GetUserProfileAsync(this.hdid, newLoginDateTime);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(this.hdid, expected.HdId);
            Assert.True(actualResult.ResourcePayload?.HasTermsOfServiceUpdated);
            Assert.True(actualResult.ResourcePayload?.LastLoginDateTimes.Count == 3);
            Assert.Equal(userProfile.LastLoginDateTime, actualResult.ResourcePayload?.LastLoginDateTimes[0]);
            Assert.Equal(userProfileHistoryMinus1.LastLoginDateTime, actualResult.ResourcePayload?.LastLoginDateTimes[1]);
            Assert.Equal(userProfileHistoryMinus2.LastLoginDateTime, actualResult.ResourcePayload?.LastLoginDateTimes[2]);
            Assert.Equal(expectedHasTourChanged, actualResult.ResourcePayload?.HasTourUpdated);
            Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
        }

        /// <summary>
        /// Validates the Update Accepted Terms of Service.
        /// </summary>
        /// <param name="readStatus"> the status to return from the mock db delegate on get user.</param>
        /// <param name="updatedStatus"> the status to return from the mock db delegate after the update.</param>
        /// <param name="resultStatus"> The expected RequestResult status.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, DbStatusCode.Updated, ResultType.Success)]
        [InlineData(DbStatusCode.NotFound, DbStatusCode.Error, ResultType.Error)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Error, ResultType.Error)]
        public async Task ShouldUpdateTerms(DbStatusCode readStatus, DbStatusCode updatedStatus, ResultType resultStatus)
        {
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            DbResult<UserProfile> readProfileDbResult = new()
            {
                Payload = userProfile,
                Status = readStatus,
            };

            DbResult<UserProfile> updatedProfileDbResult = new()
            {
                Payload = userProfile,
                Status = updatedStatus,
            };

            LegalAgreement legalAgreement = new()
            {
                Id = Guid.Empty,
                CreatedBy = "MockData",
                CreatedDateTime = DateTime.UtcNow,
                EffectiveDate = DateTime.UtcNow,
                LegalAgreementCode = LegalAgreementType.TermsOfService,
                LegalText = "Mock Terms of Service",
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupLegalAgreementDelegateMock(legalAgreement)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, readProfileDbResult, userProfileDbResultUpdate: updatedProfileDbResult)
                .SetupPatientRepository(this.hdid, dataSources);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();
            RequestResult<UserProfileModel> actualResult = await service.UpdateAcceptedTermsAsync(this.hdid, Guid.Empty);

            Assert.True(actualResult.ResultStatus == resultStatus);
            if (actualResult.ResultStatus == ResultType.Success)
            {
                Assert.True(actualResult.ResourcePayload?.TermsOfServiceId == Guid.Empty);
                Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
            }
        }

        /// <summary>
        /// CreateUserProfile call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertUserProfile()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-20),
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Created,
            };
            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
                {
                    $"{ChangeFeedOptions.ChangeFeed}:Accounts:Enabled",
                    "false"
                },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupUserProfileDelegateMockInsert(userProfile, userProfileDbResult, true)
                .SetupCryptoDelegateMockGenerateKey("abc")
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel)
                .SetupPatientRepository(this.hdid, dataSources);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfileAsync(
                    new CreateUserRequest { Profile = userProfile },
                    DateTime.Today,
                    It.IsAny<string>())
                ;

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
        }

        /// <summary>
        /// CreateUserProfile call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldInsertUserProfileWithChangeFeed()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-20),
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Created,
            };
            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
                {
                    $"{ChangeFeedOptions.ChangeFeed}:Accounts:Enabled",
                    "true"
                },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupUserProfileDelegateMockInsert(userProfile, userProfileDbResult, false)
                .SetupCryptoDelegateMockGenerateKey("abc")
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel)
                .SetupPatientRepository(this.hdid, dataSources);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfileAsync(
                new CreateUserRequest { Profile = userProfile },
                DateTime.Today,
                It.IsAny<string>());

            // Assert
            mockService.VerifyMessageSenderSendAsync<AccountCreatedEvent>();
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
        }

        /// <summary>
        /// CreateUserProfile - Happy Path with notification update.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldQueueNotificationUpdate()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = string.Empty,
            };

            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-20),
            };

            DbResult<UserProfile> insertResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Created,
            };

            UserProfileModel expected = MappingService.MapToUserProfileModel(userProfile, userProfile.TermsOfServiceId);

            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
                {
                    $"{ChangeFeedOptions.ChangeFeed}:Accounts:Enabled",
                    "false"
                },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupUserProfileDelegateMockInsert(userProfile, insertResult, true)
                .SetupCryptoDelegateMockGenerateKey("abc")
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel);
            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfileAsync(
                    new CreateUserRequest { Profile = userProfile },
                    DateTime.Today,
                    It.IsAny<string>())
                ;

            // Assert
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
            // Arrange
            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-15),
            };

            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "19" },
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel);
            IUserProfileService service = mockService.UserProfileServiceMockInstance();
            RequestResult<bool> expected = new() { ResultStatus = ResultType.Success, ResourcePayload = false };

            // Act
            RequestResult<bool> actualResult = await service.ValidateMinimumAgeAsync(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expected.ResourcePayload, actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetUserPreferences call.
        /// </summary>
        /// <param name="numberOfPreferences">The number of preferences to return.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task ShouldGetUserPreference(int numberOfPreferences)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            LegalAgreement termsOfService = new()
            {
                Id = Guid.NewGuid(),
                LegalText = string.Empty,
                EffectiveDate = DateTime.Now,
            };

            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = true.ToString(),
            };

            List<UserPreferenceModel> userPreferences = numberOfPreferences == 1 ? [userPreferenceModel] : [];

            List<UserPreference> dbUserPreferences = [];
            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);

            if (numberOfPreferences > 0)
            {
                dbUserPreferences.Add(userPreference);
            }

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupLegalAgreementDelegateMock(termsOfService)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupUserPreferenceDelegateMockReturnPreferences(this.hdid, dbUserPreferences)
                .SetupMessagingVerificationDelegateMockCustomMessage(new MessagingVerification());

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<Dictionary<string, UserPreferenceModel>> actualResult = await service.GetUserPreferencesAsync(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(userPreferences.Count, actualResult.ResourcePayload?.Count);

            if (numberOfPreferences > 0)
            {
                Assert.Equal(actualResult.ResourcePayload?["TutorialPopover"].Value, userPreferences[0].Value);
            }
        }

        /// <summary>
        ///  CreateUserPreference call.
        /// </summary>
        /// <param name="dBStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldCreateUserPreference(DbStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);
            DbResult<UserPreference> readResult = new()
            {
                Payload = userPreference,
                Status = dBStatusCode,
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserPreferenceDelegateMockActions(TestConstants.CreateAction, readResult);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = await service.CreateUserPreferenceAsync(userPreferenceModel);

            // Assert
            if (dBStatusCode == DbStatusCode.Created)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dBStatusCode == DbStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
            }
        }

        /// <summary>
        ///  UpdateUserPreference call.
        /// </summary>
        /// <param name="dBStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldUpdateUserPreference(DbStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);
            DbResult<UserPreference> readResult = new()
            {
                Payload = userPreference,
                Status = dBStatusCode,
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserPreferenceDelegateMockActions(TestConstants.UpdateAction, readResult);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = await service.UpdateUserPreferenceAsync(userPreferenceModel);

            // Assert
            if (dBStatusCode == DbStatusCode.Updated)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dBStatusCode == DbStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
            }
        }

        /// <summary>
        /// ShouldCloseUserProfile.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCloseUserProfile()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
            };

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            DbResult<UserProfile> userProfileUpdateDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Updated,
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult, userProfileDbResultUpdate: userProfileUpdateDbResult)
                .SetupEmailQueueServiceMock(false)
                .SetupPatientRepository(this.hdid, dataSources);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CloseUserProfileAsync(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
            Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path (Update Closed DateTime).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task PreviouslyClosedUserProfile()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                ClosedDateTime = DateTime.Today,
            };
            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CloseUserProfileAsync(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(userProfile.ClosedDateTime, actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path with email notification.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCloseUserProfileAndQueueNewEmail()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            DbResult<UserProfile> userProfileUpdateDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Updated,
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult, userProfileDbResultUpdate: userProfileUpdateDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CloseUserProfileAsync(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRecoverUserProfile()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                ClosedDateTime = DateTime.Today,
                Email = "unit.test@hgw.ca",
            };

            IList<DataSource> dataSources =
            [
                DataSource.Immunization,
                DataSource.Medication,
            ];

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false)
                .SetupPatientRepository(this.hdid, dataSources);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.RecoverUserProfileAsync(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
            Assert.Equal(dataSources, actualResult.ResourcePayload?.BlockedDataSources);
        }

        /// <summary>
        /// RecoverUserProfile - Active happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRecoverUserProfileAlreadyActive()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                ClosedDateTime = null,
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.RecoverUserProfileAsync(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// Build a test ready configuration populated through the available appsettings files.
        /// </summary>
        /// <param name="localConfig">Used to overwrite any of the appsettings brought in through the UnitTest.json</param>
        /// <returns>IConfigurationRoot instance.</returns>
        private static IConfigurationRoot GetIConfigurationRoot(Dictionary<string, string?>? localConfig)
        {
            Dictionary<string, string?> myConfiguration = localConfig ?? [];

            return new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
