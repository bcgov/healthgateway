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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.AcaPy;
    using HealthGateway.Common.Services;
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
        private readonly JsonSerializerOptions jsonOptions = new ()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true,
            WriteIndented = true,
        };

        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletIssuerDelegateTests"/> class.
        /// </summary>
        public WalletIssuerDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// Create Connection - Happy Path.
        /// </summary>
        [Fact]
        public void CreateConnection200()
        {
            RequestResult<ConnectionResponse> expectedRequestResult = new ()
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
            RequestResult<ConnectionResponse> expectedRequestResult = new ();

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
            RequestResult<ConnectionResponse> expectedRequestResult = new ();

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
            RequestResult<ConnectionResponse> expectedRequestResult = new ();

            Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>> response = this.CreateConnection(HttpStatusCode.OK, expectedRequestResult, false, true);
            var actualResult = response.Item1;
            Assert.True(actualResult.ResultStatus == Common.Constants.ResultType.Error);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "AcaPy:agentApiUrl", "https://health-gateway-agent-admin-dev.apps.silver.devops.gov.bc.ca/" },
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

            expectedRequestResult.ResourcePayload = JsonSerializer.Deserialize<ConnectionResponse>(json, this.jsonOptions);
            using HttpResponseMessage httpResponseMessage = new ()
            {
                StatusCode = expectedResponseStatusCode,
                Content = badContent ? null : new StringContent(json),
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage, throwException);
            IWalletIssuerDelegate issuerDelegate = new WalletIssuerDelegate(loggerFactory.CreateLogger<WalletIssuerDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<ConnectionResponse> actualResult = Task.Run(async () => await issuerDelegate.CreateConnectionAsync(guid).ConfigureAwait(true)).Result;
            return new Tuple<RequestResult<ConnectionResponse>, RequestResult<ConnectionResponse>>(actualResult, expectedRequestResult);
        }
    }
}
