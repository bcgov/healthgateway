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
namespace HealthGateway.WebClient.Server.Controllers
{
    using System;
    using System.Net.Mime;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Dynamic Robots.txt for search engines.
    /// </summary>
    public class RobotsController : Controller
    {
        private readonly string? robotsContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotsController"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        public RobotsController(IConfiguration configuration)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            string? defaultRobotsAssetContent = AssetReader.Read("HealthGateway.WebClient.Server.Assets.Robots.txt");
            string? envRobotsAssetContent = AssetReader.Read($"HealthGateway.WebClient.Server.Assets.Robots.{environment}.txt");
            this.robotsContent = configuration.GetValue("robots.txt", envRobotsAssetContent ?? defaultRobotsAssetContent);
        }

        /// <summary>
        /// Serves up an environment specific robots.txt.
        /// </summary>
        /// <returns>The robots text file.</returns>
        [Route("robots.txt")]
        [Produces(MediaTypeNames.Text.Plain)]
        public ActionResult Robots()
        {
            ContentResult result = new()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = this.robotsContent,
            };
            return result;
        }
    }
}
