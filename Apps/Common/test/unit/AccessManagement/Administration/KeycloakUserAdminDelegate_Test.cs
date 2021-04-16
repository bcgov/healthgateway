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
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class KeycloakUserAdminDelegate_Test
    {
        private readonly IConfiguration configuration;

        public KeycloakUserAdminDelegate_Test()
        {
            this.configuration = GetIConfigurationRoot();
        }

        [Fact]
        public void GetUserThrowsException()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();
            IUserAdminDelegate keycloakDelegate = new KeycloakUserAdminDelegate(logger, new Mock<IHttpClientService>().Object, this.configuration);
            Assert.Throws<NotImplementedException>(() => keycloakDelegate.GetUser(Guid.NewGuid(), new Common.AccessManagement.Authentication.Models.JwtModel()));
        }

        [Fact]
        public void ShouldDeleteUser()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            string response = "Deleted";
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response),
            };
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
            IUserAdminDelegate keycloakDelegate = new KeycloakUserAdminDelegate(logger, mockHttpClientService.Object, this.configuration);

            HealthGateway.Common.AccessManagement.Authentication.Models.JwtModel jwt = new HealthGateway.Common.AccessManagement.Authentication.Models.JwtModel()
            {
                AccessToken = "Bearer Token",
            };
            bool deleted = keycloakDelegate.DeleteUser(Guid.NewGuid(), jwt);
            Assert.True(deleted);
        }

        [Fact]
        public void ShouldNotDeleteUser()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            string response = "Did not delete!";
            using HttpResponseMessage httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(response),
            };
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
            IUserAdminDelegate keycloakDelegate = new KeycloakUserAdminDelegate(logger, mockHttpClientService.Object, this.configuration);

            HealthGateway.Common.AccessManagement.Authentication.Models.JwtModel jwt = new HealthGateway.Common.AccessManagement.Authentication.Models.JwtModel()
            {
                AccessToken = "Bearer Token",
            };
            Assert.Throws<AggregateException>(() => keycloakDelegate.DeleteUser(Guid.NewGuid(), jwt));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "KeycloakAdmin:DeleteUserUrl", "https://localhost" },
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
