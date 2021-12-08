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
    using HealthGateway.Common.Constants;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using Xunit;

    /// <summary>
    /// FhirResourceAuthorizationHandler's Unit Tests.
    /// </summary>
    public class FhirResourceAuthorizationHandlerTests
    {
        /// <summary>
        /// Handle Auth - Unknown Requirement Error.
        /// </summary>
        [Fact]
        public void UnknownRequirement()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            NameAuthorizationRequirement[] requirements = new[] { new NameAuthorizationRequirement(username) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Read Write No Resource Error.
        /// </summary>
        [Fact]
        public void ReadWriteNoResource()
        {
            // Setup
            string hdid = "The User HDID";
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
            RouteValueDictionary routeValues = new();
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Delegation Disabled Error.
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientReadAsSystemDelegationDisabled()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "system/Patient.read";
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read, supportsSystemDelegation: false) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Empty Scope Error.
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientReadAsSystemEmptyScope()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = string.Empty;
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Missing Scope Error.
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientReadAsSystemMissingScope()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Owner Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAuthPatientReadAsOwner()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Owner Happy Path (Using Query String).
        /// </summary>
        [Fact]
        public void ShouldAuthPatientReadAsOwnerUsingParameter()
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
            IQueryCollection query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "hdid", resourceHDID },
            });
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.Query).Returns(query);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read, fhirLookup: FhirResourceLookup.Parameter) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-owner Error.
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientReadAsOwner()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - System Delegated Happy Path.
        /// </summary>
        [Fact]
        public void ShouldAuthPatientReadAsSystem()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "system/Patient.read";
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - User Delegated Happy Path.
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientReadAsOtherPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "user/Patient.read";
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.HDID, hdid),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Owner Happy Path (Write).
        /// </summary>
        [Fact]
        public void ShouldAuthPatientWriteAsOwner()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Write) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - System Delegated Happy Path (Write).
        /// </summary>
        [Fact]
        public void ShouldAuthPatientWriteAsSystem()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "system/Patient.write";
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Write) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - User Delegated Error (Write).
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientWriteAsOtherPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "user/Patient.write";
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.HDID, hdid),
                new Claim(GatewayClaims.Scope, scopes),
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Write) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-owner Error (Write).
        /// </summary>
        [Fact]
        public void ShouldNotAuthPatientWriteAsOwner()
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
            ILogger<FhirResourceAuthorizationHandler> logger = loggerFactory.CreateLogger<FhirResourceAuthorizationHandler>();

            FhirResourceAuthorizationHandler authHandler = new(logger, httpContextAccessorMock.Object);
            FhirRequirement[] requirements = new[] { new FhirRequirement(FhirResource.Patient, FhirAccessType.Write) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }
    }
}
