namespace HealthGateway.WebClient.Services
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    #pragma warning disable CA1303 // disable literal strings check

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">Injected HttpContext Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        /// <param name="logger">Injected Logger Provider.</param>
        public AuthService(ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <returns>The AuthData containing the token and user information.</returns>
        public async Task<Models.AuthData> GetAuthenticationData()
        {
            Models.AuthData authData = new Models.AuthData();
            ClaimsPrincipal user = this.httpContextAccessor.HttpContext.User;
            authData.IsAuthenticated = user.Identity.IsAuthenticated;
            if (authData.IsAuthenticated)
            {
                this.logger.LogDebug("Getting Authentication data");
                authData.User = new Models.User
                {
                    Id = user.FindFirstValue("preferred_username"),
                    Name = user.FindFirstValue("name"),
                    Email = user.FindFirstValue(ClaimTypes.Email),
                };
                authData.Token = user.FindFirstValue("access_token");
                    //await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token").ConfigureAwait(true);
            }

            return authData;
        }

        /// <summary>
        /// Clears the authorization data from the context.
        /// </summary>
        /// <returns>The signout confirmation followed by the redirect uri.</returns>
        public SignOutResult Logout()
        {
            this.logger.LogTrace("Logging out user");
            return new SignOutResult(new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme });
        }

        /// <summary>
        /// Returns the authentication properties with the populated hint and redirect URL.
        /// </summary>
        /// <returns> The AuthenticationProperties.</returns>
        /// <param name="hint">The OIDC IDP Hint.</param>
        /// <param name="redirectUri">The URI to redirect to after logon.</param>
        public AuthenticationProperties GetAuthenticationProperties(string hint, string redirectUri)
        {
            this.logger.LogDebug("Getting Authentication properties with hint={0} and redirectUri={1}", hint, redirectUri);
            AuthenticationProperties authProps = new AuthenticationProperties()
            {
                RedirectUri = redirectUri,
            };
            if (!string.IsNullOrEmpty(hint))
            {
                authProps.Items.Add(this.configuration["KeyCloak:IDPHintKey"], hint);
            }

            return authProps;
        }
    }
}
