using Xunit;
using Moq;
using HealthGateway.WebClient.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using DeepEqual.Syntax;
using HealthGateway.WebClient.Services;
using System;

namespace HealthGateway.WebClient.Test.Controllers
{
    public sealed class AuthControllerTest : IDisposable
    {
        private AuthController controller;
        private Mock<IAuthService> mockService;

        public AuthControllerTest()
        {
            // Mock dependency injection of controller
            this.mockService = new Mock<IAuthService>();

            // Creates the controller passing mocked dependencies
            this.controller = new AuthController(mockService.Object);
        }

        public void Dispose()
        {
            this.controller.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void ShouldLogon()
        {
            Models.AuthData expectedResult = new Models.AuthData { IsAuthenticated = true, Token = null, User = null };

            this.mockService
                .Setup(e => e.GetAuthenticationData()).Returns(expectedResult);

            // Call actual Authenticate
            Models.AuthData actualResult = (Models.AuthData)this.controller.GetAuthenticationData();
            Assert.True(expectedResult.IsDeepEqual(actualResult));
        }

        [Fact]
        public void ShouldLogout()
        {
            SignOutResult expectedResult = new SignOutResult(
                new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme },
                new AuthenticationProperties()
            );

            this.mockService
                .Setup(e => e.Logout()).Returns(expectedResult);

            // Should Logout
            SignOutResult actualResult = (SignOutResult)controller.Logout();

            Assert.Equal(expectedResult.AuthenticationSchemes, actualResult.AuthenticationSchemes);
            Assert.True(expectedResult.Properties.IsDeepEqual(actualResult.Properties));
        }
    }
}
