namespace HealthGateway.Immunization.Test.Controller
{
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
    using Moq;
    using System.Security.Claims;
    using Xunit;

    public class ImmunizationController_Test
    {
        [Fact]
        public void Should_GetItems()
        {
            string errorMessage = "The error message";
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";

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

            Mock<IImmsService> svcMock = new Mock<IImmsService>();
            svcMock
                .Setup(s => s.GetImmunizations(hdid))
                .ReturnsAsync(new IEnumerable<ImmsDataModel>(new IEnumerable<ImmsDataModel>()) { IsError = true, Error = errorMessage });

            httpContextAccessorMock.Setup(s => s.HttpContext).Returns(httpContextMock.Object);
            Mock<IImmsService> mockSvc = new Mock<IImmsService>();
            List<ImmsDataModel> expected = new List<ImmsDataModel>();
            expected.Add(new ImmsDataModel()
            {
                Vaccine = "test"
            });
            mockSvc.Setup(m => m.GetImmunizations()).Returns(expected);

            // Create Controller
            ImmunizationController controller = new ImmunizationController(mockSvc.Object);
            IEnumerable<ImmsDataModel> actualResult = controller.GetItems();
            // Verify the result
            Assert.True(actualResult.IsDeepEqual(expected));
        }
    }
}
