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
namespace HealthGateway.WebClient.Controllers
{
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle system communications.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class CommunicationController
    {
        private readonly ICommunicationService communicationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationController"/> class.
        /// </summary>
        /// <param name="communicationService">The injected communication service.</param>
        public CommunicationController(
            ICommunicationService communicationService)
        {
            this.communicationService = communicationService;
        }

        /// <summary>
        /// Gets the latest active communication.
        /// </summary>
        /// <param name="bannerType">The banner type to retrieve.</param>
        /// <returns>The active communication or null if not found.</returns>
        /// <response code="200">Returns the communication json.</response>
        [HttpGet]
        [Route("{bannerType}")]
        public IActionResult Get(CommunicationType bannerType = CommunicationType.Banner)
        {
            RequestResult<Communication> result = this.communicationService.GetActiveBanner(bannerType);
            return new JsonResult(result);
        }
    }
}
