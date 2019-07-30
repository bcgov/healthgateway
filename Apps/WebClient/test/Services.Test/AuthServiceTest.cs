using System.Threading.Tasks;
using Xunit;
using Moq;
using HealthGateway.WebClient.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using DeepEqual;
using DeepEqual.Syntax;
using HealthGateway.WebClient.Services;

namespace HealthGateway.WebClient.Test.Services
{
    public class AuthServiceTest
    {
        private Mock<IConfiguration> mockConfig;
        private Mock<ILogger<AuthService>> mockLog;
        private Mock<IHttpContextAccessor> mockHttpCtx;
        private AuthService service;

        public AuthServiceTest()
        {
            // Mock dependency injection of controller
            this.mockConfig = new Mock<IConfiguration>();
            this.mockLog = new Mock<ILogger<AuthService>>();
            this.mockHttpCtx = new Mock<IHttpContextAccessor>();
            // Creates the controller passing mocked dependencies
            this.service = new AuthService(mockLog.Object, mockHttpCtx.Object, mockConfig.Object);
        }

        [Fact]
        public void TestAuthenticated()
        {
            Models.AuthData expectedResult = new Models.AuthData()
            {
                IsAuthenticated = true,
                Token = null,
                User = new Models.User()
                {
                    Id = "Username",
                    Name = "Client Name",
                    Email = "client@mail.me",
                }
            };

            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                  new Claim(ClaimTypes.Email, "client@mail.me"),
                  new Claim("preferred_username", "Username"),
                  new Claim("name","Client Name"),
                }, "mockAuthType"));

            // Mock Authentication Service AuthenticateAsync call
            Mock<IAuthenticationService> mockAuthSvc = new Mock<IAuthenticationService>();
            mockAuthSvc
                .Setup(e => e.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(mockUser, CookieAuthenticationDefaults.AuthenticationScheme))));

            // Mock the serviceProvider for the http context
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthSvc.Object);

            // Mock the http context user and RequestServices
            mockHttpCtx
                .Setup(e => e.HttpContext.User).Returns(mockUser);
            mockHttpCtx
                .Setup(e => e.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Call actual Authenticate
            Models.AuthData actualResult = service.GetAuthenticationData();
            Assert.True(expectedResult.User.IsDeepEqual(actualResult.User));
            Assert.Equal(expectedResult.Token, actualResult.Token);
            Assert.Equal(expectedResult.IsAuthenticated, actualResult.IsAuthenticated);
        }

        [Fact]
        public void TestUnAuthenticated()
        {
            Models.AuthData expectedResult = new Models.AuthData()
            {
                IsAuthenticated = false,
                Token = null,
                User = null,
            };

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.Setup(u => u.Identity.IsAuthenticated).Returns(false);

            //var mockUser = new ClaimsPrincipal(new ClaimsIdentity(,Array.Empty<Claim>(), "mockAuthType"));

            // Mock Authentication Service AuthenticateAsync call
            Mock<IAuthenticationService> mockAuthSvc = new Mock<IAuthenticationService>();
            mockAuthSvc
                .Setup(e => e.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(mockUser.Object, CookieAuthenticationDefaults.AuthenticationScheme))));

            // Mock the serviceProvider for the http context
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthSvc.Object);

            // Mock the http context user and RequestServices
            mockHttpCtx
                .Setup(e => e.HttpContext.User).Returns(mockUser.Object);
            mockHttpCtx
                .Setup(e => e.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Call actual Authenticate
            Models.AuthData actualResult = service.GetAuthenticationData();
            Assert.True(expectedResult.User.IsDeepEqual(actualResult.User));
            Assert.Equal(expectedResult.Token, actualResult.Token);
            Assert.Equal(expectedResult.IsAuthenticated, actualResult.IsAuthenticated);
        }

        [Fact]
        public void ShouldLogout()
        {
            SignOutResult expectedResult = new SignOutResult(
                new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme }
            );

            // Should Logout
            SignOutResult actualResult = service.Logout();

            Assert.Equal(expectedResult.AuthenticationSchemes, actualResult.AuthenticationSchemes);
            Assert.True(expectedResult.Properties.IsDeepEqual(actualResult.Properties));
        }
    }
}
