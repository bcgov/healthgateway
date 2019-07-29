namespace HealthGateway.WebClient.Controllers
{
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService authSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authSvc">The injected auth service provider.</param>
        public AuthController(IAuthService authSvc)
        {
            this.authSvc = authSvc;
        }

        /// <summary>
        /// The default page for the auth controller.
        /// </summary>
        /// <returns>The default view.</returns>
        [Authorize]
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Performs an OpenIdConnect Challenge.
        /// </summary>
        /// <param name="hint">A value to pass to KeyCloak to select the Identity Provider.</param>
        /// <param name="redirectUri">The redirectUri after successful authentication.</param>
        /// <returns>An IActionResult which results in a redirect.</returns>
        public IActionResult Login(string hint, string redirectUri)
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme, this.authSvc.GetAuthenticationProperties(hint, redirectUri));
        }

        /// <summary>
        /// Performs the logout of the application.
        /// </summary>
        /// <returns>A SignoutResult containing the redirect uri.</returns>
        public IActionResult Logout()
        {
            return this.authSvc.Logout();
        }

        /// <summary>
        /// Performs the logon to the application.
        /// </summary>
        /// <returns>A JSON object with the authentication data.</returns>
        [Route("/api/GetAuthenticationData")]
        public async Task<Models.AuthData> GetAuthenticationData()
        {
            Models.AuthData authData = await this.authSvc.GetAuthenticationData().ConfigureAwait(true);
            return authData;
        }
    }
}
