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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Controllers;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;
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
        [Fact]
        public void ShouldCreateConnection()
        {
            string[] targetIds = new string[] { "12345" };
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
            verifiableCredentialServiceMock.Setup(s => s.CreateConnection(Hdid, targetIds)).Returns(expectedResult);

            WalletController controller = new WalletController(verifiableCredentialServiceMock.Object);
            var actualResult = controller.CreateConnection(Hdid, targetIds);

            Assert.True(((JsonResult)actualResult).Value.IsDeepEqual(expectedResult));
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

            WalletController controller = new WalletController(verifiableCredentialServiceMock.Object);
            var actualResult = controller.GetConnection(Hdid);
            RequestResult<WalletConnectionModel> actualRequestResult =
                (RequestResult<WalletConnectionModel>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.Hdid);
        }

        /// <summary>
        /// Successfully Get Credential - Happy path scenario.
        /// </summary>
        [Fact]
        public void ShouldGetCredential()
        {
            RequestResult<WalletCredentialModel> expectedResult = new RequestResult<WalletCredentialModel>()
            {
                ResourcePayload = new WalletCredentialModel()
                {
                    CredentialId = Guid.NewGuid(),
                    Hdid = Hdid,
                },
                ResultStatus = Common.Constants.ResultType.Success,
            };

            Mock<IWalletService> verifiableCredentialServiceMock = new Mock<IWalletService>();
            verifiableCredentialServiceMock.Setup(s => s.GetCredential(Hdid)).Returns(expectedResult);

            WalletController controller = new WalletController(verifiableCredentialServiceMock.Object);
            var actualResult = controller.GetCredential(Hdid);
            RequestResult<WalletCredentialModel> actualRequestResult =
                (RequestResult<WalletCredentialModel>)((JsonResult)actualResult).Value;
            Assert.Equal(Common.Constants.ResultType.Success, actualRequestResult.ResultStatus);
            Assert.Equal(Hdid, actualRequestResult.ResourcePayload!.Hdid);
        }
    }
}
