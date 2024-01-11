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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Claims;
    using HealthGateway.Common.AccessManagement.Authorization.Handlers;
    using HealthGateway.Common.AccessManagement.Authorization.Requirements;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Delegates;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// UserDelegatedAccessHandler's Unit Tests.
    /// </summary>
    public class UserDelegatedAccessHandlerTests
    {
        private const int MaxDependentAge = 12;

        private readonly string hdid = "The User HDID";
        private readonly string resourceHdid = "The User HDID";
        private readonly string token = "Fake Access Token";
        private readonly string userId = "User ID";
        private readonly string username = "User Name";

        /// <summary>
        /// Handle Auth - Unknown Requirement Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task UnknownRequirement()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock(claimsPrincipal);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserDelegatedAccessHandler> logger =
                loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                new Mock<IPatientService>().Object,
                new Mock<IResourceDelegateDelegate>().Object);
            NameAuthorizationRequirement[] requirements = { new(this.username) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Read Write No Resource Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ReadWriteNoResource()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();

            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", this.token },
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
            ILogger<UserDelegatedAccessHandler> logger =
                loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                new Mock<IPatientService>().Object,
                new Mock<IResourceDelegateDelegate>().Object);
            PersonalFhirRequirement[] requirements = { new(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Delegation Disabled Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotAuthPatientReadAsUserDelegationDisabled()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock(claimsPrincipal);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserDelegatedAccessHandler> logger =
                loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                new Mock<IPatientService>().Object,
                new Mock<IResourceDelegateDelegate>().Object);
            PersonalFhirRequirement[] requirements = { new(FhirResource.Patient, FhirAccessType.Read, supportsUserDelegation: false) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - Non-owner Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotAuthPatientReadAsOwner()
        {
            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock(claimsPrincipal);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserDelegatedAccessHandler> logger = loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                new Mock<IPatientService>().Object,
                new Mock<IResourceDelegateDelegate>().Object);
            PersonalFhirRequirement[] requirements = { new(FhirResource.Patient, FhirAccessType.Read) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - User-Delegated Happy Path.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldAuthResourceDelegate()
        {
            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(MaxDependentAge * -1).AddDays(1),
            };
            RequestResult<PatientModel> getPatientResult = new(patientModel, ResultType.Success);

            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock(claimsPrincipal);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserDelegatedAccessHandler> logger =
                loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new();
            mockDependentDelegate.Setup(s => s.ExistsAsync(this.resourceHdid, this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService
                .Setup(s => s.GetPatientAsync(this.resourceHdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(getPatientResult);

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                mockPatientService.Object,
                mockDependentDelegate.Object);
            PersonalFhirRequirement[] requirements = { new(FhirResource.Observation, FhirAccessType.Read, supportsUserDelegation: true) };

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        /// <summary>
        /// Handle Auth - User-Delegated Expired Error.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task ShouldNotAuthExpiredDelegate()
        {
            PatientModel patientModel = new()
            {
                Birthdate = DateTime.Now.AddYears(MaxDependentAge * -1),
            };
            RequestResult<PatientModel> getPatientResult = new(patientModel, ResultType.Success);

            ClaimsPrincipal claimsPrincipal = this.GetClaimsPrincipal();
            Mock<IHttpContextAccessor> httpContextAccessorMock = this.GetHttpContextAccessorMock(claimsPrincipal);

            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<UserDelegatedAccessHandler> logger = loggerFactory.CreateLogger<UserDelegatedAccessHandler>();

            Mock<IResourceDelegateDelegate> mockDependentDelegate = new();
            mockDependentDelegate.Setup(s => s.ExistsAsync(this.resourceHdid, this.hdid, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Mock<IPatientService> mockPatientService = new();
            mockPatientService
                .Setup(s => s.GetPatientAsync(this.resourceHdid, PatientIdentifierType.Hdid, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(getPatientResult);

            UserDelegatedAccessHandler authHandler = new(
                logger,
                GetConfiguration(),
                httpContextAccessorMock.Object,
                mockPatientService.Object,
                mockDependentDelegate.Object);
            PersonalFhirRequirement[] requirements = [new(FhirResource.Observation, FhirAccessType.Read, supportsUserDelegation: true)];

            AuthorizationHandlerContext context = new(requirements, claimsPrincipal, null);

            await authHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(context.HasFailed);
        }

        private static IConfigurationRoot GetConfiguration()
        {
            Dictionary<string, string?> configDictionary = new()
            {
                { "Authorization:MaxDependentAge", MaxDependentAge.ToString(CultureInfo.CurrentCulture) },
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configDictionary.ToList())
                .Build();
        }

        private ClaimsPrincipal GetClaimsPrincipal()
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, this.username),
                new Claim(ClaimTypes.NameIdentifier, this.userId),
                new Claim(GatewayClaims.Hdid, this.hdid),
            };
            ClaimsIdentity identity = new(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private Mock<IHttpContextAccessor> GetHttpContextAccessorMock(ClaimsPrincipal claimsPrincipal)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary
            {
                { "Authorization", this.token },
            };
            RouteValueDictionary routeValues = new()
            {
                { "hdid", this.resourceHdid },
            };
            Mock<HttpRequest> httpRequestMock = new();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);
            httpRequestMock.Setup(s => s.RouteValues).Returns(routeValues);

            Mock<HttpContext> httpContextMock = new();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipal);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            return httpContextAccessorMock;
        }
    }
}
