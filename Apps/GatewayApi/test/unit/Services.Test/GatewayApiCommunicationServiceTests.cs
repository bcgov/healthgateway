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
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using HealthGateway.GatewayApiTests.Utils;
    using Moq;
    using Xunit;
    using CommunicationStatus = HealthGateway.GatewayApi.Constants.CommunicationStatus;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// GatewayCommunicationService's Unit Tests.
    /// </summary>
    public class GatewayApiCommunicationServiceTests
    {
        private static readonly IGatewayApiMappingService MappingService = new GatewayApiMappingService(MapperUtil.InitializeAutoMapper(), new Mock<ICryptoDelegate>().Object);

        /// <summary>
        /// Should GetActiveCommunication.
        /// </summary>
        /// <param name="sourceCommunicationType">The type of Communication returned from source.</param>
        /// <param name="communicationType">The type of Communication to passed in and returned converted from source.</param>
        /// <param name="sourceCommunicationStatus">The status of Communication returned from source.</param>
        /// <param name="communicationStatus">The status of Communication returned converted from source.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [InlineData(Common.Data.Constants.CommunicationType.Banner, CommunicationType.Banner, Common.Data.Constants.CommunicationStatus.New, CommunicationStatus.New)]
        [InlineData(Common.Data.Constants.CommunicationType.Banner, CommunicationType.Banner, Common.Data.Constants.CommunicationStatus.Error, CommunicationStatus.Error)]
        [InlineData(Common.Data.Constants.CommunicationType.Banner, CommunicationType.Banner, Common.Data.Constants.CommunicationStatus.Processed, CommunicationStatus.Processed)]
        [InlineData(Common.Data.Constants.CommunicationType.InApp, CommunicationType.InApp, Common.Data.Constants.CommunicationStatus.Pending, CommunicationStatus.Pending)]
        [InlineData(Common.Data.Constants.CommunicationType.Mobile, CommunicationType.Mobile, Common.Data.Constants.CommunicationStatus.Processing, CommunicationStatus.Processing)]
        [InlineData(Common.Data.Constants.CommunicationType.Email, CommunicationType.Email, Common.Data.Constants.CommunicationStatus.Draft, CommunicationStatus.Draft)]
        public async Task ShouldGetActiveCommunication(
            Common.Data.Constants.CommunicationType sourceCommunicationType,
            CommunicationType communicationType,
            Common.Data.Constants.CommunicationStatus sourceCommunicationStatus,
            CommunicationStatus communicationStatus)
        {
            // Arrange
            Guid id = Guid.NewGuid();
            const string text = "Communication";

            RequestResult<CommunicationModel> expected = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Text = text,
                    CommunicationTypeCode = communicationType,
                    CommunicationStatusCode = communicationStatus,
                },
            };

            IGatewayApiCommunicationService service = SetupGetActiveCommunicationMock(id, text, sourceCommunicationType, sourceCommunicationStatus);

            // Act
            RequestResult<CommunicationModel> actual = await service.GetActiveCommunicationAsync(communicationType);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        /// <summary>
        /// Should GetActiveCommunication returns error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetActiveCommunicationReturnsError()
        {
            // Arrange
            const string errorMessage = "DB Error";

            RequestResult<CommunicationModel> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = errorMessage,
                },
            };
            IGatewayApiCommunicationService service = SetupGatewayApiCommunicationServiceForGetActiveCommunicationReturnsError(errorMessage);

            // Act
            RequestResult<CommunicationModel> actual = await service.GetActiveCommunicationAsync(CommunicationType.Banner);

            // Assert
            actual.ShouldDeepEqual(expected);
        }

        private static IGatewayApiCommunicationService GetGatewayApiCommunicationService(Mock<ICommunicationService> communicationServiceMock)
        {
            return new GatewayApiCommunicationService(communicationServiceMock.Object, MappingService);
        }

        private static IGatewayApiCommunicationService SetupGetActiveCommunicationMock(
            Guid id,
            string text,
            Common.Data.Constants.CommunicationType sourceCommunicationType,
            Common.Data.Constants.CommunicationStatus sourceCommunicationStatus)
        {
            RequestResult<Communication?> requestResult = new()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new()
                {
                    Id = id,
                    Text = text,
                    CommunicationTypeCode = sourceCommunicationType,
                    CommunicationStatusCode = sourceCommunicationStatus,
                },
            };

            Mock<ICommunicationService> communicationServiceMock = new();
            communicationServiceMock.Setup(s => s.GetActiveCommunicationAsync(It.Is<Common.Data.Constants.CommunicationType>(x => x == sourceCommunicationType), It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            return GetGatewayApiCommunicationService(communicationServiceMock);
        }

        private static IGatewayApiCommunicationService SetupGatewayApiCommunicationServiceForGetActiveCommunicationReturnsError(string errorMessage)
        {
            RequestResult<Communication?> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = errorMessage,
                },
            };

            Mock<ICommunicationService> communicationServiceMock = new();
            communicationServiceMock.Setup(s => s.GetActiveCommunicationAsync(It.IsAny<Common.Data.Constants.CommunicationType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            return GetGatewayApiCommunicationService(communicationServiceMock);
        }
    }
}
