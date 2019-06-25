using System.Collections.Generic;
using System.Threading.Tasks;

public interface IPatientService
{
    Task<List<PatientData>> GetPatients();
}