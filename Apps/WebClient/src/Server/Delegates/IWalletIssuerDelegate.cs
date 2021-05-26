namespace HealthGateway.WebClient.Delegates
{
    using System;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Server.Models.AcaPy;

    /// <summary>
    /// Interface that defines a delegate to create/revoke wallet connections and credentials
    /// </summary>
    public interface IWalletIssuerDelegate
    {
        /// <summary>
        /// Creates a connection invitation request
        /// </summary>
        /// <param name="walletConnectionId">The id of the wallet connection</param>
        /// <returns>Create ConnectionResponse including the invitation URL and the agent connection id</returns>
        Task<CreateConnectionResponse> CreateConnectionAsync(string walletConnectionId);
    }
}
