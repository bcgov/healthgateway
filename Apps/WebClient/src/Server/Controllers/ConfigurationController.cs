﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.WebClient.Server.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using HealthGateway.WebClient.Server.Models;
    using HealthGateway.WebClient.Server.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to return Health Gateway configuration to approved clients.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [Produces("application/json")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfigurationService configurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
        /// </summary>
        /// <param name="service">The injected configuration service provider.</param>
        public ConfigurationController(IConfigurationService service)
        {
            this.configurationService = service;
        }

        /// <summary>
        /// Returns the external Health Gateway configuration.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The Health Gateway WebClient Configuration.</returns>
        [HttpGet]
        public async Task<ExternalConfiguration> Index(CancellationToken ct)
        {
            ExternalConfiguration config = await this.configurationService.GetConfigurationAsync(ct);
            config.WebClient.ClientIp = this.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            return config;
        }

        /// <summary>
        /// Returns the Mobile Health Gateway configuration.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The Health Gateway Mobile Configuration.</returns>
        [HttpGet]
        [Route("~/MobileConfiguration")]
        public async Task<MobileConfiguration> MobileConfiguration(CancellationToken ct)
        {
            return await this.configurationService.GetMobileConfigurationAsync(ct);
        }
    }
}
