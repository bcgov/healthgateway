namespace HealthGateway.WebClient.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    [Route("api/[controller]")]
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

        public IActionResult Login(string hint, string redirectUri)
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme, this.authSvc.GetAuthenticationProperties(hint, redirectUri));
        }

        /// <summary>
        /// Performs the logon to the application
        /// </summary>
        /// <returns>An OkObjectResult with the authenticated data.</returns>
        [Authorize]
        public async Task<IActionResult> Logon()
        {
            Models.AuthData authData = await this.authSvc.Authenticate().ConfigureAwait(true);
            return new OkObjectResult(authData);
        }

        /// <summary>
        /// Performs the logout of the application
        /// </summary>
        /// <returns>A SignoutResult containing the redirect uri.</returns>
        public IActionResult Logout()
        {
            return this.authSvc.Logout();
        }
    }
}
