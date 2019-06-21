using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;
using System.Linq;
using WebClient.Log;

public class RestfulTestApi : ITestApi
{

    public RestfulTestApi()
    {
    }

    public async Task<List<PatientData>> GetPatients()
    {
        string url = "http://localhost:3001/api/Fhir/";
        //string url = "http://test.fhir.org/r3/";
        string type = "Patient";

        SearchParams searchCommand = new SearchParams();
        searchCommand.Add("Patient", "Steven");

        var FhirClient = new LogFhirClient(url);
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