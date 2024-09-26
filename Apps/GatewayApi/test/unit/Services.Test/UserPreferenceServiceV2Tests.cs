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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling.Exceptions;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserPreferenceServiceV2's Unit Tests.
    /// </summary>
    public class UserPreferenceServiceV2Tests
    {
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// CreateUserPreferenceAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserPreference()
        {
            // Arrange
            UserPreferenceModel expected = GenerateUserPreferenceModel();
            UserPreference createUserPreference = MappingService.MapToUserPreference(expected);

            DbResult<UserPreference> dbResult = new()
            {
                Payload = createUserPreference,
                Status = DbStatusCode.Created,
            };

            IUserPreferenceServiceV2 service = SetupCreateUserPreferenceMock(dbResult, createUserPreference);

            // Act
            UserPreferenceModel actual = await service.CreateUserPreferenceAsync(Hdid, expected);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// CreateUserPreferenceAsync throws Exception.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateUserPreferenceAsyncThrowsException()
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = GenerateUserPreferenceModel();
            UserPreference createUserPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> expected = new()
            {
                Payload = createUserPreference,
                Status = DbStatusCode.Error,
                Message = "DB Error",
            };

            IUserPreferenceServiceV2 service = SetupCreateUserPreferenceMock(expected, createUserPreference);

            // Act and Assert
            DatabaseException actual = await Assert.ThrowsAsync<DatabaseException>(
                async () => { await service.CreateUserPreferenceAsync(Hdid, userPreferenceModel); });
            Assert.Equal(expected.Message, actual.Message);
        }

        /// <summary>
        /// UpdateUserPreferenceAsync call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldUpdateUserPreferenceAsync()
        {
            // Arrange
            UserPreferenceModel expected = GenerateUserPreferenceModel();
            UserPreference updateUserPreference = MappingService.MapToUserPreference(expected);

            DbResult<UserPreference> dbResult = new()
            {
                Payload = updateUserPreference,
                Status = DbStatusCode.Updated,
            };

            IUserPreferenceServiceV2 service = SetupUpdateUserPreferenceMock(dbResult, updateUserPreference);

            // Act
            UserPreferenceModel actual = await service.UpdateUserPreferenceAsync(Hdid, expected);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// UpdateUserPreferenceAsync throws DatabaseException.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UpdateUserPreferenceAsyncThrowsDatabaseException()
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = GenerateUserPreferenceModel();
            UserPreference updateUserPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> expected = new()
            {
                Payload = updateUserPreference,
                Status = DbStatusCode.Error,
                Message = "DB Error",
            };

            IUserPreferenceServiceV2 service = SetupUpdateUserPreferenceMock(expected, updateUserPreference);

            // Act and Assert
            DatabaseException actual = await Assert.ThrowsAsync<DatabaseException>(
                async () => { await service.UpdateUserPreferenceAsync(Hdid, userPreferenceModel); });
            Assert.Equal(expected.Message, actual.Message);
        }

        /// <summary>
        /// GetUserPreferenceAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserPreferences()
        {
            // Arrange
            List<UserPreference> preferences = [GenerateUserPreference()];
            Dictionary<string, UserPreferenceModel> expected = preferences.Select(MappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);
            IUserPreferenceServiceV2 service = SetupGetUserPreferencesMock(preferences);

            // Act
            Dictionary<string, UserPreferenceModel> actual = await service.GetUserPreferencesAsync(Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static IUserPreferenceServiceV2 GetUserPreferenceService(IMock<IUserPreferenceDelegate> userPreferenceDelegateMock)
        {
            return new UserPreferenceServiceV2(
                userPreferenceDelegateMock.Object,
                MappingService,
                new Mock<ILogger<UserPreferenceServiceV2>>().Object);
        }

        private static UserPreference GenerateUserPreference()
        {
            return new()
            {
                HdId = Hdid,
                Preference = "quickLinks",
                Value = "[{\"name\":\"Medications\",\"filter\":{\"modules\":[\"Medication\"]}},{\"name\":\"My Notes\",\"filter\":{\"modules\":[\"Note\"]}}]",
            };
        }

        private static UserPreferenceModel GenerateUserPreferenceModel()
        {
            return new()
            {
                HdId = Hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
                CreatedBy = Hdid,
                UpdatedBy = Hdid,
            };
        }

        private static IUserPreferenceServiceV2 SetupCreateUserPreferenceMock(DbResult<UserPreference> dbResult, UserPreference createUserPreference)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.CreateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == createUserPreference.Preference),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            userPreferenceDelegateMock.Setup(
                    s => s.UpdateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == createUserPreference.Preference),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            return new UserPreferenceServiceV2(
                userPreferenceDelegateMock.Object,
                MappingService,
                new Mock<ILogger<UserPreferenceServiceV2>>().Object);
        }

        private static IUserPreferenceServiceV2 SetupUpdateUserPreferenceMock(DbResult<UserPreference> dbResult, UserPreference updateUserPreference)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();

            userPreferenceDelegateMock.Setup(
                    s => s.UpdateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == updateUserPreference.Preference),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            return new UserPreferenceServiceV2(
                userPreferenceDelegateMock.Object,
                MappingService,
                new Mock<ILogger<UserPreferenceServiceV2>>().Object);
        }

        private static IUserPreferenceServiceV2 SetupGetUserPreferencesMock(List<UserPreference> preferences)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.GetUserPreferencesAsync(It.Is<string>(x => x == Hdid), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preferences);

            return GetUserPreferenceService(userPreferenceDelegateMock);
        }
    }
}
