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
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserAuthorizationHandler's Unit Tests.
    /// </summary>
    public class UserAuthorizationHandlerTests
    {
        /// <summary>
        /// Handle Auth - Owner Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAuthUserIsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.HDID, hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
            };
            RouteValueDictionary routeValues = new()
            {
                { "hdid", resourceHDID },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserAuthorizationHandler> logger = loggerFactory.CreateLogger<UserAuthorizationHandler>();

            UserAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            UserRequirement[] requirements = new[] { new UserRequirement(true) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            authHandler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-Owner Error.
        /// </summary>
        [Fact]
        public void ShouldNotAuthUserIsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.HDID, hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
            };
            RouteValueDictionary routeValues = new()
            {
                { "hdid", resourceHDID },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserAuthorizationHandler> logger = loggerFactory.CreateLogger<UserAuthorizationHandler>();

            UserAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            UserRequirement[] requirements = new[] { new UserRequirement(true) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-Owner Happy Path (Disabled Validate Ownership).
        /// </summary>
        [Fact]
        public void ShouldAuthUserHasHDID()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.HDID, hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
            };
            RouteValueDictionary routeValues = new()
            {
                { "hdid", resourceHDID },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserAuthorizationHandler> logger = loggerFactory.CreateLogger<UserAuthorizationHandler>();

            UserAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            UserRequirement[] requirements = new[] { new UserRequirement(false) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            authHandler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-Owner No HdId Error (Disabled Validate Ownership).
        /// </summary>
        [Fact]
        public void ShouldNotAuthUserNoHDID()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", token },
            };
            RouteValueDictionary routeValues = new()
            {
                { "hdid", resourceHDID },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserAuthorizationHandler> logger = loggerFactory.CreateLogger<UserAuthorizationHandler>();

            UserAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            UserRequirement[] requirements = new[] { new UserRequirement(false) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }
    }
}
