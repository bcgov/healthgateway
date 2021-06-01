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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle Verifiable Credentials.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class WalletController
    {
        private readonly IWalletService verifiableCredentialService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletController"/> class.
        /// </summary>
        /// <param name="verifiableCredentialService">The injected verifiable credential service.</param>
        public WalletController(IWalletService verifiableCredentialService)
        {
            this.verifiableCredentialService = verifiableCredentialService;
        }

        /// <summary>
        /// Creates a verifiable credential connection.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <param name="targetIds">The list of target ids of the verifiable credential.</param>
        /// <returns>The created verifiable credential connection model.</returns>
        [HttpPost]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<JsonResult> CreateConnection(string hdid, [FromBody] IEnumerable<string> targetIds)
        {
            return new JsonResult(await this.verifiableCredentialService.CreateConnectionAsync(hdid, targetIds).ConfigureAwait(true));
        }

        /// <summary>
        /// Gets a verifiable credential connection.
        /// </summary>
        /// <param name="hdid">The user hdid.</param>
        /// <returns>The verifiable credential connection model.</returns>
        [HttpGet]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public ActionResult GetConnection(string hdid)
        {
            return new JsonResult(this.verifiableCredentialService.GetConnection(hdid));
        }

        /// <summary>
        /// Gets a verifiable credential.
        /// </summary>
        /// <param name="exchangeId">The credential exchange id.</param>
        /// <returns>The verifiable credential model.</returns>
        [HttpGet]
        [Route("{hdid}/Credential")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public ActionResult GetCredential(Guid exchangeId)
        {
            return new JsonResult(this.verifiableCredentialService.GetCredential(exchangeId));
        }
    }
}
