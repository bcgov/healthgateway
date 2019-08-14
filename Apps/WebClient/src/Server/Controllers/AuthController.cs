//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------

namespace HealthGateway.WebClient.Controllers
{
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [Produces("application/json")]
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
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/Auth")]
        [Authorize]
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Performs an OpenIdConnect Challenge.
        /// </summary>
        /// <param name="idpHint">A value to pass to KeyCloak to select the Identity Provider.</param>
        /// <param name="redirectUri">The redirect uri after successful authentication.</param>
        /// <returns>An IActionResult which results in a redirect.</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/Auth/Login")]
        public IActionResult Login(string idpHint, System.Uri redirectUri)
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme, this.authSvc.GetAuthenticationProperties(idpHint, redirectUri));
        }

        /// <summary>
        /// Performs the logout of the application.
        /// </summary>
        /// <returns>A SignoutResult containing the redirect uri.</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("/Auth/Logout")]
        public IActionResult Logout()
        {
            return this.authSvc.Logout();
        }

        /// <summary>
        /// Reads the ASP.Net Core Authentication cookie (if available) and provides Authentication data.
        /// </summary>
        /// <remarks>
        /// The /api/GetAuthenticationData route has been deprecated in favour of /api/auth/GetAuthenticationData.
        /// This API will likely be replaced by client side authentication in the near future and is not meant to be consumed outside of the WebClient.
        /// </remarks>
        /// <returns>The authentication model representing the current ASP.Net Core Authentication cookie.</returns>
        [HttpGet]
        [Route("/api/[controller]/v{version:apiVersion}/GetAuthenticationData")]
        [Route("/api/[controller]/GetAuthenticationData")]
        [Route("/api/GetAuthenticationData")]
        [ProducesResponseType(200)]
        public Models.AuthData GetAuthenticationData()
        {
            Models.AuthData authData = this.authSvc.GetAuthenticationData();
            return authData;
        }
    }
}
