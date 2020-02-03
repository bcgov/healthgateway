// -------------------------------------------------------------------------
//  Copyright © 2020 Province of British Columbia
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
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    using System.Threading.Tasks;

    public class UserController : Controller
    {
        private ILogger<UserController> logger;
        private IConfiguration configuration;

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string hint)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/dashboard" });
            }
            return Redirect("/dashboard");
        }
        public async Task<IActionResult> Logout()
        {
            return new SignOutResult(new[]
            { OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            });
        }
    }
}