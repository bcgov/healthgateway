using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGateway.WebClient.Services;

namespace HealthGateway.WebClient.Controllers
{
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
