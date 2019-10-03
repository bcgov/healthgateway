namespace HealthGateway.MedicationService.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// The patient service.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Gets the patient phn.
        /// </summary>
        /// <param name="hdid">The patient hdid.</param>
        /// <returns>The patient phn.</returns>
        Task<string> GetPatientPHNAsync(string hdid);
    }
}
