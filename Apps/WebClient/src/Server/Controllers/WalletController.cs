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
        private readonly IWalletService walletService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletController"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="walletService">The injected wallet service.</param>
        public WalletController(
            ILogger<WalletController> logger,
            IWalletService walletService)
        {
            this.logger = logger;
            this.walletService = walletService;
        }

        /// <summary>
        /// Creates a wallet connection.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>The created wallet credential connection model.</returns>
        [HttpPost]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<JsonResult> CreateConnection(string hdId)
        {
            this.logger.LogDebug($"Creating wallet connection for user {hdId}");
            RequestResult<WalletConnectionModel> result = await this.walletService.CreateConnectionAsync(hdId).ConfigureAwait(true);

            this.logger.LogDebug($"Finished creating wallet connection for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Gets a wallet connection.
        /// </summary>
        /// <param name="hdId">The user hdid.</param>
        /// <returns>The wallet connection model.</returns>
        [HttpGet]
        [Route("{hdid}/Connection")]
        [Authorize(Policy = UserProfilePolicy.Read)]
        public ActionResult GetConnection(string hdId)
        {
            this.logger.LogDebug($"Getting current wallet connection for user {hdId}");
            RequestResult<WalletConnectionModel> result = this.walletService.GetConnection(hdId);

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
        public async Task<ActionResult> DisconnectConnection(string hdId, Guid connectionId)
        {
            this.logger.LogDebug($"Disconnecting wallet connection for user {hdId}");
            RequestResult<WalletConnectionModel> result = await this.walletService.DisconnectConnectionAsync(connectionId, hdId).ConfigureAwait(true);

            this.logger.LogDebug($"Finished disconnecting wallet connection for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the Health Gateway Wallet Credential and revokes the credential with the Agent.
        /// </summary>
        /// <param name="hdId">The users hdid.</param>
        /// <param name="targetIds">The target ids of the immunization to create a credential for.</param>
        /// <returns>Something.</returns>
        [HttpPost]
        [Route("{hdid}/Credentials")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<JsonResult> CreateCredentials(string hdId, [FromBody] IEnumerable<string> targetIds)
        {
            this.logger.LogDebug($"Creating credential for user {hdId}");
            RequestResult<IEnumerable<WalletCredentialModel>> result = await this.walletService.CreateCredentialsAsync(hdId, targetIds).ConfigureAwait(true);
            this.logger.LogDebug($"Finished creating credential for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }

        /// <summary>
        /// Updates the Health Gateway Wallet Credential and revokes the credential with the Agent.
        /// </summary>
        /// <param name="hdId">The users hdid.</param>
        /// <param name="credentialId">The credential id belonging to the user to disconnect.</param>
        /// <returns>Something.</returns>
        [HttpDelete]
        [Route("{hdid}/Credential/{credentialId}")]
        [Authorize(Policy = UserProfilePolicy.Write)]
        public async Task<JsonResult> RevokeCredential(string hdId, Guid credentialId)
        {
            this.logger.LogDebug($"Revoking credential for user {hdId}");
            RequestResult<WalletCredentialModel> result = await this.walletService.RevokeCredential(credentialId, hdId).ConfigureAwait(true);
            this.logger.LogDebug($"Finished revoking credential for user {hdId}: {JsonSerializer.Serialize(result)}");
            return new JsonResult(result);
        }
    }
}
