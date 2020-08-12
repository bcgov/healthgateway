namespace HealthGateway.Immunization.Test.Controller
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Controllers;
    using HealthGateway.Immunization.Models;
    using HealthGateway.Immunization.Services;
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

            Mock<IImmunizationService> svcMock = new Mock<IImmunizationService>();

            RequestResult<IEnumerable<ImmunizationView>> result = new RequestResult<IEnumerable<ImmunizationView>>();
            IEnumerable<ImmunizationView> immunizations = new List<ImmunizationView>();
            immunizations.Append(new ImmunizationView()
            {
                Name = "test"
            });
            result.ResourcePayload = immunizations;
            result.PageSize = immunizations.Count();
            result.PageIndex = 0;
            result.TotalResultCount = immunizations.Count();

            svcMock.Setup(m => m.GetImmunizations(hdid)).ReturnsAsync(result);

            ImmunizationController controller = new ImmunizationController(
                new Mock<ILogger<ImmunizationController>>().Object,
                svcMock.Object,
                httpContextAccessorMock.Object);

            // Act
            JsonResult jsonResult = (JsonResult)await controller.GetImmunizations(hdid).ConfigureAwait(true);

            // Verify
            RequestResult<IEnumerable<ImmunizationView>> actual = (RequestResult<IEnumerable<ImmunizationView>>)jsonResult.Value;

            // Verify the result
            Assert.True(actual.ResourcePayload.IsDeepEqual(immunizations));
        }
    }
}
