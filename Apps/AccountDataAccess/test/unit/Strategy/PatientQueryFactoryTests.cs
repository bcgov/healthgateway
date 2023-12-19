namespace AccountDataAccessTest.Strategy
{
    using HealthGateway.AccountDataAccess.Patient.Strategy;
    using Moq;
    using Xunit;

    public class PatientQueryFactoryTests
    {
        [Fact]
        public void ShouldThrowIfStrategyTypeIsNull()
        {
            PatientQueryFactory patientQueryFactory = new(new Mock<IServiceProvider>().Object);

            string strategyType = "Not.A.Strategy.Type";

            Assert.Throws<ArgumentException>(() => patientQueryFactory.GetPatientQueryStrategy(strategyType));
        }
    }
}
