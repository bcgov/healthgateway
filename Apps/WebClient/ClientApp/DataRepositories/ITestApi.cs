using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebClient
{
    public interface IPatientService
    {
        Task<List<PatientData>> GetPatients();
    }
}