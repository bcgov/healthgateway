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
    using System.Diagnostics;
    using System.IO;
    using HealthGateway.Common.Filters;
    using HealthGateway.Common.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// HomeController for Vue WebClient app.
    /// </summary>
    [IgnoreAudit]
    public class HomeController : Controller
    {
        private readonly INonceService nonceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        public HomeController(INonceService nonceService)
        {
            this.nonceService = nonceService;
        }

        /// <summary>
        /// The default page for the home controller.
        /// </summary>
        /// <returns>The default view.</returns>
        public IActionResult Index()
        {
            string nonce = this.nonceService.GetCurrentNonce();
            this.ViewData.Add("nonce", nonce);
            return this.View();
        }

        /// <summary>
        /// The bc gov logo image.
        /// </summary>
        /// <returns>The logo image.</returns>
        [Route("Logo.png")]
        public IActionResult Logo()
        {
            var file = Path.Combine(
                Directory.GetCurrentDirectory(),
                "ClientApp",
                "app",
                "assets",
                "images",
                "gov",
                "bcid-logo-rev-en.png");

            return this.PhysicalFile(file, "image/png");
        }

        /// <summary>
        /// The default page for error.
        /// </summary>
        /// <returns>The default view.</returns>
        public IActionResult Error()
        {
            this.ViewData["RequestId"] = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier;
            return this.View();
        }
    }
}
