using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
        private readonly IPatientService patientService;

        public SampleDataController(IPatientService patientService)
        {
            this.patientService = patientService;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<PatientData>> GetPatients()
        {
            return await patientService.GetPatients();
        }
    }
}
