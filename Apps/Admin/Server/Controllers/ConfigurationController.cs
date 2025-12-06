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
namespace HealthGateway.Admin.Server.Controllers
{
    using System.Threading;
    using Asp.Versioning;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Server.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Web API to return Health Gateway configuration to approved clients.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/api/[controller]")]
    [Produces("application/json")]
    public class ConfigurationController : ControllerBase
    {
        /// <summary>
        /// Returns the external Health Gateway configuration.
        /// </summary>
        /// <param name="env">The host environment.</param>
        /// <param name="configurationService">The injected configuration provider.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The Health Gateway Configuration.</returns>
        [HttpGet]
        public ExternalConfiguration Index([FromServices] IHostEnvironment env, [FromServices] IConfigurationService configurationService, CancellationToken ct)
        {
            ExternalConfiguration externalConfig = configurationService.GetConfiguration(ct);
            externalConfig.Environment = env.EnvironmentName;
            externalConfig.ClientIp = this.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            return externalConfig;
        }
    }
}
