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
namespace HealthGateway.Admin.Controllers
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Delegates;
    using HealthGateway.Common.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Authentication and Authorization controller.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    public class TestController : Controller
    {
        private readonly ILogger<TestController> logger;
        private readonly IImmunizationAdminDelegate immunizationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestController"/> class.
        /// </summary>
        /// <param name="logger">The injected logger provider.</param>
        /// <param name="immunizationDelegate">The injected httpContextAccessor.</param>
        public TestController(ILogger<TestController> logger, IImmunizationAdminDelegate immunizationDelegate)
        {
            this.logger = logger;
            this.immunizationDelegate = immunizationDelegate;
        }

        /// <summary>
        /// Reads the ASP.Net Core Authentication cookie (if available) and provides Authentication data.
        /// </summary>
        /// <remarks>
        /// The /api/GetAuthenticationData route has been deprecated in favour of /api/auth/GetAuthenticationData.
        /// This API will likely be replaced by client side authentication in the near future and is not meant to be consumed outside of the WebClient.
        /// </remarks>
        /// <returns>The authentication model representing the current ASP.Net Core Authentication cookie.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get()
        {
            PatientModel patient = new ()
            {
                PersonalHealthNumber = "9735353315",
                Birthdate = DateTime.ParseExact("19670602", "yyyyMMdd", CultureInfo.InvariantCulture),
            };
            // var immunization = await this.immunizationDelegate.GetImmunizationEvent("bb6840df-0ef2-40b3-1f9e-08d92ba03fd1").ConfigureAwait(true);
            var immunizations = await this.immunizationDelegate.GetImmunizationEvents(patient).ConfigureAwait(true);
            return new JsonResult("ok");
        }
    }
}