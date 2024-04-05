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
    public class GatewayCommunicationServiceTests
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
            GetActiveCommunicationMock mock = SetupGetActiveCommunicationMock(sourceCommunicationType, communicationType, sourceCommunicationStatus, communicationStatus);

            // Act
            RequestResult<CommunicationModel?> actual = await mock.Service.GetActiveCommunicationAsync(mock.CommunicationType);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        /// <summary>
        /// Should GetActiveCommunication returns error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetActiveCommunicationReturnsError()
        {
            // Arrange
            GetActiveCommunicationReturnsErrorMock mock = SetupGetActiveCommunicationReturnsErrorMock();

            // Act
            RequestResult<CommunicationModel?> actual = await mock.Service.GetActiveCommunicationAsync(mock.CommunicationType);

            // Assert
            mock.Expected.ShouldDeepEqual(actual);
        }

        private static IGatewayCommunicationService GetGatewayCommunicationService(IMock<ICommunicationService> communicationServiceMock)
        {
            return new GatewayCommunicationService(communicationServiceMock.Object, MappingService);
        }

        private static GetActiveCommunicationMock SetupGetActiveCommunicationMock(
            Common.Data.Constants.CommunicationType sourceCommunicationType,
            CommunicationType communicationType,
            Common.Data.Constants.CommunicationStatus sourceCommunicationStatus,
            CommunicationStatus communicationStatus)
        {
            Guid id = Guid.NewGuid();
            const string text = "Communication";

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

            RequestResult<CommunicationModel?> expected = new()
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


            Mock<ICommunicationService> communicationServiceMock = new();
            communicationServiceMock.Setup(s => s.GetActiveCommunicationAsync(It.Is<Common.Data.Constants.CommunicationType>(x => x == sourceCommunicationType), It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            IGatewayCommunicationService service = GetGatewayCommunicationService(communicationServiceMock);

            return new(service, expected, communicationType);
        }

        private static GetActiveCommunicationReturnsErrorMock SetupGetActiveCommunicationReturnsErrorMock()
        {
            const string error = "DB Error";

            RequestResult<Communication?> requestResult = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = error,
                },
            };

            RequestResult<CommunicationModel?> expected = new()
            {
                ResultStatus = ResultType.Error,
                ResultError = new()
                {
                    ResultMessage = error,
                },
            };


            Mock<ICommunicationService> communicationServiceMock = new();
            communicationServiceMock.Setup(s => s.GetActiveCommunicationAsync(It.IsAny<Common.Data.Constants.CommunicationType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(requestResult);

            IGatewayCommunicationService service = GetGatewayCommunicationService(communicationServiceMock);

            return new(service, expected, CommunicationType.Banner);
        }

        private sealed record GetActiveCommunicationMock(IGatewayCommunicationService Service, RequestResult<CommunicationModel?> Expected, CommunicationType CommunicationType);

        private sealed record GetActiveCommunicationReturnsErrorMock(IGatewayCommunicationService Service, RequestResult<CommunicationModel?> Expected, CommunicationType CommunicationType);
    }
}
