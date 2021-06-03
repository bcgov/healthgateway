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
namespace HealthGateway.WebClient.Test.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Delegates;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// CommentService's Unit Tests.
    /// </summary>
    public class VerifiableCredentialServiceTests
    {
        /// <summary>
        /// CreateConnection - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateConnection()
        {
            // ****** Data Setup
            string phn = "fakePHN";
            string hdid = "fakeHDID";
            string[] targetIds = new string[] { "12345", "54321" };
            Guid walletConnectionId = Guid.NewGuid();
            Guid exchangeId = Guid.NewGuid();
            RequestResult<WalletConnectionModel> expectedResult = new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = new WalletConnectionModel()
                {
                    WalletConnectionId = walletConnectionId,
                    Hdid = hdid,
                },
                ResultStatus = ResultType.Success,
            };

            WalletConnection walletConnection = new WalletConnection()
            {
                Id = walletConnectionId,
            };

            RequestResult<ImmunizationEvent> requestResultImmunization = new RequestResult<ImmunizationEvent>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new ImmunizationEvent()
                {
                    ProviderOrClinic = "Test Clinic",
                },
            };

            RequestResult<PatientModel> requestResultPatient = new RequestResult<PatientModel>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new PatientModel()
                {
                    PersonalHealthNumber = phn,
                },
            };

            RequestResult<CredentialResponse> requestResultCredentialResponse = new RequestResult<CredentialResponse>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new CredentialResponse()
                {
                    ExchangeId = exchangeId,
                },
            };
            RequestResult<ConnectionResponse> requestResultConnectionResponse = new RequestResult<ConnectionResponse>()
            {
                ResultStatus = ResultType.Success,
                ResourcePayload = new ConnectionResponse()
                {
                    AgentId = Guid.NewGuid(),
                    InvitationUrl = new Uri("https://www.fakeuri.com"),
                },
            };

            DBResult<WalletConnection> insertDbResultWalletConnection = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Created,
                Payload = walletConnection,
            };

            DBResult<WalletConnection> updateDbResultWalletConnection = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Updated,
                Payload = walletConnection,
            };

            DBResult<WalletConnection> getDbResultWalletConnection = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Read,
                Payload = walletConnection,
            };

            DBResult<WalletCredential> insertDbResultWalletCredential = new DBResult<WalletCredential>()
            {
                Status = DBStatusCode.Created,
                Payload = new WalletCredential()
                {
                    Id = Guid.NewGuid(),
                    WalletConnection = walletConnection,
                },
            };

            // ****** Mock Setup
            Mock<IImmunizationDelegate> immunizationDelegateMock = new Mock<IImmunizationDelegate>();
            immunizationDelegateMock
                .Setup(s => s.GetImmunization(hdid, It.IsIn(targetIds)))
                .ReturnsAsync(requestResultImmunization);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegateMock = new Mock<IClientRegistriesDelegate>();
            clientRegistriesDelegateMock.Setup(s => s.GetDemographicsByHDIDAsync(hdid)).ReturnsAsync(requestResultPatient);

            Mock<IWalletIssuerDelegate> walletIssuerDelegateMock = new Mock<IWalletIssuerDelegate>();
            walletIssuerDelegateMock
                .Setup(s =>
                    s.CreateConnectionAsync(walletConnectionId))
                .ReturnsAsync(requestResultConnectionResponse);
            walletIssuerDelegateMock
                .Setup(s =>
                    s.CreateCredentialAsync<ImmunizationCredentialPayload>(
                        walletConnection,
                        It.Is<ImmunizationCredentialPayload>(ic => ic.RecipientPHN == phn),
                        It.IsAny<string>()))
                .ReturnsAsync(requestResultCredentialResponse);

            Mock<IWalletDelegate> walletDelegateMock = new Mock<IWalletDelegate>();
            walletDelegateMock.Setup(s => s
                .InsertConnection(
                    It.Is<WalletConnection>(c =>
                        c.Status == WalletConnectionStatus.Pending &&
                        c.UserProfileId == hdid), true))
                .Returns(insertDbResultWalletConnection);
            walletDelegateMock.Setup(s =>
                s.UpdateConnection(
                    It.Is<WalletConnection>(c =>
                        c.AgentId == requestResultConnectionResponse.ResourcePayload.AgentId &&
                        c.Id == walletConnectionId), true))
                .Returns(updateDbResultWalletConnection);
            walletDelegateMock
                .Setup(s => s.GetCurrentConnection(hdid))
                .Returns(getDbResultWalletConnection);
            walletDelegateMock
                .Setup(s =>
                    s.InsertCredential(
                        It.Is<WalletCredential>(wc =>
                            wc.ExchangeId == exchangeId && wc.WalletConnectionId == walletConnectionId), true))
                .Returns(insertDbResultWalletCredential);

            // ******* Assertion
            WalletService service = new WalletService(
                new Mock<ILogger<WalletService>>().Object,
                walletDelegateMock.Object,
                walletIssuerDelegateMock.Object,
                clientRegistriesDelegateMock.Object,
                immunizationDelegateMock.Object);
            var actualResult = await service.CreateConnectionAsync(hdid, targetIds).ConfigureAwait(true);

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload!.Credentials.Count() == 2);
        }
    }
}
