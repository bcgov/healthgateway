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
    using HealthGateway.JobScheduler.Authorization;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The JobSchedulerController controller enabling secure web access to JobScheduler.
    /// </summary>
    public class JobSchedulerController : Controller
    {
        /// <summary>
        /// Login Challenge.
        /// </summary>
        /// <returns>EmptyResult if authenticated; otherwise a ChallengeResult.</returns>
        [HttpGet(AuthorizationConstants.LoginPath)]
        public IActionResult Login()
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult();
            }

            return new RedirectResult("/");
        }

        /// <summary>
        /// Logout action.
        /// </summary>
        /// <returns>Redirect to main page.</returns>
        #pragma warning disable CA1822 //  does not access instance data and can be marked as static

        [HttpGet(AuthorizationConstants.LogoutPath)]
        public IActionResult Logout()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme,
            });
        }
        #pragma warning restore CA1822 //  does not access instance data and can be marked as static

    }
}
