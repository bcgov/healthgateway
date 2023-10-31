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
namespace HealthGateway.WebClient.Server.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using HealthGateway.WebClient.Server.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides external configuration data.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly ExternalConfiguration config;
        private readonly ILogger logger;
        private readonly MobileConfiguration mobileConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        public ConfigurationService(ILogger<ConfigurationService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.config = configuration.Get<ExternalConfiguration>() ?? new();

            if (!string.IsNullOrEmpty(this.config.WebClient.FeatureToggleFilePath))
            {
                string fileContent = File.ReadAllText(this.config.WebClient.FeatureToggleFilePath);
                this.config.WebClient.FeatureToggleConfiguration = JsonSerializer.Deserialize<FeatureToggleConfiguration>(
                    fileContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            this.mobileConfig = configuration.GetSection("MobileConfiguration").Get<MobileConfiguration>() ?? new();
            FeatureToggleConfiguration? featureToggleConfig = this.config.WebClient.FeatureToggleConfiguration;
            if (featureToggleConfig != null)
            {
                this.mobileConfig.Datasets = featureToggleConfig.Datasets.Where(d => d.Enabled).Select(d => d.Name);
                if (featureToggleConfig.Dependents.Enabled)
                {
                    this.mobileConfig.DependentDatasets = this.mobileConfig.Datasets
                        .Where(d => !Array.Exists(featureToggleConfig.Dependents.Datasets, dd => dd.Name == d && !dd.Enabled));
                }

                if (featureToggleConfig.Services.Enabled)
                {
                    this.mobileConfig.Services = featureToggleConfig.Services.Services.Where(s => s.Enabled).Select(s => s.Name);
                }
            }
        }

        /// <inheritdoc/>
        public ExternalConfiguration GetConfiguration()
        {
            this.logger.LogTrace("Getting configuration data...");
            return this.config;
        }

        /// <inheritdoc/>
        public MobileConfiguration GetMobileConfiguration()
        {
            this.logger.LogTrace("Getting mobile configuration data...");
            return this.mobileConfig;
        }
    }
}
