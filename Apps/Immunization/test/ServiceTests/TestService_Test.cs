using Xunit;
using HealthGateway.Service;
using HealthGateway.Engine.Core;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using Moq;

namespace HealthGateway.Test.Service
{
    public class TestService_Test
    {
        [Fact]
        public void Shold_ReadPatients_When_KeyIsValid()
        {
            // Setup mock fhir client and patient response
            Mock<IFhirClient> mockClient = new Mock<IFhirClient>();
            Patient res = new Patient();
            res.Name.Add(HumanName.ForFamily("Doe").WithGiven("John"));
            mockClient.Setup(x => x.Read<DomainResource>(It.IsAny<string>(), It.IsAny<string>(), null)).Returns(res);

            // Create service to tests
            IFhirService service = new TestService(mockClient.Object);

            // Execute the request to read patient's data
            Key key = Key.Create("Patient");
            ServerFhirResponse response = service.Read(key);

            // Verify the response
            Assert.Equal(res.Name[0].Family, ((Patient)response.Resource).Name[0].Family);
        }
    }
}
