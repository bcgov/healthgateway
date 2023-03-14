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
namespace HealthGateway.Admin.Server.Controllers
{
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle dependent delegation.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
    public class DelegationController
    {
        private readonly IDelegationService delegationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationController"/> class.
        /// </summary>
        /// <param name="delegationService">The injected delegation service.</param>
        public DelegationController(IDelegationService delegationService)
        {
            this.delegationService = delegationService;
        }

        /// <summary>
        /// Gets the dependent delegation for the given personal health number.
        /// </summary>
        /// <param name="phn">The dependent phn.</param>
        /// <returns>A <see cref="Task{TResult}"/>Representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> GetDelegationInformation([FromQuery] string phn)
        {
            return new JsonResult(await this.delegationService.GetDelegationInformationAsync(phn).ConfigureAwait(true));
        }
    }
}
