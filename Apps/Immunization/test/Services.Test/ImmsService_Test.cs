using Xunit;
using HealthGateway.Service;
using HealthGateway.Engine.Core;
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
            const byte expectedCount = 17;
            // Create service to test
            IImmsService service = new ImmsService();

            Key key = Key.Create("Patient");
            List<ImmsDataModel> actualResult = service.GetMockData().ToList();

            // Verify the result
            Assert.Equal(expectedCount, actualResult.Count);
        }
    }
}
