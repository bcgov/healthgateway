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

namespace HealthGateway.WebClient.Test.Controllers
{
    public class AuthController_Test
    {

        private Mock<IConfiguration> mockConfig;
        private Mock<ILogger<AuthController>> mockLog;
        private AuthController controller;

        public AuthController_Test()
        {
            // Mock dependency injection of controller
            mockConfig = new Mock<IConfiguration>();
            mockLog = new Mock<ILogger<AuthController>>();
            // Creates the controller passing mocked dependencies
            controller = new AuthController(mockConfig.Object, mockLog.Object);
        }

        [Fact]
        public async void Shold_Logon()
        {
            LogonResult expectedResult = new LogonResult() { Auth = true, Token = null, User = "MockNameIdentifier" };

            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "MockNameIdentifier"),
            }, "mockAuthType"));


            // Mock Authentication Service used by GetToKenAsync method
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(_ => _.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(mockUser, CookieAuthenticationDefaults.AuthenticationScheme))));

            // Mock Service Provider used by GetToKenAsync method
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(_ => _.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            // Mock the controller context (HttpContext)
            controller.ControllerContext = new ControllerContext() {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser,
                    RequestServices = serviceProviderMock.Object
                }
            };

            // Should Logon
            // The result is not typed thats why the properties are being casted
            // We need to research how to avoid (updating the controller maybe?)
            LogonResult actualResult = (LogonResult)((OkObjectResult)await controller.Logon()).Value;

            Assert.Equal(expectedResult.User, actualResult.User);
            Assert.Equal(expectedResult.Token, actualResult.Token);
            Assert.Equal(expectedResult.Auth, actualResult.Auth);
        }

        [Fact]
        public void Shold_Logout()
        {
            SignOutResult expectedResult = new SignOutResult(
                new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme}, 
                new AuthenticationProperties() 
            );

            // Should Logout
            // The result is not typed thats why the properties are being casted
            // We need to research how to avoid (updating the controller maybe?)
            SignOutResult actualResult = (SignOutResult)controller.Logout();

            Assert.Equal(expectedResult.AuthenticationSchemes, actualResult.AuthenticationSchemes);
            Assert.True(expectedResult.Properties.IsDeepEqual(actualResult.Properties));
        }
    }
}
