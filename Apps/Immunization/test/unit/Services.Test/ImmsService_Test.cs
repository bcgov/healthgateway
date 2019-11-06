using Xunit;
using HealthGateway.Immunization.Services;
using HealthGateway.Immunization.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthGateway.Immunization.Test.Controller
{
    public class ImmsService_Test
    {
        [Fact]
        public void Should_GetMockData()
        {
            string hdid = "EXTRIOYFPNX35TWEBUAJ3DNFDFXSYTBC6J4M76GYE3HC5ER2NKWQ";
            const int expectedCount = 17;
            // Create service to test
            IImmsService service = new MockImmsService();
            IEnumerable<ImmsDataModel> actualResult = service.GetImmunizations(hdid).ToList();

            // Verify the result
            Assert.Equal(expectedCount, actualResult.Count());
        }
    }
}
