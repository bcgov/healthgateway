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
    public class WalletServiceTests
    {
        private const string HdId = "fakeHDID";

        /// <summary>
        /// CreateConnection - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldCreateConnection()
        {
            // ****** Data Setup
            string phn = "fakePHN";
            string[] targetIds = new string[] { "12345", "54321" };
            Guid walletConnectionId = Guid.NewGuid();
            Guid exchangeId = Guid.NewGuid();
            RequestResult<WalletConnectionModel> expectedResult = new RequestResult<WalletConnectionModel>()
            {
                ResourcePayload = new WalletConnectionModel()
                {
                    WalletConnectionId = walletConnectionId,
                    Hdid = HdId,
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
                .Setup(s => s.GetImmunization(HdId, It.IsIn(targetIds)))
                .ReturnsAsync(requestResultImmunization);

            Mock<IClientRegistriesDelegate> clientRegistriesDelegateMock = new Mock<IClientRegistriesDelegate>();
            clientRegistriesDelegateMock.Setup(s => s.GetDemographicsByHDIDAsync(HdId, false)).ReturnsAsync(requestResultPatient);

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
                        c.UserProfileId == HdId), true))
                .Returns(insertDbResultWalletConnection);
            walletDelegateMock.Setup(s =>
                s.UpdateConnection(
                    It.Is<WalletConnection>(c =>
                        c.AgentId == requestResultConnectionResponse.ResourcePayload.AgentId &&
                        c.Id == walletConnectionId), true))
                .Returns(updateDbResultWalletConnection);
            walletDelegateMock
                .Setup(s => s.GetCurrentConnection(HdId))
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
            var actualResult = await service.CreateConnectionAsync(HdId).ConfigureAwait(true);

            Assert.Equal(expectedResult.ResultStatus, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload!.Credentials.Count == 0);
        }

        /// <summary>
        /// GetConnection - Happy Path.
        /// </summary>
        [Fact]
        public void GetConnection()
        {
            DBResult<WalletConnection> getDbResultWalletConnection = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Read,
                Payload = new WalletConnection()
                {
                    UserProfileId = HdId,
                },
            };

            // ****** Mock Setup
            Mock<IWalletDelegate> walletDelegateMock = new Mock<IWalletDelegate>();
            walletDelegateMock.Setup(s => s
                .GetCurrentConnection(HdId))
                .Returns(getDbResultWalletConnection);

            // ******* Assertion
            WalletService service = new WalletService(
                new Mock<ILogger<WalletService>>().Object,
                walletDelegateMock.Object,
                new Mock<IWalletIssuerDelegate>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<IImmunizationDelegate>().Object);
            var actualResult = service.GetConnection(HdId);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(HdId, actualResult.ResourcePayload!.Hdid);
        }

        /// <summary>
        /// GetCredentialByExchangeId - Happy Path.
        /// </summary>
        [Fact]
        public void GetCredentialByExchangeId()
        {
            Guid exchangeId = Guid.NewGuid();
            Guid credentialId = Guid.NewGuid();
            DBResult<WalletCredential> getDbResultWalletCredential = new DBResult<WalletCredential>()
            {
                Status = DBStatusCode.Read,
                Payload = new WalletCredential()
                {
                    ExchangeId = exchangeId,
                    Id = credentialId,
                    WalletConnection = new WalletConnection(),
                },
            };

            // ****** Mock Setup
            Mock<IWalletDelegate> walletDelegateMock = new Mock<IWalletDelegate>();
            walletDelegateMock.Setup(s => s
                .GetCredentialByExchangeId(exchangeId))
                .Returns(getDbResultWalletCredential);

            // ******* Assertion
            WalletService service = new WalletService(
                new Mock<ILogger<WalletService>>().Object,
                walletDelegateMock.Object,
                new Mock<IWalletIssuerDelegate>().Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<IImmunizationDelegate>().Object);
            var actualResult = service.GetCredentialByExchangeId(exchangeId);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(credentialId, actualResult.ResourcePayload!.CredentialId);
        }

        /// <summary>
        /// RevokeCredential - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RevokeCredential()
        {
            Guid credentialId = Guid.NewGuid();
            DBResult<WalletCredential> getDbResultWalletCredential = new DBResult<WalletCredential>()
            {
                Status = DBStatusCode.Read,
                Payload = new WalletCredential()
                {
                    Id = credentialId,
                    Status = WalletCredentialStatus.Added,
                },
            };

            DBResult<WalletCredential> updateDbResultWalletCredential = new DBResult<WalletCredential>()
            {
                Status = DBStatusCode.Updated,
                Payload = new WalletCredential()
                {
                    Id = credentialId,
                    Status = WalletCredentialStatus.Revoked,
                    RevokedDateTime = DateTime.UtcNow,
                    WalletConnection = new WalletConnection(),
                },
            };

            RequestResult<WalletCredential> revokeCredentialResult = new RequestResult<WalletCredential>()
            {
                ResultStatus = ResultType.Success,
            };

            // ****** Mock Setup
            Mock<IWalletDelegate> walletDelegateMock = new Mock<IWalletDelegate>();
            walletDelegateMock.Setup(s => s
                .GetCredentialById(credentialId, HdId))
                .Returns(getDbResultWalletCredential);

            walletDelegateMock.Setup(s => s
                .UpdateCredential(It.Is<WalletCredential>(c => c.Id == credentialId && c.Status == WalletCredentialStatus.Revoked), true))
                .Returns(updateDbResultWalletCredential);

            Mock<IWalletIssuerDelegate> walletIssuerDelegateMock = new Mock<IWalletIssuerDelegate>();
            walletIssuerDelegateMock.Setup(s => s
                .RevokeCredentialAsync(getDbResultWalletCredential.Payload, It.IsAny<string>()))
                .ReturnsAsync(revokeCredentialResult);

            // ******* Assertion
            WalletService service = new WalletService(
                new Mock<ILogger<WalletService>>().Object,
                walletDelegateMock.Object,
                walletIssuerDelegateMock.Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<IImmunizationDelegate>().Object);
            var actualResult = await service.RevokeCredential(credentialId, HdId).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(WalletCredentialStatus.Revoked, actualResult.ResourcePayload!.Status);
            Assert.NotNull(actualResult.ResourcePayload!.RevokedDate);
        }

        /// <summary>
        /// Disconnect Connection - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DisconnectConnection()
        {
            Guid connectionId = Guid.NewGuid();

            WalletCredential credential = new WalletCredential()
            {
                WalletConnectionId = connectionId,
                Status = WalletCredentialStatus.Added,
            };

            WalletConnection connection = new WalletConnection()
            {
                Id = connectionId,
                Status = WalletConnectionStatus.Connected,
                Credentials = new WalletCredential[] { credential },
            };

            DBResult<WalletConnection> getConnectionDbResult = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Read,
                Payload = connection,
            };

            DBResult<WalletCredential> updateDbResultWalletCredential = new DBResult<WalletCredential>()
            {
                Status = DBStatusCode.Updated,
                Payload = new WalletCredential()
                {
                    Status = WalletCredentialStatus.Revoked,
                    RevokedDateTime = DateTime.UtcNow,
                    WalletConnection = new WalletConnection(),
                },
            };

            DBResult<WalletConnection> updateDbResultWalletConnection = new DBResult<WalletConnection>()
            {
                Status = DBStatusCode.Updated,
                Payload = new WalletConnection()
                {
                    Id = connectionId,
                    Status = WalletConnectionStatus.Disconnected,
                    DisconnectedDateTime = DateTime.UtcNow,
                },
            };

            RequestResult<WalletCredential> revokeCredentialResult = new RequestResult<WalletCredential>()
            {
                ResultStatus = ResultType.Success,
            };

            RequestResult<WalletConnection> disconnectResult = new RequestResult<WalletConnection>()
            {
                ResultStatus = ResultType.Success,
            };

            // ****** Mock Setup
            Mock<IWalletDelegate> walletDelegateMock = new Mock<IWalletDelegate>();
            walletDelegateMock.Setup(s => s
                .GetConnection(connectionId, HdId, false))
                .Returns(getConnectionDbResult);

            walletDelegateMock.Setup(s => s
                .UpdateCredential(credential, true))
                .Returns(updateDbResultWalletCredential);

            walletDelegateMock.Setup(s => s
                .UpdateConnection(It.Is<WalletConnection>(c => c.Id == connectionId && c.Status == WalletConnectionStatus.Disconnected), true))
                .Returns(updateDbResultWalletConnection);

            Mock<IWalletIssuerDelegate> walletIssuerDelegateMock = new Mock<IWalletIssuerDelegate>();
            walletIssuerDelegateMock.Setup(s => s
                .RevokeCredentialAsync(credential, It.IsAny<string>()))
                .ReturnsAsync(revokeCredentialResult);

            walletIssuerDelegateMock.Setup(s => s
                .DisconnectConnectionAsync(connection))
                .ReturnsAsync(disconnectResult);

            // ******* Assertion
            WalletService service = new WalletService(
                new Mock<ILogger<WalletService>>().Object,
                walletDelegateMock.Object,
                walletIssuerDelegateMock.Object,
                new Mock<IClientRegistriesDelegate>().Object,
                new Mock<IImmunizationDelegate>().Object);
            var actualResult = await service.DisconnectConnectionAsync(connectionId, HdId).ConfigureAwait(true);

            Assert.Equal(ResultType.Success, actualResult.ResultStatus);
            Assert.Equal(WalletConnectionStatus.Disconnected, actualResult.ResourcePayload!.Status);
            Assert.NotNull(actualResult.ResourcePayload!.DisconnectedDate);
        }
    }
}
