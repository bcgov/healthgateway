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
    using AutoMapper;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Constants;
    using HealthGateway.GatewayApi.MapUtils;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Constants;
    using HealthGateway.GatewayApiTests.Services.Test.Mock;
    using HealthGateway.GatewayApiTests.Services.Test.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private readonly string hdid = Guid.NewGuid().ToString();
        private readonly Guid termsOfServiceGuid = Guid.Parse("c99fd839-b4a2-40f9-b103-529efccd0dcd");

        /// <summary>
        /// GetUserProfile call - test for status Read, Error and NotFound.
        /// </summary>
        /// <param name="dBStatus">Db Status code.</param>
        /// <param name="expectedHasTourChanged">Used to test if the app tour has changes.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Read, true)]
        [InlineData(DbStatusCode.Read, false)]
        [InlineData(DbStatusCode.Error)]
        [InlineData(DbStatusCode.NotFound)]
        public async Task ShouldGetUserProfile(DbStatusCode dBStatus, bool expectedHasTourChanged = false)
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
                Status = dBStatus,
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

            IEnumerable<UserProfileHistory> userProfileHistories = new List<UserProfileHistory>
            {
                // Number of User Profile History records should match UserProfileHistoryRecordLimit value in UnitTest.json
                userProfileHistoryMinus1,
                userProfileHistoryMinus2,
            };

            DbResult<IEnumerable<UserProfileHistory>> userProfileHistoryDbResult = new()
            {
                Payload = userProfileHistories,
                Status = dBStatus,
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

            List<UserPreference> userPreferences = new()
                { dbUserPreference };

            DbResult<IEnumerable<UserPreference>> readResult = new()
            {
                Payload = userPreferences,
                Status = DbStatusCode.Read,
            };

            HashSet<DataSource> dataSources = new()
            {
                DataSource.Immunization,
                DataSource.Medication,
            };

            UserProfileModel expected = UserProfileMapUtils.CreateFromDbModel(userProfile, Guid.Empty, MapperUtil.InitializeAutoMapper());
            UserProfileServiceMock mockService = new(GetIConfigurationRoot(null));
            mockService.SetupGetOrSetCache<DateTime?>(lastTourChangeDateTime, $"{TourApplicationSettings.Application}:{TourApplicationSettings.Component}")
                .SetupPatientRepository(this.hdid, dataSources)
                .SetupUserProfileDelegateMockGetUpdateAndHistory(this.hdid, userProfile, userProfileDbResult, userProfileHistoryDbResult)
                .SetupLegalAgreementDelegateMock(termsOfService)
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel)
                .SetupUserPreferenceDelegateMockReturnPreferences(this.hdid, readResult);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.GetUserProfile(this.hdid, newLoginDateTime).ConfigureAwait(true);

            // Assert
            if (dBStatus == DbStatusCode.Read)
            {
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

            if (dBStatus == DbStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.True(actualResult.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
            }

            if (dBStatus == DbStatusCode.NotFound)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Null(actualResult.ResourcePayload?.HdId);
            }
        }

        /// <summary>
        /// Validates the Update Accepted Terms of Service.
        /// </summary>
        /// <param name="readStatus"> the status to return from the mock db delegate on get user.</param>
        /// <param name="updatedStatus"> the status to return from the mock db delegate after the update.</param>
        /// <param name="resultStatus"> The expected RequestResult status.</param>
        [Theory]
        [InlineData(DbStatusCode.Read, DbStatusCode.Updated, ResultType.Success)]
        [InlineData(DbStatusCode.NotFound, DbStatusCode.Error, ResultType.Error)]
        [InlineData(DbStatusCode.Read, DbStatusCode.Error, ResultType.Error)]
        public void ShouldUpdateTerms(DbStatusCode readStatus, DbStatusCode updatedStatus, ResultType resultStatus)
        {
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

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
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, readProfileDbResult, userProfileDbResultUpdate: updatedProfileDbResult);
            IUserProfileService service = mockService.UserProfileServiceMockInstance();
            RequestResult<UserProfileModel> actualResult = service.UpdateAcceptedTerms(this.hdid, Guid.Empty);

            Assert.True(actualResult.ResultStatus == resultStatus);
            if (actualResult.ResultStatus == ResultType.Success)
            {
                Assert.True(actualResult.ResourcePayload?.TermsOfServiceId == Guid.Empty);
            }
        }

        /// <summary>
        /// CreateUserProfile call.
        /// </summary>
        /// <param name="dbStatus">Db status code.</param>
        /// <param name="registration">Registration status code.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created, RegistrationStatus.Open)]
        [InlineData(DbStatusCode.Error, RegistrationStatus.Closed)]
        public async Task ShouldInsertUserProfile(DbStatusCode dbStatus, string registration)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(-20),
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = dbStatus,
            };
            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
                { "WebClient:RegistrationStatus", registration },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupUserProfileDelegateMockInsert(userProfile, userProfileDbResult)
                .SetupCryptoDelegateMockGenerateKey("abc")
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(
                    new CreateUserRequest { Profile = userProfile },
                    DateTime.Today,
                    It.IsAny<string>())
                .ConfigureAwait(true);

            // Assert
            if (dbStatus == DbStatusCode.Created && registration == RegistrationStatus.Open)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dbStatus == DbStatusCode.Error && registration == RegistrationStatus.Closed)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.Equal(ErrorTranslator.InternalError(ErrorType.InvalidState), actualResult.ResultError?.ErrorCode);
                Assert.Equal("Registration is closed", actualResult.ResultError!.ResultMessage);
            }
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

            UserProfileModel expected = UserProfileMapUtils.CreateFromDbModel(userProfile, userProfile.TermsOfServiceId, MapperUtil.InitializeAutoMapper());

            Dictionary<string, string?> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(localConfig))
                .SetupUserProfileDelegateMockInsert(userProfile, insertResult)
                .SetupCryptoDelegateMockGenerateKey("abc")
                .SetupPatientServiceMockCustomPatient(this.hdid, patientModel);
            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(
                    new CreateUserRequest { Profile = userProfile },
                    DateTime.Today,
                    It.IsAny<string>())
                .ConfigureAwait(true);

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
            RequestResult<bool> actualResult = await service.ValidateMinimumAge(this.hdid).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expected.ResourcePayload, actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetUserPreferences call.
        /// </summary>
        /// <param name="dBStatusCode">Db status code.</param>
        [Theory]
        [InlineData(DbStatusCode.Read)]
        [InlineData(DbStatusCode.Error)]
        public void ShouldGetUserPreference(DbStatusCode dBStatusCode)
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
                Status = dBStatusCode,
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

            List<UserPreferenceModel> userPreferences = new()
                { userPreferenceModel };

            List<UserPreference> dbUserPreferences = new();
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            UserPreference userPreference = autoMapper.Map<UserPreference>(userPreferenceModel);
            dbUserPreferences.Add(userPreference);

            DbResult<IEnumerable<UserPreference>> readResult = new()
            {
                Payload = dbUserPreferences,
                Status = dBStatusCode,
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupLegalAgreementDelegateMock(termsOfService)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupUserPreferenceDelegateMockReturnPreferences(this.hdid, readResult)
                .SetupMessagingVerificationDelegateMockCustomMessage(new MessagingVerification());

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<Dictionary<string, UserPreferenceModel>> actualResult = service.GetUserPreferences(this.hdid);

            // Assert
            if (dBStatusCode == DbStatusCode.Read)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Equal(actualResult.ResourcePayload?.Count, userPreferences.Count);
                Assert.Equal(actualResult.ResourcePayload?["TutorialPopover"].Value, userPreferences[0].Value);
            }

            if (dBStatusCode == DbStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError?.ErrorCode);
            }
        }

        /// <summary>
        ///  CreateUserPreference call.
        /// </summary>
        /// <param name="dBStatusCode">dBStatusCode.</param>
        [Theory]
        [InlineData(DbStatusCode.Created)]
        [InlineData(DbStatusCode.Error)]
        public void ShouldCreateUserPreference(DbStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            UserPreference userPreference = autoMapper.Map<UserPreference>(userPreferenceModel);
            DbResult<UserPreference> readResult = new()
            {
                Payload = userPreference,
                Status = dBStatusCode,
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserPreferenceDelegateMockActions(TestConstants.CreateAction, readResult);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = service.CreateUserPreference(userPreferenceModel);

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
        [Theory]
        [InlineData(DbStatusCode.Updated)]
        [InlineData(DbStatusCode.Error)]
        public void ShouldUpdateUserPreference(DbStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            IMapper autoMapper = MapperUtil.InitializeAutoMapper();
            UserPreference userPreference = autoMapper.Map<UserPreference>(userPreferenceModel);
            DbResult<UserPreference> readResult = new()
            {
                Payload = userPreference,
                Status = dBStatusCode,
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupUserPreferenceDelegateMockActions(TestConstants.UpdateAction, readResult);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = service.UpdateUserPreference(userPreferenceModel);

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
        [Fact]
        public void ShouldCloseUserProfile()
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

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };
            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupHttpAccessorMockCustomHeaders(headerDictionary)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path (Update Closed DateTime).
        /// </summary>
        [Fact]
        public void PreviouslyClosedUserProfile()
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

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupHttpAccessorMockCustomHeaders(headerDictionary)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(userProfile.ClosedDateTime, actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path with email notification.
        /// </summary>
        [Fact]
        public void ShouldCloseUserProfileAndQueueNewEmail()
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

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupHttpAccessorMockCustomHeaders(headerDictionary)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// RecoverUserProfile - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldRecoverUserProfile()
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                ClosedDateTime = DateTime.Today,
                Email = "unit.test@hgw.ca",
            };

            DbResult<UserProfile> userProfileDbResult = new()
            {
                Payload = userProfile,
                Status = DbStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupHttpAccessorMockCustomHeaders(headerDictionary)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.RecoverUserProfile(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// RecoverUserProfile - Active happy Path.
        /// </summary>
        [Fact]
        public void ShouldRecoverUserProfileAlreadyActive()
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

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            UserProfileServiceMock mockService = new UserProfileServiceMock(GetIConfigurationRoot(null))
                .SetupHttpAccessorMockCustomHeaders(headerDictionary)
                .SetupUserProfileDelegateMockGetAndUpdate(this.hdid, userProfile, userProfileDbResult)
                .SetupEmailQueueServiceMock(false);

            IUserProfileService service = mockService.UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.RecoverUserProfile(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        private static IConfigurationRoot GetIConfigurationRoot(Dictionary<string, string?>? localConfig)
        {
            Dictionary<string, string?> myConfiguration = localConfig ?? new();

            return new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json", true)
                .AddInMemoryCollection(myConfiguration.ToList())
                .Build();
        }
    }
}
