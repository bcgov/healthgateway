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
namespace HangfireDefault.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The default Hangfire controller.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        /// <summary>
        /// Default index.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Index()
        {
            return Redirect("/hangfire");
        }

        /// <summary>
        /// Default index.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Login()
        {
            return new EmptyResult();
        }

        /// <summary>
        /// Default index.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Logout()
        {
            return HttpContext.Authentication.SignOutAsync();
        }
    }
}
