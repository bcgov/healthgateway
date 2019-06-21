using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITestApi
{
    Task<List<PatientData>> GetPatients();
}