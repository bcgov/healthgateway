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
namespace HealthGateway.Mock.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Utils;
    using HealthGateway.Mock.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Odr Mock controller.
    /// </summary>
    [Route("[controller]")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Mocked Controller methods.")]
    public class OdrController : ControllerBase
    {
        private const string ProtectiveWord = "KEYWORD";

        /// <summary>
        /// Gets mock data for encounters.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>The mocked encounter json.</returns>
        [HttpPost]
        [Route("encounter")]
        [Produces("application/json")]
        public ContentResult Encounter([FromBody] OdrRequest request)
        {
            Dictionary<string, string> variables = new()
            {
                { "${uuid}", request.Id },
                { "${hdid}", request.HdId },
                { "${requestingIP}", request.RequestingIP },
            };
            string? fixtureString = AssetReader.Read("HealthGateway.Mock.Assets.Encounter.json");
            return new ContentResult { Content = ReplaceVariables(fixtureString!, variables), ContentType = "application/json" };
        }

        /// <summary>
        /// Gets mock data for medications.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>The mocked medication json.</returns>
        [HttpPost]
        [Route("medication")]
        [Produces("application/json")]
        public ContentResult Medication([FromBody] OdrRequest request)
        {
            Dictionary<string, string> variables = new()
            {
                { "${uuid}", request.Id },
                { "${hdid}", request.HdId },
                { "${requestingIP}", request.RequestingIP },
            };
            string? fixtureString = AssetReader.Read("HealthGateway.Mock.Assets.Medication.json");
            return new ContentResult { Content = ReplaceVariables(fixtureString!, variables), ContentType = "application/json" };
        }

        /// <summary>
        /// Mocks the endpoint for retrieving the protective word.
        /// </summary>
        /// <param name="request">The query request.</param>
        /// <returns>The mocked protective word json.</returns>
        [HttpPost]
        [Route("maintainProtectiveWord")]
        [Produces("application/json")]
        public ContentResult MaintainProtectiveWord([FromBody] OdrRequest request)
        {
            Dictionary<string, string> variables = new()
            {
                { "${uuid}", request.Id },
                { "${hdid}", request.HdId },
                { "${requestingIP}", request.RequestingIP },
            };

            // Protected User HDID
            if (request.HdId == "RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWQ")
            {
                variables.Add("${value}", ProtectiveWord);
            }
            else
            {
                variables.Add("${value}", string.Empty);
            }

            string? fixtureString = AssetReader.Read("HealthGateway.Mock.Assets.ProtectiveWord.json");
            return new ContentResult { Content = ReplaceVariables(fixtureString!, variables), ContentType = "application/json" };
        }

        private static string ReplaceVariables(string fixtureString, Dictionary<string, string> variables)
        {
            foreach (KeyValuePair<string, string> variable in variables)
            {
                fixtureString = fixtureString.Replace(variable.Key, variable.Value, StringComparison.CurrentCulture);
            }

            return fixtureString;
        }
    }
}
