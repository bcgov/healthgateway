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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.WebClient.Server.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides external configuration data.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly IConfiguration configuration;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">Injected Configuration Provider.</param>
        public ConfigurationService(ILogger<ConfigurationService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<ExternalConfiguration> GetConfigurationAsync(CancellationToken ct = default)
        {
            ExternalConfiguration externalConfig = this.configuration.Get<ExternalConfiguration>() ?? new();
            externalConfig.WebClient.FeatureToggleConfiguration =
                await this.ReadFeatureToggleConfigurationAsync(externalConfig.WebClient.FeatureToggleFilePath, ct);

            return externalConfig;
        }

        /// <inheritdoc/>
        public async Task<MobileConfiguration> GetMobileConfigurationAsync(CancellationToken ct = default)
        {
            ExternalConfiguration externalConfig = this.configuration.Get<ExternalConfiguration>() ?? new();
            MobileConfiguration mobileConfig = this.configuration.GetSection("MobileConfiguration").Get<MobileConfiguration>() ?? new();
            FeatureToggleConfiguration? featureToggleConfig = await this.ReadFeatureToggleConfigurationAsync(externalConfig.WebClient.FeatureToggleFilePath, ct);

            if (featureToggleConfig != null)
            {
                mobileConfig.Datasets = featureToggleConfig.Datasets.Where(d => d.Enabled).Select(d => d.Name);
                if (featureToggleConfig.Dependents.Enabled)
                {
                    mobileConfig.DependentDatasets = mobileConfig.Datasets
                        .Where(d => !Array.Exists(featureToggleConfig.Dependents.Datasets, dd => dd.Name == d && !dd.Enabled));
                }

                if (featureToggleConfig.Services.Enabled)
                {
                    mobileConfig.Services = featureToggleConfig.Services.Services.Where(s => s.Enabled).Select(s => s.Name);
                }
            }

            return mobileConfig;
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Team decision")]
        private async Task<FeatureToggleConfiguration?> ReadFeatureToggleConfigurationAsync(string path, CancellationToken ct)
        {
            try
            {
                string fileContent = await File.ReadAllTextAsync(path, ct);
                return JsonSerializer.Deserialize<FeatureToggleConfiguration>(fileContent, SerializerOptions);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to read feature toggle configuration from path {Path}", path);
                return null;
            }
        }
    }
}
