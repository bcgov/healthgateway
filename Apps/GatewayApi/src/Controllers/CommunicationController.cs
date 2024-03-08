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
namespace HealthGateway.GatewayApi.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Services;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle system communications.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class CommunicationController
    {
        private readonly ICommunicationService communicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationController"/> class.
        /// </summary>
        /// <param name="communicationService">The injected communication service.</param>
        public CommunicationController(ICommunicationService communicationService)
        {
            this.communicationService = communicationService;
        }

        /// <summary>
        /// Gets the latest active communication.
        /// </summary>
        /// <param name="communicationType">The communication type to retrieve.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The active communication or null if not found.</returns>
        /// <response code="200">Returns the communication json.</response>
        [HttpGet]
        [Route("{communicationType}")]
        public async Task<RequestResult<Communication?>> Get(CommunicationType communicationType = CommunicationType.Banner, CancellationToken ct = default)
        {
            return await this.communicationService.GetActiveCommunicationAsync(communicationType, ct);
        }
    }
}
