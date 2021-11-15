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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Constants;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using HealthGateway.WebClientTests.Services.Test.Constants;
    using HealthGateway.WebClientTests.Services.Test.Mock;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserProfileService's Unit Tests.
    /// </summary>
    public class UserProfileServiceTests
    {
        private readonly string hdid = "1234567890123456789012345678901234567890123456789012";
        private readonly Mock<IConfigurationService> emptyConfigServiceMock = new();

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
                AcceptedTermsOfService = true,
                LastLoginDateTime = DateTime.Today,
            };

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
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

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);
            expected.HasTermsOfServiceUpdated = true;

            IUserProfileService service = new UserProfileServiceMock(this.hdid, dBStatus, userProfile, userProfileDBResult, readResult, termsOfService).UserProfileServiceMockInstance();

            // Act
            RequestResult<UserProfileModel> actualResult = service.GetUserProfile(this.hdid, DateTime.Now);

            // Assert
            if (dBStatus == DBStatusCode.Read)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Equal(this.hdid, expected.HdId);
                Assert.True(actualResult.ResourcePayload?.HasTermsOfServiceUpdated);
            }

            if (dBStatus == DBStatusCode.Error)
            {
                Assert.Equal(ResultType.Error, actualResult.ResultStatus);
                Assert.Equal("testhostServer-CI-DB", actualResult.ResultError?.ErrorCode);
            }

            if (dBStatus == DBStatusCode.NotFound)
            {
                Assert.Equal(ResultType.Success, actualResult.ResultStatus);
                Assert.Null(actualResult.ResourcePayload?.HdId);
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
                AcceptedTermsOfService = true,
                Email = "unit.test@hgw.ca",
            };

            DBResult<UserProfile> userProfileDBResult = new()
            {
                Payload = userProfile,
                Status = dbStatus,
            };

            var service = new UserProfileServiceMock("abc", userProfile, userProfileDBResult, registration).UserProfileServiceMockInstance();

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
                AcceptedTermsOfService = true,
                Email = string.Empty,
            };

            DBResult<UserProfile> insertResult = new()
            {
                Payload = userProfile,
                Status = DBStatusCode.Created,
            };

            UserProfileModel expected = UserProfileModel.CreateFromDbModel(userProfile);

            var service = new UserProfileServiceMock("abc", userProfile, insertResult, RegistrationStatus.Open).UserProfileServiceMockInstance();

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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, patientModel, 19).UserProfileServiceMockInstance();
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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, readResult, new MessagingVerification(), termsOfService).UserProfileServiceMockInstance();

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

            IUserProfileService service = new UserProfileServiceMock(readResult, TestConstants.CreateAction).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.CreateUserPreference(userPreferenceModel);

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

            IUserProfileService service = new UserProfileServiceMock(readResult, TestConstants.UpdateAction).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.UpdateUserPreference(userPreferenceModel);

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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, false, true).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, false, true).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, false, true).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.CloseUserProfile(this.hdid, Guid.NewGuid());

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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, false, true).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.RecoverUserProfile(this.hdid);

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
                AcceptedTermsOfService = true,
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

            IUserProfileService service = new UserProfileServiceMock(this.hdid, userProfile, userProfileDBResult, headerDictionary, false, true).UserProfileServiceMockInstance();

            // Act
            var actualResult = service.RecoverUserProfile(this.hdid);

            // Assert
            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResourcePayload?.ClosedDateTime);
        }
    }
}
