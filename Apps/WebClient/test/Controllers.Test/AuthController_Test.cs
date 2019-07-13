using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using HealthGateway.WebClient.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HealthGateway.Mock;

namespace HealthGateway.WebClient.Test.Controllers
{
    public class AuthController_Test
    {
        [Fact]
        public async void Shold_Logon()
        {
            Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();
            Mock<ILogger<AuthController>> mockLog = new Mock<ILogger<AuthController>>();

            Princi principal = new CustomPrincipal("2038786");
            principal.UserId = "2038786";
            principal.FirstName = "Test";
            principal.LastName = "User";
            principal.IsStoreUser = true;


            OkObjectResult expectedResult = new OkObjectResult(new LogonResult { Auth = true, Token = "", User = "" });

            //Controller needs a controller context 
            ControllerContext mockControllerContext = new ControllerContext()
            {
                HttpContext = new MockHttpContext()
            };

            mockControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity[1]);
            AuthController controller = new AuthController(mockConfig.Object, mockLog.Object)
            {
                ControllerContext = mockControllerContext
            };

            //httpContext.Request.Headers["token"] = "fake_token_here"; //Set header
            IActionResult actualResult = await controller.Logon();


            Assert.Equal(1, 2);
        }
    }
}
