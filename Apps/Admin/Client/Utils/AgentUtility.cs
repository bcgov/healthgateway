namespace HealthGateway.Admin.Client.Utils
{
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Utilities for interacting with admin agents.
    /// </summary>
    public static class AgentUtility
    {
        /// <summary>
        /// Returns the formatted representation of an agent's identity provider.
        /// </summary>
        /// <param name="identityProvider">The agent's identity provider.</param>
        /// <returns>A string containing the formatted representation of a agent's identity provider.</returns>
        public static string FormatKeycloakIdentityProvider(KeycloakIdentityProvider identityProvider)
        {
            return identityProvider switch
            {
                KeycloakIdentityProvider.Idir => "IDIR",
                KeycloakIdentityProvider.PhsaAzure => "PHSA",
                _ => identityProvider.ToString(),
            };
        }
    }
}
