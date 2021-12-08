// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.CommonTests.AccessManagement.Authorization
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// ApiKeyAuthorizationHandler's Unit Tests.
    /// </summary>
    public class ApiKeyAuthorizationHandlerTests
    {
        private const string ApiKey = "FakeAPIKey";
        private const string InvalidApiKey = "NotAMatch";

        /// <summary>
        /// Handle Auth - Auth Key is correct.
        /// </summary>
        [Fact]
        public void AuthSuccess()
        {
            ApiKeyRequirement[] requirements = new[] { new ApiKeyRequirement(GetIConfigurationRoot(ApiKey)) };
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new();
            AuthorizationHandlerContext context = new(requirements, mockClaimsPrincipal.Object, null);
            ApiKeyAuthorizationHandler authHandler = GetAuthorizationHandler(ApiKey);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
        }

        /// <summary>
        /// Handle Auth - Auth Key is invalid.
        /// </summary>
        [Fact]
        public void AuthFailHeaderKeyWrong()
        {
            ApiKeyRequirement[] requirements = new[] { new ApiKeyRequirement(GetIConfigurationRoot(ApiKey)) };
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new();
            AuthorizationHandlerContext context = new(requirements, mockClaimsPrincipal.Object, null);
            ApiKeyAuthorizationHandler authHandler = GetAuthorizationHandler(InvalidApiKey);
            authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        /// <summary>
        /// Handle Auth - Requirement Key is invalid.
        /// </summary>
        [Fact]
        public void AuthFailRequirementKeyBad()
        {
            ApiKeyRequirement[] requirements = new[] { new ApiKeyRequirement(GetIConfigurationRoot(string.Empty)) };
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new();
            AuthorizationHandlerContext context = new(requirements, mockClaimsPrincipal.Object, null);
            ApiKeyAuthorizationHandler authHandler = GetAuthorizationHandler(InvalidApiKey);
            authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        /// <summary>
        /// Handle Auth - Auth Key not found.
        /// </summary>
        [Fact]
        public void AuthFailHeaderNotFound()
        {
            ApiKeyRequirement[] requirements = new[] { new ApiKeyRequirement(GetIConfigurationRoot(ApiKey, "fakekey")) };
            Mock<ClaimsPrincipal> mockClaimsPrincipal = new();
            AuthorizationHandlerContext context = new(requirements, mockClaimsPrincipal.Object, null);
            ApiKeyAuthorizationHandler authHandler = GetAuthorizationHandler(InvalidApiKey);
            authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        private static ApiKeyAuthorizationHandler GetAuthorizationHandler(string apiKey)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { ApiKeyRequirement.ApiKeyHeaderNameDefault, apiKey },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserAuthorizationHandler> logger = loggerFactory.CreateLogger<UserAuthorizationHandler>();

            return new ApiKeyAuthorizationHandler(logger, httpContextAccessorMock.Object);
        }

        private static IConfigurationRoot GetIConfigurationRoot(string requirementKey, string headerKeyName = ApiKeyRequirement.ApiKeyHeaderNameDefault)
        {
            Dictionary<string, string> myConfiguration = new()
            {
                { $"{ApiKeyRequirement.WebHookApiSectionKey}:{ApiKeyRequirement.WebHookApiKey}", requirementKey },
                { $"{ApiKeyRequirement.WebHookApiSectionKey}:{ApiKeyRequirement.ApiKeyHeaderNameKey}", headerKeyName },
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
