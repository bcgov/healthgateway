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
    using System.Threading.Tasks;

    /// <summary>
    /// The JobSchedulerController controller enabling secure web access to JobScheduler.
    /// </summary>
    public class JobSchedulerController : Controller
    {
        /// <summary>
        /// hello.
        /// </summary>
        /// <returns>hello world message.</returns>
        [HttpGet("/hello")]
        [Authorize]
        public string Hello()
        {
            return @"Hello, World!";
        }

        /// <summary>
        /// Login Challenge.
        /// </summary>
        /// <returns>EmptyResult if authenticated; otherwise a ChallengeResult.</returns>
        [HttpGet(AuthorizationConstants.LoginPath)]
        public IActionResult Login(string origin = "/")
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult();
            }

            return new RedirectResult(origin);
        }
        /// <summary>
        /// Logout action.
        /// </summary>
        /// <returns>Redirect to main page.</returns>
        [HttpGet(AuthorizationConstants.LogoutPath)]
        public IActionResult Logout()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            });
        }
    }
}
