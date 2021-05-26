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

        /// <summary>
        /// Revokes a connection
        /// </summary>
        /// <param name="agentId">The connection_id of the wallet connection</param>
        /// <returns>boolean indicating success.</returns>
        Task<bool> RevokeConnectionAsync(string agentId);

        /// <summary>
        /// Issue a credential to a connection
        /// </summary>
        /// <param name="agentId">The connection_id of the wallet connection</param>
        /// <param name="immunizationCredential">The attributes of the credential</param>
        /// <returns>The invitation URL</returns>
        Task<string> IssueCredentialAsync(string agentId, ImmunizationCredential immunizationCredential);

        /// <summary>
        /// Revokes a credential
        /// </summary>
        /// <param name="credentialExchangeId">The credential_exchange_id for the credential</param>
        /// <param name="agentId">The connection_id of the wallet connection</param>
        /// <param name="addedDateTime">Date credential was accepted into the wallet</param>
        /// <returns>boolean indicating success.</returns>
        Task<bool> RevokeCredentialAsync(string credentialExchangeId, string agentId, DateTime? addedDateTime);
    }
}
