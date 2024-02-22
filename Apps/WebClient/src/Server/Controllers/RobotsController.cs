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
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Mime;
    using HealthGateway.Common.Utils;
    using HealthGateway.WebClient.Server.Models;
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
            RobotsConfiguration robotsConfiguration = new();
            configuration.Bind(RobotsConfiguration.ConfigurationSectionKey, robotsConfiguration);
            string robotsFilePath = robotsConfiguration.RobotsFilePath;
            this.robotsContent = ReadRobotsFile(robotsFilePath);
        }

        /// <summary>
        /// Serves up an environment specific robots.txt.
        /// </summary>
        /// <returns>The robots text file.</returns>
        [Route("robots.txt")]
        [Produces(MediaTypeNames.Text.Plain)]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        private static string? ReadRobotsFile(string robotsFilePath)
        {
            string? defaultRobotsAssetContent = AssetReader.Read("HealthGateway.WebClient.Server.Assets.Robots.txt");

            try
            {
                return !string.IsNullOrEmpty(robotsFilePath) ? System.IO.File.ReadAllText(robotsFilePath) : defaultRobotsAssetContent;
            }
            catch (Exception)
            {
                return defaultRobotsAssetContent;
            }
        }
    }
}
