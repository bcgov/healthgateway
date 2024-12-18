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
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.ErrorHandling;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserPreferenceService's Unit Tests.
    /// </summary>
    public class UserPreferenceServiceTests
    {
        private const string Hdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// CreateUserPreferenceAsync.
        /// </summary>
        /// <param name="dbStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldCreateUserPreference(DbStatusCode dbStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = Hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };

            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> createUserPreferenceResult = new()
            {
                Payload = userPreference,
                Status = dbStatusCode,
                Message = dbStatusCode == DbStatusCode.Error ? "DB Error" : string.Empty,
            };

            RequestResult<UserPreferenceModel> expected = new()
            {
                ResultStatus = dbStatusCode == DbStatusCode.Created ? ResultType.Success : ResultType.Error,
                ResourcePayload = dbStatusCode == DbStatusCode.Created ? userPreferenceModel : null,
                ResultError = dbStatusCode == DbStatusCode.Error
                    ? new()
                    {
                        ResultMessage = "DB Error",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };

            IUserPreferenceService service = SetupUserPreferenceServiceForCreateUserPreference(createUserPreferenceResult, userPreference);

            // Act
            RequestResult<UserPreferenceModel> actual = await service.CreateUserPreferenceAsync(userPreferenceModel);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// UpdateUserPreferenceAsync.
        /// </summary>
        /// <param name="dbStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldUpdateUserPreference(DbStatusCode dbStatusCode)
        {
            // Arrange
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = Hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };

            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> updateUserPreferenceResult = new()
            {
                Payload = userPreference,
                Status = dbStatusCode,
                Message = dbStatusCode == DbStatusCode.Error ? "DB Error" : string.Empty,
            };

            RequestResult<UserPreferenceModel> expected = new()
            {
                ResultStatus = dbStatusCode == DbStatusCode.Updated ? ResultType.Success : ResultType.Error,
                ResourcePayload = dbStatusCode == DbStatusCode.Updated ? userPreferenceModel : null,
                ResultError = dbStatusCode == DbStatusCode.Error
                    ? new()
                    {
                        ResultMessage = "DB Error",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };

            IUserPreferenceService service = SetupUserPreferenceServiceForUpdateUserPreference(updateUserPreferenceResult, userPreference);

            // Act
            RequestResult<UserPreferenceModel> actual = await service.UpdateUserPreferenceAsync(userPreferenceModel);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// GetUserPreferenceAsync.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserPreferences()
        {
            // Arrange
            UserPreference userPreference = new()
            {
                HdId = Hdid,
                Preference = "quickLinks",
                Value = "[{\"name\":\"Medications\",\"filter\":{\"modules\":[\"Medication\"]}},{\"name\":\"My Notes\",\"filter\":{\"modules\":[\"Note\"]}}]",
            };

            List<UserPreference> preferences = [userPreference];
            Dictionary<string, UserPreferenceModel> expected = preferences.Select(MappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);

            IUserPreferenceService service = SetupUserPreferenceServiceForGetUserPreference(preferences);

            // Act
            Dictionary<string, UserPreferenceModel> actual = await service.GetUserPreferencesAsync(Hdid);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static IUserPreferenceService GetUserPreferenceService(IMock<IUserPreferenceDelegate> userPreferenceDelegateMock)
        {
            return new UserPreferenceService(
                userPreferenceDelegateMock.Object,
                MappingService);
        }

        private static IUserPreferenceService SetupUserPreferenceServiceForCreateUserPreference(DbResult<UserPreference> dbResult, UserPreference userPreference)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.CreateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == userPreference.Preference),
                        It.Is<bool>(x => x),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            return GetUserPreferenceService(userPreferenceDelegateMock);
        }

        private static IUserPreferenceService SetupUserPreferenceServiceForUpdateUserPreference(DbResult<UserPreference> dbResult, UserPreference userPreference)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.UpdateUserPreferenceAsync(
                        It.Is<UserPreference>(x => x.Preference == userPreference.Preference),
                        It.Is<bool>(x => x),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbResult);

            return GetUserPreferenceService(userPreferenceDelegateMock);
        }

        private static IUserPreferenceService SetupUserPreferenceServiceForGetUserPreference(List<UserPreference> preferences)
        {
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.GetUserPreferencesAsync(It.Is<string>(x => x == Hdid), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preferences);

            return GetUserPreferenceService(userPreferenceDelegateMock);
        }
    }
}
