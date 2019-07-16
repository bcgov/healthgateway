using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;

namespace HealthGateway.WebClient
{
    public class RestfulPatientService : IPatientService
    {
        private readonly IFhirClient fhirClient;

        public RestfulPatientService(IFhirClient fhirClient)
        {
            this.fhirClient = fhirClient;
        }

        public async Task<List<PatientData>> GetPatients()
        {
            string type = "Patient";
            SearchParams searchCommand = new SearchParams();
            searchCommand.Add("Patient", "Steven");

            Bundle bundle = await fhirClient.SearchAsync(searchCommand, type);

            List<PatientData> patientDataList = new List<PatientData>();
            foreach (var patient in bundle.GetResources())
            {
                Patient pat = (Patient)patient;
                patientDataList.Add(new PatientData()
                {
                    Name = pat.Name[0].ToString(),
                    Id = pat.Id.ToString(),
                    DivStruct = pat.Text.Div.ToString()
                });
            }

            return patientDataList;
        }
    }
}