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
namespace HealthGateway.Admin.Controllers
{
    using HealthGateway.Admin.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web API to return Health Gateway configuration to approved clients.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationService configservice;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationController"/> class.
        /// </summary>
        /// <param name="service">The injected configuration service provider.</param>
        public ConfigurationController(IConfigurationService service)
        {
            this.configservice = service;
        }

        /// <summary>
        /// Returns the external Health Gateway configuration.
        /// </summary>
        /// <returns>The Health Gatway Configuration.</returns>
        [HttpGet]
        public Models.ExternalConfiguration Index()
        {
            return this.configservice.GetConfiguration();
        }
    }
}
