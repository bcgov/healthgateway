namespace HealthGateway.WebClient.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Immunization Service.
    /// </summary>
    public interface IImmsService
    {
        /// <summary>
        /// Returns the list of immunization records.
        /// </summary>
        /// <returns>A list of immunization records.</returns>
        Task<IEnumerable<Models.ImmsData>> GetItems();
    }
}
