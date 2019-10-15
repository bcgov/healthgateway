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
namespace HealthGateway.HNClient.Controllers
{
    using HealthGateway.HNClient.Models;
    using HealthGateway.HNClient.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The HNClient controller enabling secure web access to HNClient.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [ApiController]
    public class HNClientController : ControllerBase
    {
        private readonly IHNClientService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="HNClientController"/> class.
        /// </summary>
        /// <param name="svc">The injected HNClient Proxy service.</param>
        public HNClientController(IHNClientService svc)
        {
            this.service = svc;
        }

        /// <summary>
        /// Sends the HL7 V2.3 message to HNClient.
        /// </summary>
        /// <returns>A structured Message object.</returns>
        /// <param name="msg">The HL7 V2.3 formatted message.</param>
        /// <response code="200">Returns the HL7 Response message.</response>
        /// <response code="401">The client is not authorized to call SendMessage.</response>
        [HttpPost]
        [Produces("application/json")]
        [Authorize]
        public HNMessage SendMessage(HNMessage msg)
        {
            return this.service.SendMessage(msg);
        }

        /// <summary>
        /// Performs a time transaction for verification purposes.
        /// </summary>
        /// <returns>a Structured TimeMessage object.</returns>
        /// <response code="200">Returns a message containing the DateTime from HNSecure.</response>
        [HttpGet]
        [Produces("application/json")]
        public HNMessage GetTime()
        {
            return this.service.GetTime();
        }
    }
}
