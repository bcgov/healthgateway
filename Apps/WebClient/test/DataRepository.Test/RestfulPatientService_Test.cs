using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using Fhir = Hl7.Fhir;

namespace HealthGateway.WebClient.Test.DataRepository
{
    public class RestfulPatientService_Test
    {
        [Fact]
        public async void Shold_GetPatientData_When_SearchResultExists()
        {
            // Setup mock fhir client and patient response
            Mock<Fhir.Rest.IFhirClient> mockClient = new Mock<Fhir.Rest.IFhirClient>();
            Fhir.Model.Patient patientResource = new Fhir.Model.Patient();
            patientResource.ResourceBase = new System.Uri("http://www.testurl.com/Patient");
            patientResource.Name.Add(Fhir.Model.HumanName.ForFamily("Doe").WithGiven("John"));
            patientResource.Id = "16515165161679879asd789879";
            patientResource.Text = new Fhir.Model.Narrative();
            patientResource.Text.Div = "<div></div>";

            // Create bundle entry
            Fhir.Model.Bundle.EntryComponent bundleEntry = new Fhir.Model.Bundle.EntryComponent();
            bundleEntry.FullUrl = patientResource.ResourceBase.ToString() + patientResource.ResourceType.ToString() + patientResource.Id;
            bundleEntry.Resource = patientResource;

            // Create Search bundle
            Fhir.Model.Bundle searchBundle = new Fhir.Model.Bundle();
            searchBundle.Type = Fhir.Model.Bundle.BundleType.Searchset;
            // adding some metadata
            searchBundle.Id = "uuid here";
            searchBundle.Meta = new Fhir.Model.Meta()
            {
                VersionId = "1",
            };
            searchBundle.Entry.Add(bundleEntry);
            searchBundle.Total = 1;

            mockClient.Setup(x => x.SearchAsync(It.IsAny<Fhir.Rest.SearchParams>(), It.IsAny<string>())).Returns(
                Task.FromResult(searchBundle));

            // Create service and execute GetPatients
            IPatientService service = new RestfulPatientService(mockClient.Object);
            List<PatientData> patientData = await service.GetPatients();

            Assert.Equal(patientResource.Id, patientData[0].Id);
        }
    }
}
