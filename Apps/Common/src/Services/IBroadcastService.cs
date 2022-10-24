namespace HealthGateway.Common.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Service to interact with broadcasts.
    /// </summary>
    public interface IBroadcastService
    {
        /// <summary>
        /// Creates a broadcast.
        /// </summary>
        /// <param name="broadcast">The broadcast model.</param>
        /// <returns>The created broadcast wrapped in a RequestResult.</returns>
        Task<RequestResult<Broadcast>> CreateBroadcastAsync(Broadcast broadcast);

        /// <summary>
        /// Retrieves all broadcasts.
        /// </summary>
        /// <returns>The collection of broadcasts wrapped in a RequestResult.</returns>
        Task<RequestResult<IEnumerable<Broadcast>>> GetBroadcastsAsync();
    }
}
