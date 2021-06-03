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
namespace HealthGateway.WebClient.Test.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// VerifiableCredentialController's Unit Tests.
    /// </summary>
    public class VerifiableCredentialControllerTests
    {
        private const string Hdid = "mockedHdId";

        /// <summary>
        /// Successfully Create Connection - Happy Path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateConnection()
        {
            RequestResult<WalletConnectionModel> expectedResult = new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = new WalletConnectionModel()
                {
                    WalletConnectionId = Guid.NewGuid(),
                    Hdid = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> verifiableCredentialServiceMock = new Mock<IWalletService>();
            verifiableCredentialServiceMock.Setup(s => s.CreateConnectionAsync(Hdid)).ReturnsAsync(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                verifiableCredentialServiceMock.Object);
            var actualResult = await controller.CreateConnection(Hdid).ConfigureAwait(true);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)actualResult.Value;

            Assert.True(actualRequestResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Successfully Get Connection - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetConnection()
        {
            RequestResult<WalletConnectionModel> expectedResult = new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = new WalletConnectionModel()
                {
                    WalletConnectionId = Guid.NewGuid(),
                    Hdid = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> verifiableCredentialServiceMock = new Mock<IWalletService>();
            verifiableCredentialServiceMock.Setup(s => s.GetConnection(Hdid)).Returns(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                verifiableCredentialServiceMock.Object);
            var actualResult = controller.GetConnection(Hdid);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.Hdid);
        }
    }
}
