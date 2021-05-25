using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HealthGateway.WebClient.Delegates
{
    public interface IWalletIssuerDelegate
    {
        Task<WalletConnectionResponse> CreateConnectionAsync(string walletConnectionId);
        Task<WalletConnectionResponse> RevokeConnectionAsync(string agentId);
        Task<WalletCredentialResponse> IssueCredentialAsync(string agentId, string attributes);
        Task<bool> RevokeCredentialAsync(string credentialExchangeId, string agentId, DateTime? addedDateTime);
    }
}
