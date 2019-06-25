using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using WebClient.Log;

namespace WebClient
{
    public class RestfulPatientService : IPatientService
    {
        private string baseUrl;

        public RestfulPatientService(IEnvironment environment)
        {
            string host = environment.GetValue("IMMUNIZATION_URL");
            string version = environment.GetValue("IMMUNIZATION_VERSION");
            string path = environment.GetValue("IMMUNIZATION_PATH");


            baseUrl = new UriBuilder(host + version + path).ToString();
        }

        public RestfulPatientService(string url)
        {
            baseUrl = url;
        }

        public async Task<List<PatientData>> GetPatients()
        {
            string type = "Patient";

            SearchParams searchCommand = new SearchParams();
            searchCommand.Add("Patient", "Steven");

            var FhirClient = new LogFhirClient(baseUrl);
            FhirClient.Timeout = (60 * 1000);
            FhirClient.PreferredFormat = ResourceFormat.Json;
            Bundle bundle = await FhirClient.SearchAsync(searchCommand, type);

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