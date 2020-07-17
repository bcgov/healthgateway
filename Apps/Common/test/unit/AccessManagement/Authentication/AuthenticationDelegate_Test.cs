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
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Services;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Moq.Protected;
    using System.Threading;
    using System.Net;
    using System.Text.Json;
    using DeepEqual.Syntax;
    using HealthGateway.Common.AccessManagement.Authentication;
    using HealthGateway.Common.AccessManagement.Authentication.Models;

    public class AuthenticationDelegate_Test
    {

        private readonly IConfiguration configuration;

        public AuthenticationDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot();
        }

        [Fact]
        public void ShouldAuthenticateAsUser()
        {
            string json = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            JWTModel expected = JsonSerializer.Deserialize<JWTModel>(json, options);
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<IAuthenticationDelegate> logger = loggerFactory.CreateLogger<IAuthenticationDelegate>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(json),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, this.configuration, mockHttpClientService.Object);
            JWTModel actualModel = authDelegate.AuthenticateAsUser();
            Assert.True(actualModel.IsDeepEqual(expected));
        }

        [Fact]
        public void ShouldAuthenticateAsSystem()
        {
            string json = @"{ ""access_token"":""token"", ""expires_in"":500, ""refresh_expires_in"":0, ""refresh_token"":""refresh_token"", ""token_type"":""bearer"", ""not-before-policy"":25, ""session_state"":""session_state"", ""scope"":""scope"" }";
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                WriteIndented = true,
            };
            JWTModel expected = JsonSerializer.Deserialize<JWTModel>(json, options);
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<IAuthenticationDelegate> logger = loggerFactory.CreateLogger<IAuthenticationDelegate>();
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(json),
               })
               .Verifiable();
            Mock<IHttpClientService> mockHttpClientService = new Mock<IHttpClientService>();
            mockHttpClientService.Setup(s => s.CreateDefaultHttpClient()).Returns(() => new HttpClient(handlerMock.Object));

            IAuthenticationDelegate authDelegate = new AuthenticationDelegate(logger, this.configuration, mockHttpClientService.Object);
            JWTModel actualModel = authDelegate.AuthenticateAsSystem();
            Assert.True(actualModel.IsDeepEqual(expected));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"ClientAuthentication:TokenUri", "https://localhost"},
                {"ClientAuthentication:Audience", "audience"},
                {"ClientAuthentication:Scope", "scope"},
                {"ClientAuthentication:ClientId", "client_id"},
                {"ClientAuthentication:ClientSecret", "client_secret"},
                {"ClientAuthentication:Username", "username"},
                {"ClientAuthentication:Password", "password"},
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }
    }
}
