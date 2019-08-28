namespace HealthGateway.Immunization.Test.Controller
{
    using System.Collections.Generic;
    using Xunit;
    using HealthGateway.Service;
    using Moq;
    using HealthGateway.Models;
    using Microsoft.AspNetCore.Mvc;
    using DeepEqual.Syntax;
    using global::Immunization.Controllers;

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
            mockSvc.Setup(m => m.GetImmunizations()).Returns(expected);

            // Create Controller
            ImmsController controller = new ImmsController(mockSvc.Object);
            IEnumerable<ImmsDataModel> actualResult = controller.GetItems();
            // Verify the result
            Assert.True(actualResult.IsDeepEqual(expected));
        }
    }
}
