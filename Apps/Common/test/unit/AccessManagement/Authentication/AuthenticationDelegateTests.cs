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
namespace HealthGateway.CommonTests.AccessManagement.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Caching.Memory;
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
        /// <summary>
        /// AuthenticateAsUser - Happy Path.
        /// </summary>
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1506:Avoid excessive class coupling", Justification = "Team decision")]
        public void ShouldAuthenticateAsUser()
        {
            Uri tokenUri = new("http://testsite");
            Dictionary<string, string> configurationParams = new()
            {
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

            using IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            string json = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JWTModel? expected = JsonSerializer.Deserialize<JWTModel>(json);
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<IAuthenticationDelegate> logger = loggerFactory.CreateLogger<IAuthenticationDelegate>();
            Mock<HttpMessageHandler> handlerMock = new();
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, mockHttpClientService.Object, configuration, memoryCache, null);
            JWTModel actualModel = authDelegate.AuthenticateAsUser(tokenUri, tokenRequest, false);
            expected.ShouldDeepEqual(actualModel);

            (JWTModel cacheResult, bool cached) = authDelegate.AuthenticateUser(tokenUri, tokenRequest, true);
            Assert.True(!cached);

            (JWTModel fetchedCachedResult, cached) = authDelegate.AuthenticateUser(tokenUri, tokenRequest, true);
            Assert.True(cached);
        }

        /// <summary>
        /// AuthenticateAsSystem - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAuthenticateAsSystem()
        {
            Uri tokenUri = new("http://testsite");
            ClientCredentialsTokenRequest tokenRequest = new()
            {
                ClientId = "CLIENT_ID",
                ClientSecret = "SOME_SECRET",
            };
            string json = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            JWTModel? expected = JsonSerializer.Deserialize<JWTModel>(json);
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<IAuthenticationDelegate> logger = loggerFactory.CreateLogger<IAuthenticationDelegate>();
            Mock<HttpMessageHandler> handlerMock = new();
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(httpResponseMessage)
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, mockHttpClientService.Object, CreateConfiguration(null), null, null);
            JWTModel actualModel = authDelegate.AuthenticateAsSystem(tokenUri, tokenRequest);
            expected.ShouldDeepEqual(actualModel);
        }

        private static IConfiguration CreateConfiguration(Dictionary<string, string> configParams)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(configParams)
                .Build();
        }
    }
}
