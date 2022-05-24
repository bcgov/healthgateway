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
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Common.Utils;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Salesforce mock controller.
    /// </summary>
    [Route("[controller]")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Mocked Controller methods.")]
    public class SalesforceController : ControllerBase
    {
        /// <summary>
        /// Gets mock data for medication requests.
        /// </summary>
        /// <returns>The mocked medication request json.</returns>
        [HttpGet]
        [Route("medicationRequest")]
        [Produces("application/json")]
        public ContentResult MedicationRequest()
        {
            string? payload = AssetReader.Read("HealthGateway.Mock.Assets.MedicationRequest.json");
            return new ContentResult { Content = payload!, ContentType = "application/json" };
        }
    }
}
