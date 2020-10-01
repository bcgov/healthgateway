namespace Keycloak.Authorization.Client
{
    public class Configuration
    {
        /// <summary>
        /// Gets or sets the authorization server URL.
        /// </summary>
        public string AuthServerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the realm
        /// </summary>
        public string Realm { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client Id
        /// </summary>
        public string ClientId { get; set; } = string.Empty;
    }

}