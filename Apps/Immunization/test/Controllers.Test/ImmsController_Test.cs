using Xunit;
using HealthGateway.Service;
using HealthGateway.Engine.Core;
using Moq;
using HealthGateway.Models;
using System.Collections.Generic;
using System.Linq;
using Immunization.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DeepEqual.Syntax;

namespace HealthGateway.Immunization.Test.Controller
{
    public class ImmsController_Test
    {
        [Fact]
        public void Should_GetItems()
        {
            // Create mock service
            Mock<IImmsService> mockSvc = new Mock<IImmsService>();
            List<ImmsDataModel> expected = new List<ImmsDataModel>();
            expected.Add(new ImmsDataModel()
            {
                Vaccine = "test"
            });
            mockSvc.Setup(m => m.GetMockData()).Returns(expected);

            // Create Controller
            ImmsController controller = new ImmsController(mockSvc.Object);
            JsonResult actualResult = controller.GetItems();
            // Verify the result
            Assert.True(actualResult.Value.IsDeepEqual(expected));
        }
    }
}
