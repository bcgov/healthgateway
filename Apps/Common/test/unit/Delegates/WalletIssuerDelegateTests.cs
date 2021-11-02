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
namespace HealthGateway.CommonTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// Unit Tests for the Wallet Issuer Delegate.
    /// </summary>
    public class WalletIssuerDelegateTests
    {
        private const string AcapyConfigSectionKey = "AcaPy";
        private readonly IConfiguration configuration;
        private readonly WalletIssuerConfiguration walletIssuerConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletIssuerDelegateTests"/> class.
        /// </summary>
        public WalletIssuerDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
            this.walletIssuerConfig = new WalletIssuerConfiguration();
            this.configuration.Bind(AcapyConfigSectionKey, this.walletIssuerConfig);
        }

        /// <summary>
        /// Create Connection - Happy Path.
        /// </summary>
        [Fact]
        public void CreateConnection200()
        {
            RequestResult<ConnectionResponse> expectedRequestResult = new()
            {
                ResultStatus = Common.Constants.ResultType.Success,
                ResourcePayload = new ConnectionResponse()
                {
                    AgentId = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    InvitationUrl = new Uri("https://invite.url/mock"),
                },
                TotalResultCount = 1,
            };

            Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> response = this.CreateConnection(HttpStatusCode.OK, expectedRequestResult);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Create Connection - Agent down.
        /// </summary>
        [Fact]
        public void CreateConnection500()
        {
            RequestResult<ConnectionResponse> expectedRequestResult = new();

            Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> response = this.CreateConnection(HttpStatusCode.ServiceUnavailable, expectedRequestResult);
            var actualResult = response.Item1;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// Create Connection - Bad Payload from Agent.
        /// </summary>
        [Fact]
        public void CreateConnectionBadPayload()
        {
            RequestResult<ConnectionResponse> expectedRequestResult = new();

            Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> response = this.CreateConnection(HttpStatusCode.OK, expectedRequestResult, true);
            var actualResult = response.Item1;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// Create Connection - Exception during HTTP Call.
        /// </summary>
        [Fact]
        public void CreateConnectionException()
        {
            RequestResult<ConnectionResponse> expectedRequestResult = new();

            Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> response = this.CreateConnection(HttpStatusCode.OK, expectedRequestResult, false, true);
            var actualResult = response.Item1;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        /// <summary>
        /// Create Credential - Happy Path.
        /// </summary>
        [Fact]
#pragma warning disable CA1506 // Do not catch general exception types
        public void CreateCredential()
        {
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<HttpMessageHandler> handlerMock = new();

            // Setup IssuerDID
            string issuerDID = "fakeDID";
            IssuerDidResponse didResponseData = new()
            {
                Result = new IssuerDidResult()
                {
                    Did = issuerDID,
                },
            };
            this.MessageHandlerMockSetup(handlerMock, didResponseData, new($"{this.walletIssuerConfig.AgentApiUrl}wallet/did/public"));

            // Setup Schema ID
            string schemaId = "schemaId";
            SchemaIdResponse schemaResponseData = new();
            schemaResponseData.SchemaIds.Add(schemaId);
            this.MessageHandlerMockSetup(handlerMock, schemaResponseData, new($"{this.walletIssuerConfig.AgentApiUrl}schemas/created?schema_version={this.walletIssuerConfig.SchemaVersion}&schema_issuer_did={issuerDID}&schema_name={this.walletIssuerConfig.SchemaName}"));

            // Setup credentialDefinitionIdResponse
            CredentialDefinitionIdResponse credentialDefinitionIdData = new();
            credentialDefinitionIdData.CredentialDefinitionIds.Add("credentialDefinitionId");
            this.MessageHandlerMockSetup(handlerMock, credentialDefinitionIdData, new($"{this.walletIssuerConfig.AgentApiUrl}credential-definitions/created?schema_id={schemaId}"));

            // Setup CredentialResponse
            Guid exchangeId = System.Guid.NewGuid();
            CredentialResponse credentialResponseData = new()
            {
                ExchangeId = exchangeId,
            };
            this.MessageHandlerMockSetup(handlerMock, credentialResponseData, new($"{this.walletIssuerConfig.AgentApiUrl}issue-credential/send"));

            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            WalletConnection connection = new()
            {
                AgentId = Guid.NewGuid(),
            };

            ImmunizationCredentialPayload payload = new ImmunizationCredentialPayload()
            {
                RecipientName = "recipientName",
                ImmunizationAgent = "immunizationAgent",
            };
            string comment = "Immunization Credential";

            RequestResult<CredentialResponse> expectedResult = new()
            {
                ResourcePayload = credentialResponseData,
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };
            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(loggerFactory.CreateLogger<RestWalletIssuerDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<CredentialResponse> actualResult = Task.Run(async () => await issuerDelegate.CreateCredentialAsync<ImmunizationCredentialPayload>(connection, payload, comment).ConfigureAwait(true)).Result;
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Revoke Credential - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task RevokeCredential()
        {
            WalletConnection connection = new()
            {
                AgentId = Guid.NewGuid(),
            };
            WalletCredential credential = new()
            {
                WalletConnection = connection,
                RevocationId = "1234",
                RevocationRegistryId = "4321",
            };

            RequestResult<CredentialResponse> expectedResult = new()
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Success,
            };

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<HttpMessageHandler> handlerMock = new();
            this.MessageHandlerMockSetup(handlerMock, string.Empty, new($"{this.walletIssuerConfig.AgentApiUrl}revocation/revoke"));
            this.MessageHandlerMockSetup(handlerMock, string.Empty, new($"{this.walletIssuerConfig.AgentApiUrl}connections/{connection.AgentId}/send-message"));

            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(
                loggerFactory.CreateLogger<RestWalletIssuerDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<WalletCredential> actualResult = await issuerDelegate.RevokeCredentialAsync(credential, "Test revoke").ConfigureAwait(true);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Disconnect Connection - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task DisconnectConnection()
        {
            WalletConnection connection = new()
            {
                AgentId = Guid.NewGuid(),
            };

            RequestResult<CredentialResponse> expectedResult = new()
            {
                ResourcePayload = null,
                ResultStatus = ResultType.Success,
            };

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<HttpMessageHandler> handlerMock = new();
            this.MessageHandlerMockSetup(handlerMock, string.Empty, new($"{this.walletIssuerConfig.AgentApiUrl}connections/{connection.AgentId}"));

            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(
                loggerFactory.CreateLogger<RestWalletIssuerDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<WalletConnection> actualResult = await issuerDelegate.DisconnectConnectionAsync(connection).ConfigureAwait(true);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Create Schema - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateSchema()
        {
            SchemaRequest schemaRequest = new()
            {
                SchemaName = "Test",
            };

            RequestResult<SchemaResponse> expectedResult = new()
            {
                ResourcePayload = new SchemaResponse() { SchemaId = "123" },
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<HttpMessageHandler> handlerMock = new();
            this.MessageHandlerMockSetup(handlerMock, expectedResult.ResourcePayload, new($"{this.walletIssuerConfig.AgentApiUrl}schemas"));

            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(
                loggerFactory.CreateLogger<RestWalletIssuerDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<SchemaResponse> actualResult = await issuerDelegate.CreateSchemaAsync(schemaRequest).ConfigureAwait(true);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        /// <summary>
        /// Create Credential Definition - Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task CreateCredentialDefinition()
        {
            string schemaId = "123";
            RequestResult<CredentialDefinitionResponse> expectedResult = new()
            {
                ResourcePayload = new CredentialDefinitionResponse() { CredentialDefinitionId = "321" },
                ResultStatus = ResultType.Success,
                TotalResultCount = 1,
            };

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<HttpMessageHandler> handlerMock = new();
            this.MessageHandlerMockSetup(handlerMock, expectedResult.ResourcePayload, new($"{this.walletIssuerConfig.AgentApiUrl}credential-definitions"));

            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(
                loggerFactory.CreateLogger<RestWalletIssuerDelegate>(),
                mockHttpClientService.Object,
                this.configuration);
            RequestResult<CredentialDefinitionResponse> actualResult = await issuerDelegate.CreateCredentialDefinitionAsync(schemaId).ConfigureAwait(true);
            Assert.True(actualResult.IsDeepEqual(expectedResult));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { $"{AcapyConfigSectionKey}:agentApiUrl", "http://localhost:8024/" },
                { $"{AcapyConfigSectionKey}:agentApiKey", "agent-api-key-dev" },
                { $"{AcapyConfigSectionKey}:schemaName", "schemaName" },
                { $"{AcapyConfigSectionKey}:schemaVersion", "schemaVersion" },
            };

            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static Mock<IHttpClientService> GetHttpClientServiceMock(HttpResponseMessage httpResponseMessage, bool throwException)
        {
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            var handlerMock = new Mock<HttpMessageHandler>();
            if (!throwException)
            {
                handlerMock
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>(
                      "SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                   .ReturnsAsync(httpResponseMessage)
                   .Verifiable();
            }
            else
            {
                handlerMock
                   .Protected()
                   .Setup<Task<HttpResponseMessage>>(
                      "SendAsync",
                      ItExpr.IsAny<HttpRequestMessage>(),
                      ItExpr.IsAny<CancellationToken>())
                   .Throws<HttpRequestException>()
                   .Verifiable();
            }

            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientService;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unit Test")]
        private void MessageHandlerMockSetup(Mock<HttpMessageHandler> handlerMock, object responseObject, Uri endpoint, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            _ = this.configuration;

            HttpResponseMessage response = new()
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(responseObject)),
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c => c.RequestUri == endpoint),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response)
                .Verifiable();
        }

        /// <summary>
        /// Create Wallet Connection.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="badContent">Pass no json data as as response.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>The notification settings.</returns>
        private Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> CreateConnection(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<ConnectionResponse> expectedRequestResult,
            bool badContent = false,
            bool throwException = false)
        {
            string json = @"{""connection_id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",""invitation_url"": ""https://invite.url/mock""}";
            Guid guid = Guid.Parse("6b0ed0250bf946a1bca33744e9f3acf1");

            expectedRequestResult.ResourcePayload = JsonSerializer.Deserialize<ConnectionResponse>(json);
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = expectedResponseStatusCode,
                Content = badContent ? null : new StringContent(json),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage, throwException);
            IWalletIssuerDelegate issuerDelegate = new RestWalletIssuerDelegate(loggerFactory.CreateLogger<RestWalletIssuerDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<ConnectionResponse> actualResult = Task.Run(async () => await issuerDelegate.CreateConnectionAsync(guid).ConfigureAwait(true)).Result;
            return new Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>>(actualResult, expectedRequestResult);
        }
    }
}
