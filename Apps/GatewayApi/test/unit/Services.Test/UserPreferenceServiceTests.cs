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
    using Microsoft.Extensions.Logging;
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
        /// CreateUserPreferenceAsync call.
        /// </summary>
        /// <param name="dBStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Created)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldCreateUserPreferenceAsync(DbStatusCode dBStatusCode)
        {
            // Arrange
            CreateUserPreferenceMock mock = SetupCreateUserPreferenceMock(dBStatusCode);

            // Act
            RequestResult<UserPreferenceModel> actual = await mock.Service.CreateUserPreferenceAsync(mock.UserPreferenceModel);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        /// <summary>
        /// UpdateUserPreferenceAsync call.
        /// </summary>
        /// <param name="dBStatusCode">dBStatusCode.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(DbStatusCode.Updated)]
        [InlineData(DbStatusCode.Error)]
        public async Task ShouldUpdateUserPreferenceAsync(DbStatusCode dBStatusCode)
        {
            // Arrange
            UpdateUserPreferenceMock mock = SetupUpdateUserPreferenceMock(dBStatusCode);

            // Act
            RequestResult<UserPreferenceModel> actual = await mock.Service.UpdateUserPreferenceAsync(mock.UserPreferenceModel);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        /// <summary>
        /// GetUserPreferenceAsync call.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetUserPreferencesAsync()
        {
            // Arrange
            GetUserPreferenceMock mock = SetupGetUserPreferenceMock();

            // Act
            Dictionary<string, UserPreferenceModel> actual = await mock.Service.GetUserPreferencesAsync(mock.Hdid);

            // Assert
            actual.ShouldDeepEqual(mock.Expected);
        }

        private static IUserPreferenceService GetUserPreferenceService(IMock<IUserPreferenceDelegate> userPreferenceDelegateMock)
        {
            return new UserPreferenceService(userPreferenceDelegateMock.Object, MappingService, new Mock<ILogger<UserPreferenceService>>().Object);
        }

        private static UserPreferenceMock SetupUserPreferenceMock(DbStatusCode action, DbStatusCode dbStatusCode)
        {
            UserPreferenceModel userPreferenceModel = new()
            {
                HdId = Hdid,
                Preference = "TutorialPopover",
                Value = "mocked value",
            };

            UserPreference userPreference = MappingService.MapToUserPreference(userPreferenceModel);

            DbResult<UserPreference> dbResult = new()
            {
                Payload = userPreference,
                Status = dbStatusCode,
                Message = dbStatusCode == DbStatusCode.Error ? "DB Error" : string.Empty,
            };

            RequestResult<UserPreferenceModel> expected = new()
            {
                ResultStatus = dbStatusCode == action ? ResultType.Success : ResultType.Error,
                ResourcePayload = dbStatusCode == action ? userPreferenceModel : null,
                ResultError = dbStatusCode == DbStatusCode.Error
                    ? new()
                    {
                        ResultMessage = "DB Error",
                        ErrorCode = ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database),
                    }
                    : null,
            };

            return new(expected, userPreference, dbResult, userPreferenceModel);
        }

        private static CreateUserPreferenceMock SetupCreateUserPreferenceMock(DbStatusCode dbStatusCode)
        {
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Created, dbStatusCode);
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.CreateUserPreferenceAsync(It.Is<UserPreference>(x => x.Preference == mock.UserPreference.Preference), It.Is<bool>(x => x == true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mock.DbResult);

            IUserPreferenceService service = GetUserPreferenceService(userPreferenceDelegateMock);
            return new(service, mock.Expected, mock.UserPreferenceModel);
        }

        private static UpdateUserPreferenceMock SetupUpdateUserPreferenceMock(DbStatusCode dbStatusCode)
        {
            UserPreferenceMock mock = SetupUserPreferenceMock(DbStatusCode.Updated, dbStatusCode);
            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.UpdateUserPreferenceAsync(It.Is<UserPreference>(x => x.Preference == mock.UserPreference.Preference), It.Is<bool>(x => x == true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mock.DbResult);

            IUserPreferenceService service = GetUserPreferenceService(userPreferenceDelegateMock);
            return new(service, mock.Expected, mock.UserPreferenceModel);
        }

        private static GetUserPreferenceMock SetupGetUserPreferenceMock()
        {
            UserPreference userPreference = new()
            {
                HdId = Hdid,
                Preference = "quickLinks",
                Value = "[{\"name\":\"Medications\",\"filter\":{\"modules\":[\"Medication\"]}},{\"name\":\"My Notes\",\"filter\":{\"modules\":[\"Note\"]}}]",
            };

            List<UserPreference> preferences = [userPreference];
            Dictionary<string, UserPreferenceModel> expected = preferences.Select(MappingService.MapToUserPreferenceModel).ToDictionary(x => x.Preference, x => x);

            Mock<IUserPreferenceDelegate> userPreferenceDelegateMock = new();
            userPreferenceDelegateMock.Setup(
                    s => s.GetUserPreferencesAsync(It.Is<string>(x => x == Hdid), It.IsAny<CancellationToken>()))
                .ReturnsAsync(preferences);

            IUserPreferenceService service = GetUserPreferenceService(userPreferenceDelegateMock);
            return new(service, expected, Hdid);
        }

        private sealed record UserPreferenceMock(
            RequestResult<UserPreferenceModel> Expected,
            UserPreference UserPreference,
            DbResult<UserPreference> DbResult,
            UserPreferenceModel UserPreferenceModel);

        private sealed record CreateUserPreferenceMock(IUserPreferenceService Service, RequestResult<UserPreferenceModel> Expected, UserPreferenceModel UserPreferenceModel);

        private sealed record UpdateUserPreferenceMock(IUserPreferenceService Service, RequestResult<UserPreferenceModel> Expected, UserPreferenceModel UserPreferenceModel);

        private sealed record GetUserPreferenceMock(IUserPreferenceService Service, Dictionary<string, UserPreferenceModel> Expected, string Hdid);
    }
}
