// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Controllers
{
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using HealthGateway.Common.Authorization.Admin;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<AuthenticationController> logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <param name="authenticationService">The injected auth service provider.</param>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpContextAccessor">The injected httpContextAccessor.</param>
        public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.authenticationService = authenticationService;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
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
        [ProducesResponseType(200)]
        public AuthenticationData GetAuthenticationData()
        {
            AuthenticationData authData = this.authenticationService.GetAuthenticationData();
            return authData;
        }

        /// <summary>
        /// Performs an OpenIdConnect Challenge.
        /// </summary>
        /// <returns>An IActionResult which results in a redirect.</returns>
        [HttpGet(AuthorizationConstants.LoginPath)]
        public IActionResult Login()
        {
            if (this.HttpContext.User.Identity != null && !this.HttpContext.User.Identity.IsAuthenticated)
            {
                this.logger.LogDebug("Issuing Challenge result");
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
            }

            this.authenticationService.SetLastLoginDateTime();

            this.logger.LogDebug("Redirecting to COVID-19 support page");
            string basePath = this.httpContextAccessor.HttpContext?.Request.PathBase.Value ?? string.Empty;
            if (this.Url.IsLocalUrl(basePath))
            {
                return new RedirectResult($"{basePath}/covidcard");
            }

            return new RedirectResult("/covidcard");
        }

        /// <summary>
        /// Performs the logout of the application.
        /// </summary>
        /// <returns>A SignoutResult containing the redirect uri.</returns>
        [HttpGet(AuthorizationConstants.LogoutPath)]
        public IActionResult Logout()
        {
            return this.authenticationService.Logout();
        }
    }
}
