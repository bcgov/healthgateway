using Xunit;
using HealthGateway.Service;
using HealthGateway.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthGateway.Immunization.Test.Controller
{
    public class ImmsService_Test
    {
        [Fact]
        public void Should_GetMockData()
        {
            const int expectedCount = 17;
            // Create service to test
            IImmsService service = new MockImmsService();
            IEnumerable<ImmsDataModel> actualResult = service.GetImmunizations().ToList();

            // Verify the result
            Assert.Equal(expectedCount, actualResult.Count());
        }
    }
}
