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
namespace HealthGateway.GatewayApiTests.Controllers.Test
{
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.GatewayApi.Controllers;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Services;
    using Moq;
    using Xunit;
    using CommunicationType = HealthGateway.GatewayApi.Constants.CommunicationType;

    /// <summary>
    /// CommunicationController's Unit Tests.
    /// </summary>
    public class CommunicationControllerTests
    {
        /// <summary>
        /// Successfully Create Comment - Happy Path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldGetCommunication()
        {
            RequestResult<CommunicationModel> expectedResult = new()
            {
                ResourcePayload = new()
                {
                    Subject = "Mocked Subject",
                    Text = "Mocked Text",
                },
                ResultStatus = ResultType.Success,
            };

            Mock<IGatewayApiCommunicationService> communicationServiceMock = new();
            communicationServiceMock.Setup(s => s.GetActiveCommunicationAsync(CommunicationType.Banner, It.IsAny<CancellationToken>())).Returns(Task.FromResult(expectedResult));

            CommunicationController controller = new(communicationServiceMock.Object);
            RequestResult<CommunicationModel> actualResult = await controller.Get();

            actualResult.ShouldDeepEqual(expectedResult);
        }
    }
}
