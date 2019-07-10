using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGateway.WebClient
{
    public interface IPatientService
    {
        Task<List<PatientData>> GetPatients();
    }
}