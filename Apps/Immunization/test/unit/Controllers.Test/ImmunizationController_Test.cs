namespace HealthGateway.Immunization.Test.Controller
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Castle.Core.Logging;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Authorization;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ImmunizationController_Test
    {
        [Fact]
        public async Task ShouldGetImmunizations()
        {
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            string userId = "1001";

            IHeaderDictionary headerDictionary = new HeaderDictionary();
            headerDictionary.Add("Authorization", "Bearer TestJWT");
            Mock<HttpRequest> httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.Setup(s => s.Headers).Returns(headerDictionary);

            Mock<IIdentity> identityMock = new Mock<IIdentity>();
            identityMock.Setup(s => s.Name).Returns(userId);

            Mock<ClaimsPrincipal> claimsPrincipalMock = new Mock<ClaimsPrincipal>();
            claimsPrincipalMock.Setup(s => s.Identity).Returns(identityMock.Object);

            Mock<HttpContext> httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(s => s.User).Returns(claimsPrincipalMock.Object);
            httpContextMock.Setup(s => s.Request).Returns(httpRequestMock.Object);

            Mock<IHttpContextAccessor> httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);

            Mock<IAuthorizationService> authzMock = new Mock<IAuthorizationService>();
            authzMock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), hdid, PolicyNameConstants.UserIsPatient)).ReturnsAsync(AuthorizationResult.Success);

            Mock<IImmunizationService> svcMock = new Mock<IImmunizationService>();

            List<ImmunizationView> immunizations = new List<ImmunizationView>();
            immunizations.Add(new ImmunizationView()
            {
                Name = "test"
            });
            svcMock.Setup(m => m.GetImmunizations(hdid)).Returns(immunizations);

            ImmunizationController controller = new ImmunizationController(
                new Mock<ILogger<ImmunizationController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object,
                authzMock.Object);

            // Act
            JsonResult result = (JsonResult)await controller.GetImmunizations(hdid).ConfigureAwait(true);

            // Verify
            RequestResult<List<ImmunizationView>> actual = (RequestResult<List<ImmunizationView>>)result.Value;

            // Verify the result
            Assert.True(actual.ResourcePayload.IsDeepEqual(immunizations));
        }
    }
}
