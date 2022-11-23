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
namespace HealthGateway.JobScheduler.Controllers
{
    using HealthGateway.Common.AccessManagement.Authorization.Admin;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The JobSchedulerController controller enabling secure web access to JobScheduler.
    /// </summary>
    public class JobSchedulerController : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<JobSchedulerController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobSchedulerController"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="httpContextAccessor">The injected httpContextAccessor.</param>
        public JobSchedulerController(ILogger<JobSchedulerController> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Login Challenge.
        /// </summary>
        /// <returns>EmptyResult if authenticated; otherwise a ChallengeResult.</returns>
        [HttpGet(AuthorizationConstants.LoginPath)]
        public IActionResult Login()
        {
#pragma warning disable CA1303 //Disable literals
            if (this.HttpContext.User.Identity != null && !this.HttpContext.User.Identity.IsAuthenticated)
            {
                this.logger.LogDebug(@"Issuing Challenge result");
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme);
            }

            this.logger.LogDebug(@"Redirecting to dashboard");
            string basePath = this.httpContextAccessor.HttpContext?.Request.PathBase.Value ?? string.Empty;
            if (this.Url.IsLocalUrl(basePath))
            {
                return new RedirectResult($"{basePath}/");
            }

            return new RedirectResult("/");
#pragma warning restore CA1303 //Restore literal warning
        }

        /// <summary>
        /// Logout action.
        /// </summary>
        /// <returns>Redirect to main page.</returns>
#pragma warning disable CA1822 //  does not access instance data and can be marked as static
        [HttpGet(AuthorizationConstants.LogoutPath)]
        public IActionResult Logout()
        {
            return new SignOutResult(
                new[]
                {
                    OpenIdConnectDefaults.AuthenticationScheme,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                });
        }

        /// <summary>
        /// Authorization Failed action.
        /// </summary>
        /// <returns>Display Authorization Error.</returns>
        [HttpGet("/Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            this.Logout();
            return this.Unauthorized("401 Access Denied. You have not been authorized to access this Dashboard.");
        }
#pragma warning restore CA1822 //  does not access instance data and can be marked as static
    }
}
