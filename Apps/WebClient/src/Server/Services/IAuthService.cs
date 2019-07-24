namespace HealthGateway.WebClient.Services
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        Task<Models.AuthData> GetAuthenticationData();

        /// <summary>
        /// Clears the authorization data from the context.
        /// </summary>
        /// <returns>The signout confirmation followed by the redirect uri.</returns>
        SignOutResult Logout();

        /// <summary>
        /// Returns the authentication properties with the populated hint and redirect URL.
        /// </summary>
        /// <returns> The AuthenticationProperties.</returns>
        /// <param name="hint">The OIDC IDP Hint.</param>
        /// <param name="redirectUri">The URI to redirect to after logon.</param>
        AuthenticationProperties GetAuthenticationProperties(string hint, string redirectUri);
    }
}
