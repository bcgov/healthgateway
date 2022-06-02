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
namespace HealthGateway.Admin.Services
{
    using HealthGateway.Admin.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides external configuration data.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger logger;
        private readonly ExternalConfiguration config;

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
            this.config = new ExternalConfiguration();
            this.config = configuration.Get<ExternalConfiguration>();
        }

        /// <summary>
        /// Reads the external configuration and returns it to the caller.
        /// </summary>
        /// <returns>The external configuration data.</returns>
        public ExternalConfiguration GetConfiguration()
        {
            return this.config;
        }
    }
}
