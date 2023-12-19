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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.CacheProviders;
    using HealthGateway.Common.Data.Constants;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using Moq.Protected;
    using Xunit;

    /// <summary>
    /// AuthenticationDelegate's Unit Tests.
    /// </summary>
    public class AuthenticationDelegateTests
    {
        private readonly DistributedCacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDelegateTests"/> class.
        /// </summary>
        public AuthenticationDelegateTests()
        {
            MemoryDistributedCache cache = new(Options.Create(new MemoryDistributedCacheOptions()));
            this.cacheProvider = new DistributedCacheProvider(cache);
        }

        /// <summary>
        /// AuthenticateAsUser - Happy Path.
        /// </summary>
        [Fact]
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldAuthenticateAsUser()
        {
            Uri tokenUri = new("http://testsite");
            Dictionary<string, string?> configurationParams = new()
            {
                { "ClientAuthentication:TokenUri", tokenUri.ToString() },
                { "AuthCache:TokenCacheExpireMinutes", "20" },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
                Username = "A_USERNAME",
                Password = "SOME_PASSWORD",
            };

            string json = """
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
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<AuthenticationDelegate> logger = loggerFactory.CreateLogger<AuthenticationDelegate>();
            Mock<HttpMessageHandler> handlerMock = new();
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(json);
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
            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, mockHttpClientFactory.Object, configuration, this.cacheProvider, null);
            JwtModel actualModel = authDelegate.AuthenticateAsUser(tokenUri, tokenRequest);
            expected.ShouldDeepEqual(actualModel);

            httpResponseMessage.Content = new StringContent(json);
            (_, bool cached) = authDelegate.AuthenticateUser(tokenUri, tokenRequest, true);
            Assert.True(!cached);

            httpResponseMessage.Content = new StringContent(json);
            (_, cached) = authDelegate.AuthenticateUser(tokenUri, tokenRequest, true);
            Assert.True(cached);
        }

        /// <summary>
        /// AuthenticateAsUser - Happy Path.
        /// </summary>
        [Fact]
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldNotAuthenticatedAsUser()
        {
            Uri tokenUri = new("http://testsite");
            Dictionary<string, string?> configurationParams = new()
            {
                { "ClientAuthentication:TokenUri", tokenUri.ToString() },
                { "AuthCache:TokenCacheExpireMinutes", "20" },
            };
            IConfiguration configuration = CreateConfiguration(configurationParams);

            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
                Username = "A_USERNAME",
                Password = "SOME_PASSWORD",
            };

            string json = "Bad JSON";
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<AuthenticationDelegate> logger = loggerFactory.CreateLogger<AuthenticationDelegate>();
            Mock<HttpMessageHandler> handlerMock = new();
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(json);
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
            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, mockHttpClientFactory.Object, configuration, this.cacheProvider, null);
            Assert.Throws<InvalidOperationException>(() => authDelegate.AuthenticateAsUser(tokenUri, tokenRequest));
        }

        /// <summary>
        /// AuthenticateAsSystem - Happy Path.
        /// </summary>
        [SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Deferred Refactor")]
        [Fact]
        public void ShouldAuthenticateAsSystem()
        {
            Uri tokenUri = new("http://testsite");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
            };
            string json = """
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
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<AuthenticationDelegate> logger = loggerFactory.CreateLogger<AuthenticationDelegate>();
            Mock<HttpMessageHandler> handlerMock = new();
            using HttpResponseMessage httpResponseMessage = new();
            httpResponseMessage.StatusCode = HttpStatusCode.OK;
            httpResponseMessage.Content = new StringContent(json);

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
            Dictionary<string, string?> extraConfig = new()
            {
                { "ClientAuthentication:TokenUri", tokenUri.ToString() },
            };
            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, mockHttpClientFactory.Object, CreateConfiguration(extraConfig), new Mock<ICacheProvider>().Object, null);
            JwtModel actualModel = authDelegate.AuthenticateAsSystem(tokenUri, tokenRequest);
            expected.ShouldDeepEqual(actualModel);
        }

        [Theory]
        [InlineData("hg", UserLoginClientType.Web)]
        [InlineData("hg-mobile", UserLoginClientType.Mobile)]
        [InlineData("should-be-null", null)]
        public void ShouldBeAbleToDetermineLoginClientType(string clientAzp, UserLoginClientType? clientType)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<AuthenticationDelegate> logger = loggerFactory.CreateLogger<AuthenticationDelegate>();
            Mock<IHttpClientFactory> mockHttpClientFactory = new();
            Mock<ICacheProvider> mockCacheProvider = new();

            HttpContext httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new Claim[]
                        {
                            new("azp", clientAzp),
                        },
                        "token")),
            };
            Mock<IHttpContextAccessor> mockHttpContextAccessor = new();
            mockHttpContextAccessor.Setup(s => s.HttpContext).Returns(httpContext);
            AuthenticationDelegate authDelegate = new(
                logger,
                mockHttpClientFactory.Object,
                CreateConfiguration(new Dictionary<string, string?>()),
                mockCacheProvider.Object,
                mockHttpContextAccessor.Object);
            UserLoginClientType? result = authDelegate.FetchAuthenticatedUserClientType();
            Assert.Equal(clientType, result);
        }

        private static IConfiguration CreateConfiguration(Dictionary<string, string?> configParams)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddJsonFile("appsettings.local.json", true)
                .AddInMemoryCollection(configParams.ToList())
                .Build();
        }
    }
}
