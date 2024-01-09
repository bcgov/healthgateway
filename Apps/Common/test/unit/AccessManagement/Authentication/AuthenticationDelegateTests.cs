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
namespace HealthGateway.CommonTests.AccessManagement.Authentication
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.CacheProviders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// AuthenticationDelegate's Unit Tests.
    /// </summary>
    public class AuthenticationDelegateTests
    {
        private const string UserJson =
            """
            {
                "access_token": "token",
                "expires_in": 500,
                "refresh_expires_in": 0,
                "refresh_token": "refresh_token",
                "token_type": "bearer",
                "not-before-policy": 25,
                "session_state": "session_state",
                "scope": "scope"
            }
            """;

        private static readonly ClientCredentialsRequest UserClientCredentialsRequest = new()
        {
            TokenUri = new("http://testsite"),
            Parameters = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
                Username = "A_USERNAME",
                Password = "SOME_PASSWORD",
            },
        };

        private static readonly ClientCredentialsRequest SystemClientCredentialsRequest = new()
        {
            TokenUri = new("http://testsite"),
            Parameters = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
            },
        };

        /// <summary>
        /// AuthenticateAsUser - Happy Path (disabled cache).
        /// </summary>
        [Fact]
        public void ShouldAuthenticateAsUserDisabledCache()
        {
            JwtModel? expected = JsonSerializer.Deserialize<JwtModel>(UserJson);
            Mock<ICacheProvider> mockCacheProvider = CreateCacheProvider();
            using HttpResponseMessage httpResponseMessage = new();
            IAuthenticationDelegate authDelegate = CreateAuthenticationDelegate(CreateHttpClientFactory(UserJson, httpResponseMessage), mockCacheProvider);

            JwtModel actualModel = authDelegate.AuthenticateUser(UserClientCredentialsRequest);

            expected.ShouldDeepEqual(actualModel);
            mockCacheProvider.Verify(v => v.GetItem<JwtModel>(It.IsAny<string>()), Times.Never());
            mockCacheProvider.Verify(v => v.AddItem(It.IsAny<string>(), It.IsAny<JwtModel>(), It.IsAny<TimeSpan?>()), Times.Never());
        }

        /// <summary>
        /// AuthenticateAsUser - Happy Path (empty cache).
        /// </summary>
        [Fact]
        public void ShouldAuthenticateAsUserEmptyCache()
        {
            JwtModel? expected = JsonSerializer.Deserialize<JwtModel>(UserJson);
            Mock<ICacheProvider> mockCacheProvider = CreateCacheProvider();
            using HttpResponseMessage httpResponseMessage = new();
            IAuthenticationDelegate authDelegate = CreateAuthenticationDelegate(CreateHttpClientFactory(UserJson, httpResponseMessage), mockCacheProvider);

            JwtModel actualModel = authDelegate.AuthenticateUser(UserClientCredentialsRequest, true);

            expected.ShouldDeepEqual(actualModel);
            mockCacheProvider.Verify(v => v.GetItem<JwtModel>(It.IsAny<string>()), Times.Exactly(1));
            mockCacheProvider.Verify(v => v.AddItem(It.IsAny<string>(), It.IsAny<JwtModel>(), It.IsAny<TimeSpan?>()), Times.Exactly(1));
        }

        /// <summary>
        /// AuthenticateAsUser - Happy Path (populated cache).
        /// </summary>
        [Fact]
        public void ShouldAuthenticateAsUserPopulatedCache()
        {
            JwtModel? expected = JsonSerializer.Deserialize<JwtModel>(UserJson);
            Mock<ICacheProvider> mockCacheProvider = CreateCacheProvider(expected);
            using HttpResponseMessage httpResponseMessage = new();
            IAuthenticationDelegate authDelegate = CreateAuthenticationDelegate(CreateHttpClientFactory(UserJson, httpResponseMessage), mockCacheProvider);

            JwtModel actualModel = authDelegate.AuthenticateUser(UserClientCredentialsRequest, true);

            expected.ShouldDeepEqual(actualModel);
            mockCacheProvider.Verify(v => v.GetItem<JwtModel>(It.IsAny<string>()), Times.Exactly(1));
            mockCacheProvider.Verify(v => v.AddItem(It.IsAny<string>(), It.IsAny<JwtModel>(), It.IsAny<TimeSpan?>()), Times.Never());
        }

        /// <summary>
        /// AuthenticateAsUser - Unhappy Path.
        /// </summary>
        [Fact]
        public void ShouldNotAuthenticateAsUser()
        {
            const string json = "Bad JSON";
            using HttpResponseMessage httpResponseMessage = new();
            IAuthenticationDelegate authDelegate = CreateAuthenticationDelegate(CreateHttpClientFactory(json, httpResponseMessage), CreateCacheProvider());

            Assert.Throws<InvalidOperationException>(() => authDelegate.AuthenticateUser(UserClientCredentialsRequest));
        }

        /// <summary>
        /// AuthenticateAsSystem - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAuthenticateAsSystem()
        {
            const string json =
                """
                {
                    "access_token": "token",
                    "expires_in": 500,
                    "refresh_expires_in": 0,
                    "refresh_token": "refresh_token",
                    "token_type": "bearer",
                    "not-before-policy": 25,
                    "session_state": "session_state",
                    "scope": "scope"
                }
                """;
            JwtModel? expected = JsonSerializer.Deserialize<JwtModel>(json);
            using HttpResponseMessage httpResponseMessage = new();
            IAuthenticationDelegate authDelegate = CreateAuthenticationDelegate(CreateHttpClientFactory(json, httpResponseMessage), CreateCacheProvider(expected));

            JwtModel actualModel = authDelegate.AuthenticateAsSystem(SystemClientCredentialsRequest);

            expected.ShouldDeepEqual(actualModel);
        }

        private static IAuthenticationDelegate CreateAuthenticationDelegate(IMock<IHttpClientFactory> mockHttpClientFactory, IMock<ICacheProvider> mockCacheProvider)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<AuthenticationDelegate> logger = loggerFactory.CreateLogger<AuthenticationDelegate>();

            IConfiguration configuration = CreateConfiguration();

            return new AuthenticationDelegate(logger, mockHttpClientFactory.Object, configuration, mockCacheProvider.Object, null);
        }

        private static Mock<IHttpClientFactory> CreateHttpClientFactory(string jsonResponse, HttpResponseMessage httpResponseMessage)
        {
            Mock<HttpMessageHandler> handlerMock = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(jsonResponse);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage)
                .Verifiable();
            Mock<IHttpClientFactory> mockHttpClientFactory = new();
            mockHttpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handlerMock.Object));
            return mockHttpClientFactory;
        }

        private static Mock<ICacheProvider> CreateCacheProvider(JwtModel? cachedValue = null)
        {
            Mock<ICacheProvider> mockCacheProvider = new();
            mockCacheProvider.Setup(s => s.GetItem<JwtModel>(It.IsAny<string>())).Returns(cachedValue);
            return mockCacheProvider;
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection([new("AuthCache:TokenCacheExpireMinutes", "20")])
                .Build();
        }
    }
}
