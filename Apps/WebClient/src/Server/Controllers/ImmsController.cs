//-------------------------------------------------------------------------
// Copyright � 2019 Province of British Columbia
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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ImmsController : Controller
    {
        private readonly IImmsService immsSvc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmsController"/> class.
        /// </summary>
        /// <param name="immsSvc">The injected imms service provider.</param>
        public ImmsController(IImmsService immsSvc)
        {
            this.immsSvc = immsSvc;
        }

        /// <summary>
        /// Gets a list of Imms records.
        /// </summary>
        /// <returns>A list of Immunization recorsd.</returns>
        [HttpGet]
        [Route("/api/[controller]/v{version:apiVersion}/items")]
        [Route("/api/[controller]/items")]
        [ProducesResponseType(200)]
        public async Task<JsonResult> GetItems()
        {
            IEnumerable<Models.ImmsData> data = await this.immsSvc.GetItems().ConfigureAwait(true);
            return new JsonResult(data);
        }
    }
}
