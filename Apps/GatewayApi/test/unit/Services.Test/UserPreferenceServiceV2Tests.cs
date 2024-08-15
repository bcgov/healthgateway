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
        /// CreateUserPreferenceAsync call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateUserPreferenceAsync()
        {
            // Arrange
            UserPreferenceModel expected = GenerateUserPreferenceModel();
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Created);

            // Act
            UserPreferenceModel actual = await mock.Service.CreateUserPreferenceAsync(mock.Hdid, mock.UserPreferenceModel);

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
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Error);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(
                async () => { await mock.Service.CreateUserPreferenceAsync(mock.Hdid, mock.UserPreferenceModel); });
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
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Updated);

            // Act
            UserPreferenceModel actual = await mock.Service.UpdateUserPreferenceAsync(mock.Hdid, mock.UserPreferenceModel);

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
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Error);

            // Act and Assert
            await Assert.ThrowsAsync<DatabaseException>(
                async () => { await mock.Service.UpdateUserPreferenceAsync(mock.Hdid, mock.UserPreferenceModel); });
        }

        /// <summary>
        /// GetUserPreferenceAsync call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserPreferencesAsync()
        {
            // Arrange
            List<UserPreference> preferences = [GenerateUserPreference()];
            Dictionary<string, UserPreferenceModel> expected = preferences.Select(MappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);
            UserPreferencesMock mock = SetupUserPreferencesMock();

            // Act
            Dictionary<string, UserPreferenceModel> actual = await mock.Service.GetUserPreferencesAsync(mock.Hdid);

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
            };
        }

        private static UserPreferenceMock SetupUserPreferenceMock(DbStatusCode dbStatusCode)
        {
            UserPreferenceModel userPreferenceModel = GenerateUserPreferenceModel();
            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> dbResult = new()
            {
                Payload = userPreference,
                Status = dbStatusCode,
                Message = dbStatusCode == DbStatusCode.Error ? "DB Error" : string.Empty,
            };

            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.CreateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == userPreference.Preference),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            userPreferenceDelegateMock.Setup(
                    s => s.UpdateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == userPreference.Preference),
                        It.Is<bool>(x => x == true),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            IUserPreferenceServiceV2 service = GetUserPreferenceService(userPreferenceDelegateMock);

            return new UserPreferenceMock(service, Hdid, userPreferenceModel);
        }

        private static UserPreferencesMock SetupUserPreferencesMock()
        {
            List<UserPreference> preferences = [GenerateUserPreference()];
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.GetUserPreferencesAsync(It.Is<string>(x => x == Hdid), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preferences);

            IUserPreferenceServiceV2 service = GetUserPreferenceService(userPreferenceDelegateMock);
            return new(service, Hdid);
        }

        private sealed record UserPreferenceMock(
            IUserPreferenceServiceV2 Service,
            string Hdid,
            UserPreferenceModel UserPreferenceModel);

        private sealed record UserPreferencesMock(
            IUserPreferenceServiceV2 Service,
            string Hdid);
    }
}
