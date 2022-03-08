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
namespace HealthGateway.WebClient.Services
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides external configuration data.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger logger;
        private readonly Models.ExternalConfiguration config;
        private readonly Models.MobileConfiguration mobileConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        public ConfigurationService(
            ILogger<ConfigurationService> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.config = new Models.ExternalConfiguration();
            this.config = configuration.Get<Models.ExternalConfiguration>();
            this.mobileConfig = new Models.MobileConfiguration();
            configuration.Bind("MobileConfiguration", this.mobileConfig);
        }

        /// <inheritdoc />
        public Models.ExternalConfiguration GetConfiguration()
        {
            this.logger.LogTrace($"Getting configuration data...");
            return this.config;
        }

        /// <inheritdoc />
        public Models.MobileConfiguration GetMobileConfiguration()
        {
            this.logger.LogTrace($"Getting mobile configuration data...");
            return this.mobileConfig;
        }
    }
}
