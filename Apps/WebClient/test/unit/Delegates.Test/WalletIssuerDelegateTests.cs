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
namespace HealthGateway.WebClient.Test.Delegates
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
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.WebClient.Delegates;
    using HealthGateway.WebClient.Models.AcaPy;
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
        /// Create Credential - Happy Path.
        /// </summary>
        [Fact]
        public void ValidateCreateCredential()
        {
            RequestResult<CreateConnectionResponse> expectedRequestResult = new ();

            Tuple<RequestResult<CreateConnectionResponse>, RequestResult<CreateConnectionResponse>> response = this.CreateConnection(HttpStatusCode.OK, expectedRequestResult);
            var actualResult = response.Item1;
            var expectedResult = response.Item2;
            Assert.True(actualResult.IsDeepEqual(expectedResult));
            Assert.True(true);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "NotificationSettings:Endpoint", "https://phsahealthgatewayapi.azurewebsites.net/api/v1/Settings/Notification" },
            };

            return new ConfigurationBuilder()

                // .SetBasePath(outputPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static Mock<IHttpClientService> GetHttpClientServiceMock(HttpResponseMessage httpResponseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            return mockHttpClientService;
        }

        /// <summary>
        /// Create Wallet Connection.
        /// </summary>
        /// <param name="expectedResponseStatusCode">expectedResponseStatusCode.</param>
        /// <param name="expectedRequestResult">expectedRequestResult.</param>
        /// <param name="throwException">Throw exception indicator.</param>
        /// <returns>The notification settings.</returns>
        private Tuple<RequestResult<CreateConnectionResponse>, RequestResult<CreateConnectionResponse>> CreateConnection(
            HttpStatusCode expectedResponseStatusCode,
            RequestResult<CreateConnectionResponse> expectedRequestResult,
            bool throwException = false)
        {
            string json = @"{}";
            Guid guid = Guid.Parse("6b0ed0250bf946a1bca33744e9f3acf1");

            List<KeyValuePair<string?, string?>> values = new ();
            FormUrlEncodedContent httpContent = new (values);

            expectedRequestResult.ResourcePayload = JsonSerializer.Deserialize<CreateConnectionResponse>(json, this.jsonOptions);
            using HttpResponseMessage httpResponseMessage = new ()
            {
                StatusCode = expectedResponseStatusCode,
                Content = throwException ? null : httpContent,
            };
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Mock<IHttpClientService> mockHttpClientService = GetHttpClientServiceMock(httpResponseMessage);
            IWalletIssuerDelegate issuerDelegate = new WalletIssuerDelegate(loggerFactory.CreateLogger<WalletIssuerDelegate>(), mockHttpClientService.Object, this.configuration);
            RequestResult<CreateConnectionResponse> actualResult = Task.Run(async () => await issuerDelegate.CreateConnectionAsync(guid.ToString()).ConfigureAwait(true)).Result;
            return new Tuple<RequestResult<CreateConnectionResponse>, RequestResult<CreateConnectionResponse>>(actualResult, expectedRequestResult);
        }
    }
}
