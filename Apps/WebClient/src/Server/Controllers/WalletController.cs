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
    using System.Text.Json;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Authorization.Policy;
    using HealthGateway.Common.Models;
    using HealthGateway.WebClient.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Web API to handle Verifiable Credentials.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class WalletController
    {
        private readonly ILogger logger;
        private readonly IWalletService verifiableCredentialService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="verifiableCredentialService">The injected verifiable credential service.</param>
        public WalletController(
            ILogger<WalletController> logger,
            IWalletService verifiableCredentialService)
        {
            this.logger = logger;
            this.verifiableCredentialService = verifiableCredentialService;
        }

        /// <summary>
        /// Creates a verifiable credential connection.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <param name="targetIds">The list of target ids of the verifiable credential.</param>
        /// <returns>The created verifiable credential connection model.</returns>
        [HttpPost]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<JsonResult> CreateConnection(string hdId, [FromBody] IEnumerable<string> targetIds)
        {
            this.logger.LogDebug($"Creating wallet connection {JsonSerializer.Serialize(targetIds)} for user {hdId}");
            RequestResult<WalletConnectionModel> result = await this.verifiableCredentialService.CreateConnectionAsync(hdId, targetIds).ConfigureAwait(true);

            this.logger.LogDebug($"Finished creating wallet connection {JsonSerializer.Serialize(targetIds)} for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a verifiable credential connection.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>The verifiable credential connection model.</returns>
        [HttpGet]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public ActionResult GetConnection(string hdId)
        {
            this.logger.LogDebug($"Getting current wallet connection for user {hdId}");
            RequestResult<WalletConnectionModel> result = this.verifiableCredentialService.GetConnection(hdId);

            this.logger.LogDebug($"Finished getting current wallet connection for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the Health Gateway Wallet Connection and revokes the connection with the Agent.
        /// </summary>
        /// <param name="hdId">The users hdid.</param>
        /// <param name="connectionId">The connection Id belonging to the user to disconnect.</param>
        /// <returns>Something.</returns>
        [HttpDelete]
        [Route("{hdid}/Connection/{connectionId}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public ActionResult DisconnectConnection(string hdId, Guid connectionId)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Updates the Health Gateway Wallet Credentail and revokes the credential with the Agent.
        /// </summary>
        /// <param name="hdId">The users hdid.</param>
        /// <param name="credentialId">The credential id belonging to the user to disconnect.</param>
        /// <returns>Something.</returns>
        [HttpDelete]
        [Route("{hdid}/Credential/{credentialId}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public ActionResult RevokeCredential(string hdId, Guid credentialId)
        {
            throw new NotSupportedException();
        }
    }
}
