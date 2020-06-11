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
    using HealthGateway.Common.AccessManagement.Authorization;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class UserAuthorizationHandler_Test
    {
        [Fact]
        public void ShouldAuthIsPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void UnknownRequirement()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new NameAuthorizationRequirement(username) };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ReadWriteNoResource()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientReadRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldNotAuthIsPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldAuthPatientReadAsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
           
            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientReadRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldNotAuthPatientReadAsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientReadRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

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
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientReadRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldAuthPatientReadAsOtherPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "user/Patient.read";
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
                new Claim(GatewayClaims.Scope, scopes),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientReadRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldAuthPatientWriteAsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientWriteRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

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
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(GatewayClaims.Scope, scopes),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientWriteRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldAuthPatientWriteAsOtherPatient()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = "The Resource HDID";
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";
            string scopes = "user/Patient.write";
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(PatientClaims.Patient, hdid),
                new Claim(GatewayClaims.Scope, scopes),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientWriteRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        [Fact]
        public void ShouldNotAuthPatientWriteAsOwner()
        {
            // Setup
            string hdid = "The User HDID";
            string resourceHDID = hdid;
            string token = "Fake Access Token";
            string userId = "User ID";
            string username = "User Name";

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", token);
            RouteValueDictionary routeValues = new RouteValueDictionary();
            routeValues.Add("hdid", resourceHDID);
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<PatientAuthorizationHandler> logger = loggerFactory.CreateLogger<PatientAuthorizationHandler>();

            PatientAuthorizationHandler authHandler = new PatientAuthorizationHandler(logger, httpContextAccessorMock.Object);
            var requirements = new[] { new PatientWriteRequirement() };

            AuthorizationHandlerContext context = new AuthorizationHandlerContext(requirements, claimsPrincipal, null);
            authHandler.HandleAsync(context);
            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }
    }
}
