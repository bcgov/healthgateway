﻿// -------------------------------------------------------------------------
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
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using HealthGateway.Admin.Services;
    using HealthGateway.Database.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to handle user feedback review.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "AdminUser")]
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
        /// Adds a given communication to the backend.
        /// </summary>
        /// <returns>The added communication wrapped in a RequestResult.</returns>
        /// <param name="communication">The communication to be added.</param>
        /// <response code="200">Returns the communication json.</response>
        /// <response code="401">the client must authenticate itself to get the requested response.</response>
        /// <response code="403">The client does not have access rights to the content; that is, it is unauthorized, so the server is refusing to give the requested resource. Unlike 401, the client's identity is known to the server.</response>
        [HttpPost]
        public IActionResult Add(Communication communication)
        {
            return new JsonResult(this.communicationService.Add(communication));
        }
    }
}
