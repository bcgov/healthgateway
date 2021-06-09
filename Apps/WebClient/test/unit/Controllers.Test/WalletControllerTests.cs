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
    using System.Linq;
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
    public class WalletControllerTests
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

            Mock<IWalletService> walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(s => s.CreateConnectionAsync(Hdid)).ReturnsAsync(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                walletServiceMock.Object);
            var actualResult = await controller.CreateConnection(Hdid).ConfigureAwait(true);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)actualResult.Value;

            Assert.True(actualRequestResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Successfully Create Credential - Happy Path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateCredential()
        {
            IEnumerable<string> targetIds = new List<string>() { "1234" };

            IEnumerable<WalletCredentialModel> walletCredentialModel = new WalletCredentialModel[]
            {
                new WalletCredentialModel()
                {
                    WalletConnectionId = Guid.NewGuid(),
                    SourceId = targetIds.First(),
                },
            };

            RequestResult<IEnumerable<WalletCredentialModel>> expectedResult = new RequestResult<IEnumerable<WalletCredentialModel>>()
            {
                ResourcePayload = walletCredentialModel,
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(s => s.CreateCredentialsAsync(Hdid, targetIds)).ReturnsAsync(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                walletServiceMock.Object);
            var actualResult = await controller.CreateCredentials(Hdid, targetIds).ConfigureAwait(true);
            RequestResult<IEnumerable<WalletCredentialModel>> actualRequestResult =
                (RequestResult<IEnumerable<WalletCredentialModel>>)actualResult.Value;

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

            Mock<IWalletService> walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(s => s.GetConnection(Hdid)).Returns(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                walletServiceMock.Object);
            var actualResult = controller.GetConnection(Hdid);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.Hdid);
        }

        /// <summary>
        /// Successfully Disconnect Connection - Happy path scenario.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldDisconnectConnection()
        {
            Guid connectionId = Guid.NewGuid();
            RequestResult<WalletConnectionModel> expectedResult = new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = new WalletConnectionModel()
                {
                    WalletConnectionId = connectionId,
                    Hdid = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(s => s.DisconnectConnectionAsync(connectionId, Hdid)).ReturnsAsync(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                walletServiceMock.Object);
            var actualResult = await controller.DisconnectConnection(Hdid, connectionId).ConfigureAwait(true);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.Hdid);
        }

        /// <summary>
        /// Successfully Revoke Credential - Happy path scenario.
        /// </summary>
        /// <returns>>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldRevokeCredential()
        {
            Guid credentialId = Guid.NewGuid();
            RequestResult<WalletCredentialModel> expectedResult = new RequestResult<WalletCredentialModel>()
            {
                ResourcePayload = new WalletCredentialModel()
                {
                    CredentialId = credentialId,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> walletServiceMock = new Mock<IWalletService>();
            walletServiceMock.Setup(s => s.RevokeCredential(credentialId, Hdid)).ReturnsAsync(expectedResult);

            WalletController controller = new WalletController(
                new Mock<ILogger<WalletController>>().Object,
                walletServiceMock.Object);
            var actualResult = await controller.RevokeCredential(Hdid, credentialId).ConfigureAwait(true);
            RequestResult<WalletCredentialModel> actualRequestResult =
                (RequestResult<WalletCredentialModel>)actualResult.Value;
            Assert.True(actualRequestResult.IsDeepEqual(expectedResult));
        }
    }
}
