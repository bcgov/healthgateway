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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The JobSchedulerController controller enabling secure web access to JobScheduler.
    /// </summary>
    public class JobSchedulerController : ControllerBase
    {

        /// <summary>
        /// Index.
        /// </summary>

        [HttpGet("/")]
        public IActionResult Index()
        {
            return RedirectToAction("Hello");
        }

        /// <summary>
        /// Login.
        /// </summary>
        [Authorize]
        [HttpGet("/login")]
        public IActionResult Login()
        {
            /*
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult();
            }*/
            return RedirectToAction("Hello");
        }

        /// <summary>
        /// Logout Challenge.
        /// </summary>
        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            });
        }

        /// <summary>
        /// hello.
        /// </summary>
        [HttpGet("/hello")]
        // [Authorize]
        public string Hello()
        {
            return "Hello World!";
        }
    }
}
