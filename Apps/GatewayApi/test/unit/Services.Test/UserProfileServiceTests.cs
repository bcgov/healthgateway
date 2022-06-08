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
namespace HealthGateway.GatewayApi.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Constants;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Services.Test.Constants;
    using HealthGateway.GatewayApiTests.Services.Test.Mock;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
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
        [Theory]
        [InlineData(DBStatusCode.Read)]
        [InlineData(DBStatusCode.Error)]
        [InlineData(DBStatusCode.NotFound)]
        public void ShouldGetUserProfile(DBStatusCode dBStatus)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                LastLoginDateTime = DateTime.Today,
            };

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = dBStatus,
            };

            UserProfileHistory userProfileHistoryMinus1 = new()
            {
                HdId = Guid.NewGuid().ToString(),
                Id = Guid.Parse(this.hdid),
                LastLoginDateTime = DateTime.Today.AddDays(-1),
            };

            UserProfileHistory userProfileHistoryMinus2 = new()
            {
                HdId = Guid.NewGuid().ToString(),
                Id = Guid.Parse(this.hdid),
                LastLoginDateTime = DateTime.Today.AddDays(-2),
            };

            IEnumerable<UserProfileHistory> userProfileHistories = new List<UserProfileHistory>
            {
                // Number of User Profile History records should match UserProfileHistoryRecordLimit value in UnitTest.json
                userProfileHistoryMinus1,
                userProfileHistoryMinus2,
            };

            DBResult<IEnumerable<UserProfileHistory>> userProfileHistoryDBResult = new()
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

            UserPreference dbUserPreference = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = true.ToString(),
            };

            List<UserPreference> userPreferences = new();
            userPreferences.Add(dbUserPreference);

            DBResult<IEnumerable<UserPreference>> readResult = new()
            {
                Payload = userPreferences,
                Status = DBStatusCode.Read,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile, Guid.Empty);

            IUserProfileService service = new UserProfileServiceMock(this.hdid, dBStatus, userProfile, userProfileDBResult, readResult, termsOfService, GetIConfigurationRoot(null), userProfileHistoryDBResult).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.GetUserProfile(this.hdid, DateTime.Now);

            // Assert
            if (dBStatus == DBStatusCode.Read)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Equal(this.hdid, expected.HdId);
                Assert.True(actualResult.ResourcePayload?.HasTermsOfServiceUpdated);
                Assert.True(actualResult.ResourcePayload?.LastLoginDateTimes.Count == 3);
                Assert.Equal(actualResult.ResourcePayload?.LastLoginDateTimes[0], userProfile.LastLoginDateTime);
                Assert.Equal(actualResult.ResourcePayload?.LastLoginDateTimes[1], userProfileHistoryMinus1.LastLoginDateTime);
                Assert.Equal(actualResult.ResourcePayload?.LastLoginDateTimes[2], userProfileHistoryMinus2.LastLoginDateTime);
            }

            if (dBStatus == DBStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.True(actualResult?.ResultError?.ErrorCode.EndsWith("-CI-DB", StringComparison.InvariantCulture));
            }

            if (dBStatus == DBStatusCode.NotFound)
            {
                Assert.Equal(ResultType.Success, actualResult?.ResultStatus);
                Assert.Null(actualResult?.ResourcePayload?.HdId);
            }
        }

        /// <summary>
        /// Validates the Update Accepted Terms of Service.
        /// </summary>
        /// <param name="readStatus"> the status to return from the mock db delegate on get user.</param>
        /// <param name="updatedStatus"> the status to return from the mock db delegate after the update.</param>
        /// <param name="resultStatus"> The expected RequestResult status.</param>
        [Theory]
        [InlineData(DBStatusCode.Read, DBStatusCode.Updated, ResultType.Success)]
        [InlineData(DBStatusCode.NotFound, DBStatusCode.Error, ResultType.Error)]
        [InlineData(DBStatusCode.Read, DBStatusCode.Error, ResultType.Error)]
        public void ShouldUpdateTerms(DBStatusCode readStatus, DBStatusCode updatedStatus, ResultType resultStatus)
        {
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            DBResult<UserProfile> readProfileDBResult = new()
            {
                Payload = userProfile,
                Status = readStatus,
            };

            DBResult<UserProfile> updatedProfileDBResult = new()
            {
                Payload = userProfile,
                Status = updatedStatus,
            };

            DBResult<LegalAgreement> tosDbResult = new()
            {
                Status = DBStatusCode.Read,
                Payload = new LegalAgreement()
                {
                    Id = Guid.Empty,
                    CreatedBy = "MockData",
                    CreatedDateTime = DateTime.UtcNow,
                    EffectiveDate = DateTime.UtcNow,
                    LegalAgreementCode = LegalAgreementType.TermsofService,
                    LegalText = "Mock Terms of Service",
                },
            };

            Mock<ILegalAgreementDelegate> mockLegalAgreementDelegate = new Mock<ILegalAgreementDelegate>();
            mockLegalAgreementDelegate.Setup(s => s.GetActiveByAgreementType(LegalAgreementType.TermsofService)).Returns(tosDbResult);
            Mock<IUserProfileDelegate> mockUserProfileDelegate = new();
            mockUserProfileDelegate.Setup(s => s.GetUserProfile(It.IsAny<string>())).Returns(readProfileDBResult);
            mockUserProfileDelegate.Setup(s => s.Update(It.IsAny<UserProfile>(), true)).Returns(updatedProfileDBResult);
            IUserProfileService service = new UserProfileService(
                                                        new Mock<ILogger<UserProfileService>>().Object,
                                                        new Mock<IPatientService>().Object,
                                                        new Mock<IUserEmailService>().Object,
                                                        new Mock<IUserSMSService>().Object,
                                                        new Mock<IEmailQueueService>().Object,
                                                        new Mock<INotificationSettingsService>().Object,
                                                        mockUserProfileDelegate.Object,
                                                        new Mock<IUserPreferenceDelegate>().Object,
                                                        mockLegalAgreementDelegate.Object,
                                                        new Mock<IMessagingVerificationDelegate>().Object,
                                                        new Mock<ICryptoDelegate>().Object,
                                                        new Mock<IHttpContextAccessor>().Object,
                                                        GetIConfigurationRoot(null));
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
        [InlineData(DBStatusCode.Created, RegistrationStatus.Open)]
        [InlineData(DBStatusCode.Error, RegistrationStatus.Closed)]
        public async Task ShouldInsertUserProfile(DBStatusCode dbStatus, string registration)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
                Email = "unit.test@hgw.ca",
            };

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = dbStatus,
            };
            Dictionary<string, string> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
                { "WebClient:RegistrationStatus",  registration },
            };
            IUserProfileService service = new UserProfileServiceMock("abc", userProfile, userProfileDBResult, registration, GetIConfigurationRoot(localConfig)).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, DateTime.Today, It.IsAny<string>()).ConfigureAwait(true);

            // Assert
            if (dbStatus == DBStatusCode.Created && registration == RegistrationStatus.Open)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dbStatus == DBStatusCode.Error && registration == RegistrationStatus.Closed)
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

            DBResult<UserProfile> insertResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Created,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile, userProfile.TermsOfServiceId);

            Dictionary<string, string> localConfig = new()
            {
                { "WebClient:MinPatientAge", "0" },
            };
            IUserProfileService service = new UserProfileServiceMock("abc", userProfile, insertResult, RegistrationStatus.Open, GetIConfigurationRoot(localConfig)).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = await service.CreateUserProfile(new CreateUserRequest() { Profile = userProfile }, DateTime.Today, It.IsAny<string>()).ConfigureAwait(true);

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

            Dictionary<string, string> localConfig = new()
            {
                { "WebClient:MinPatientAge", "19" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, patientModel, GetIConfigurationRoot(localConfig)).UserProfileServiceMockInstance();
            PrimitiveRequestResult<bool> expected = new() { ResultStatus = ResultType.Success, ResourcePayload = false };

            // Act
            PrimitiveRequestResult<bool> actualResult = await service.ValidateMinimumAge(this.hdid).ConfigureAwait(true);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(expected.ResourcePayload, actualResult.ResourcePayload);
        }

        /// <summary>
        /// GetUserPreferences call.
        /// </summary>
        /// <param name="dBStatusCode">Db status code.</param>
        [Theory]
        [InlineData(DBStatusCode.Read)]
        [InlineData(DBStatusCode.Error)]
        public void ShouldGetUserPreference(DBStatusCode dBStatusCode)
        {
            // Arrange
            UserProfile userProfile = new()
            {
                HdId = this.hdid,
                TermsOfServiceId = this.termsOfServiceGuid,
            };

            DBResult<UserProfile> userProfileDBResult = new()
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

            List<UserPreferenceModel> userPreferences = new();
            userPreferences.Add(userPreferenceModel);

            List<UserPreference> dbUserPreferences = new();
            dbUserPreferences.Add(userPreferenceModel.ToDbModel());

            DBResult<IEnumerable<UserPreference>> readResult = new()
            {
                Payload = dbUserPreferences,
                Status = dBStatusCode,
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, readResult, new MessagingVerification(), termsOfService, GetIConfigurationRoot(null)).UserProfileServiceMockInstance();

            // Act
            RequestResult<Dictionary<string, UserPreferenceModel>> actualResult = service.GetUserPreferences(this.hdid);

            // Assert
            if (dBStatusCode == DBStatusCode.Read)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Equal(actualResult.ResourcePayload?.Count, userPreferences.Count);
                Assert.Equal(actualResult.ResourcePayload?["TutorialPopover"].Value, userPreferences[0].Value);
            }

            if (dBStatusCode == DBStatusCode.Error)
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
        [InlineData(DBStatusCode.Created)]
        [InlineData(DBStatusCode.Error)]
        public void ShouldCreateUserPreference(DBStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            DBResult<UserPreference> readResult = new()
            {
                Payload = userPreferenceModel.ToDbModel(),
                Status = dBStatusCode,
            };

            IUserProfileService service = new UserProfileServiceMock(readResult, TestConstants.CreateAction, GetIConfigurationRoot(null)).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = service.CreateUserPreference(userPreferenceModel);

            // Assert
            if (dBStatusCode == DBStatusCode.Created)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dBStatusCode == DBStatusCode.Error)
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
        [InlineData(DBStatusCode.Updated)]
        [InlineData(DBStatusCode.Error)]
        public void ShouldUpdateUserPreference(DBStatusCode dBStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = this.hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };
            DBResult<UserPreference> readResult = new()
            {
                Payload = userPreferenceModel.ToDbModel(),
                Status = dBStatusCode,
            };

            IUserProfileService service = new UserProfileServiceMock(readResult, TestConstants.UpdateAction, GetIConfigurationRoot(null)).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserPreferenceModel> actualResult = service.UpdateUserPreference(userPreferenceModel);

            // Assert
            if (dBStatusCode == DBStatusCode.Updated)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            }

            if (dBStatusCode == DBStatusCode.Error)
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

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, GetIConfigurationRoot(null), false, true).UserProfileServiceMockInstance();

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
            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, GetIConfigurationRoot(null), false, true).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(userProfile.ClosedDateTime, actualResult.ResourcePayload?.ClosedDateTime);
        }

        /// <summary>
        /// CloseUserProfile - Happy Path with email notificaition.
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

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, GetIConfigurationRoot(null), false, true).UserProfileServiceMockInstance();

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

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, GetIConfigurationRoot(null), false, true).UserProfileServiceMockInstance();

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

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Read,
            };

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "referer", "http://localhost/" },
            };

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, GetIConfigurationRoot(null), false, true).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.RecoverUserProfile(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }

        private static IConfigurationRoot GetIConfigurationRoot(Dictionary<string, string>? localConfig)
        {
            Dictionary<string, string> myConfiguration = localConfig ?? new();

            return new ConfigurationBuilder()
                .AddJsonFile("UnitTest.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
