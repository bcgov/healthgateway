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
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Administration;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Newtonsoft.Json;
    using Xunit;

    /// <summary>
    /// KeycloakUserAdminDelegate's Unit Tests.
    /// </summary>
    public class KeycloakUserAdminDelegateTests
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeycloakUserAdminDelegateTests"/> class.
        /// </summary>
        public KeycloakUserAdminDelegateTests()
        {
            this.configuration = GetIConfigurationRoot();
        }

        /// <summary>
        /// GetUser - Not Implemented Error.
        /// </summary>
        [Fact]
        public void GetUserThrowsException()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            IUserAdminDelegate keycloakDelegate = new KeycloakUserAdminDelegate(logger, new Mock<IHttpClientService>().Object, this.configuration);

            Assert.Throws<NotImplementedException>(() => keycloakDelegate.GetUser(Guid.NewGuid(), new JWTModel()));
        }

        /// <summary>
        /// GetUsers - Valid role returns users.
        /// </summary>
        [Fact]
        public void ShouldGetUsers()
        {
            ResultType expectedResult = ResultType.Success;
            int expectedCount = 1;

            // Arrange
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            var response = new List<UserRepresentation>()
            {
                new UserRepresentation()
                {
                    FirstName = "first name",
                    LastName = "last name",
                    UserId = Guid.NewGuid(),
                    CreatedTimestamp = DateTime.UtcNow,
                },
            };

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json"),
            };
            IUserAdminDelegate keycloakDelegate = GetKeycloakUserAdminDelegate(this.configuration, logger, httpResponseMessage);

            JWTModel jwt = new()
            {
                AccessToken = "Bearer Token",
            };

            // Act
            Task<RequestResult<IEnumerable<UserRepresentation>>> result = keycloakDelegate.GetUsers(IdentityAccessRole.AdminUser, jwt);

            // Assert
            Assert.Equal(expectedResult, result.Result.ResultStatus);
            Assert.Equal(expectedCount, result.Result.TotalResultCount);
        }

        /// <summary>
        /// GetUsers - Valid role does not return users.
        /// </summary>
        [Fact]
        public void ShouldNotGetUsers()
        {
            ResultType expectedResult = ResultType.Success;
            int expectedCount = 0;

            // Arrange
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            var response = new List<UserRepresentation>();

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8, "application/json"),
            };
            IUserAdminDelegate keycloakDelegate = GetKeycloakUserAdminDelegate(this.configuration, logger, httpResponseMessage);

            JWTModel jwt = new()
            {
                AccessToken = "Bearer Token",
            };

            // Act
            Task<RequestResult<IEnumerable<UserRepresentation>>> result = keycloakDelegate.GetUsers(IdentityAccessRole.AdminUser, jwt);

            // Assert
            Assert.Equal(expectedResult, result.Result.ResultStatus);
            Assert.Equal(expectedCount, result.Result.TotalResultCount);
        }

        /// <summary>
        /// GetUsers - Returns error.
        /// </summary>
        [Fact]
        public void GetUsersReturnsError()
        {
            ResultType expectedResult = ResultType.Error;

            // Arrange
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.NotFound,
            };
            IUserAdminDelegate keycloakDelegate = GetKeycloakUserAdminDelegate(this.configuration, logger, httpResponseMessage);

            JWTModel jwt = new()
            {
                AccessToken = "Bearer Token",
            };

            // Act
            Task<RequestResult<IEnumerable<UserRepresentation>>> result = keycloakDelegate.GetUsers(IdentityAccessRole.AdminUser, jwt);

            // Assert
            Assert.Equal(expectedResult, result.Result.ResultStatus);
        }

        /// <summary>
        /// DeleteUser - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldDeleteUser()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            string response = "Deleted";
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(response),
            };
            IUserAdminDelegate keycloakDelegate = GetKeycloakUserAdminDelegate(this.configuration, logger, httpResponseMessage);

            JWTModel jwt = new()
            {
                AccessToken = "Bearer Token",
            };

            bool deleted = keycloakDelegate.DeleteUser(Guid.NewGuid(), jwt);

            Assert.True(deleted);
        }

        /// <summary>
        /// DeleteUser - Unauthorized.
        /// </summary>
        [Fact]
        public void ShouldNotDeleteUser()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<KeycloakUserAdminDelegate> logger = loggerFactory.CreateLogger<KeycloakUserAdminDelegate>();

            string response = "Did not delete!";
            using HttpResponseMessage httpResponseMessage = new()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent(response),
            };
            IUserAdminDelegate keycloakDelegate = GetKeycloakUserAdminDelegate(this.configuration, logger, httpResponseMessage);

            JWTModel jwt = new()
            {
                AccessToken = "Bearer Token",
            };

            Assert.Throws<AggregateException>(() => keycloakDelegate.DeleteUser(Guid.NewGuid(), jwt));
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { "KeycloakAdmin:DeleteUserUrl", "https://localhost" },
                { "KeycloakAdmin:GetRolesUrl", "https://localhost" },
            };

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .AddInMemoryCollection(myConfiguration)
                .Build();
        }

        private static IUserAdminDelegate GetKeycloakUserAdminDelegate(IConfiguration configuration, ILogger<KeycloakUserAdminDelegate> logger, HttpResponseMessage httpResponseMessage)
        {
            Mock<HttpMessageHandler> handlerMock = new();
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
            IUserAdminDelegate keycloakDelegate = new KeycloakUserAdminDelegate(logger, mockHttpClientService.Object, configuration);
            return keycloakDelegate;
        }
    }
}
