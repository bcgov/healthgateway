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
    using System.Threading.Tasks;
    using HealthGateway.Common.Filters;
    using HealthGateway.WebClient.Models.AcaPy;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Provides a web hook for Aca-Py to receive status updates on Wallet Connections and Credentials.
    /// </summary>
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    [IgnoreAudit]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class WalletStatusController : Controller
    {
        private readonly IConfigurationService configservice;
        private readonly IWalletStatusService walletStatusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletStatusController"/> class.
        /// </summary>
        /// <param name="configService">The injected configuration service provider.</param>
        /// <param name="walletStatusService">The injected wallet status service provider.</param>
        public WalletStatusController(IConfigurationService configService, IWalletStatusService walletStatusService)
        {
            this.configservice = configService;
            this.walletStatusService = walletStatusService;
        }

        /// <summary>
        /// Handle webhook events sent from the issuing agent.
        /// </summary>
        /// <param name="topic">The type of webhook response (connection or issue credential).</param>
        /// <param name="data">Webhook response data.</param>
        /// <returns>An empty response.</returns>
        /// <response code="204">Webhook request received.</response>
        [HttpPost]
        [Route("topic/{topic}")]
        public IActionResult Webhook(string topic, [FromBody] WebhookData data)
        {
            this.walletStatusService.WebhookAsync(topic, data);
            return this.NoContent();
        }
    }
}
