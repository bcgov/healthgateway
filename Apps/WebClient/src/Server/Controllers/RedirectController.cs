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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net.Mime;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Redirects Salesforce requests to the correct HG endpoint.
    /// </summary>
    public class RedirectController : Controller
    {
        /// <summary>
        /// Redirects the URL to the correct Health Gateway endpoint.
        /// </summary>
        /// <param name="oldPath">The path under /s that we want to redirect.</param>
        /// <returns>A redirect.</returns>
        [Route("/s/{oldPath}")]
        [Produces(MediaTypeNames.Text.Plain)]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Deferred")]
        public ActionResult MapRedirect(string oldPath)
        {
            string baseUrl = $"{this.Request.Scheme}://{this.Request.Host}";

            string newPath = oldPath.ToLower(CultureInfo.InvariantCulture) switch
            {
                "timeline" => "timeline",
                "dependents" => "dependents",
                "service" => "services",
                "export-records"=> "reports",
                "profile" => "profile",
                _ => "home",
            };

            return this.Redirect($"{baseUrl}/{newPath}");
        }
    }
}
