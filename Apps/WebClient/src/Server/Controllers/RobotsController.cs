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
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly string robotsFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotsController"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration.</param>
        public RobotsController(IConfiguration configuration)
        {
            WebClientConfiguration webClientConfiguration = new();
            configuration.Bind(WebClientConfiguration.ConfigurationSectionKey, webClientConfiguration);
            this.robotsFilePath = webClientConfiguration.RobotsFilePath;
        }

        /// <summary>
        /// Serves up an environment specific robots.txt.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The robots text file.</returns>
        [Route("robots.txt")]
        [Produces(MediaTypeNames.Text.Plain)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult> Robots(CancellationToken ct = default)
        {
            string? robotsContent = await ReadRobotsFileAsync(this.robotsFilePath, ct);

            ContentResult result = new()
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = MediaTypeNames.Text.Plain,
                Content = robotsContent,
            };
            return result;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        private static async Task<string?> ReadRobotsFileAsync(string robotsFilePath, CancellationToken ct)
        {
            try
            {
                return !string.IsNullOrEmpty(robotsFilePath) ? await System.IO.File.ReadAllTextAsync(robotsFilePath, ct) : GetDefaultContent();
            }
            catch (Exception)
            {
                return GetDefaultContent();
            }
        }

        private static string? GetDefaultContent()
        {
            return AssetReader.Read("HealthGateway.WebClient.Server.Assets.Robots.txt");
        }
    }
}
