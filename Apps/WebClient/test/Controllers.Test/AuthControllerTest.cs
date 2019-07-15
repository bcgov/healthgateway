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

namespace HealthGateway.WebClient.Test.Controllers
{
    public class AuthControllerTest
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

        [Fact]
        public async void ShouldLogon()
        {
            Models.AuthData expectedAuthData = new Models.AuthData { IsAuthenticated = true, Token = null, User = "MockNameIdentifier" };
            OkObjectResult expectedResult = new OkObjectResult(expectedAuthData);

            this.mockService
                .Setup(e => e.Authenticate()).Returns(Task.FromResult(expectedAuthData));

            // Call actual Authenticate
            OkObjectResult actualResult = (OkObjectResult)await this.controller.Logon().ConfigureAwait(true);
            Assert.True(expectedResult.Value.IsDeepEqual(actualResult.Value));
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
