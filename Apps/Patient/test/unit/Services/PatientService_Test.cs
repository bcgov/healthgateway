namespace HealthGateway.Service.Patient.Test
{
    using Xunit;
    using Models;
    using Microsoft.Extensions.Logging;
    using Moq;

    public class PatientService_Test
    {
        [Fact]
        public async System.Threading.Tasks.Task Should_true()
        {
            var mock = new Mock<ILogger<LoggingMessageInspector>>();
            ILogger<LoggingMessageInspector> logger = mock.Object;

            PatientService service = new PatientService(new LoggingEndpointBehaviour(new LoggingMessageInspector(logger)));
            Patient pat = await service.GetPatient("qeqwe");
            Assert.True(true);
        }
    }
}
