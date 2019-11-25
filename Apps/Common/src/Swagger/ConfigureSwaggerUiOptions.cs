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
namespace HealthGateway.Common.Swagger
{
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.SwaggerUI;

    /// <inheritdoc cref="SwaggerUIOptions"/>>
    public sealed class ConfigureSwaggerUiOptions : IConfigureOptions<SwaggerUIOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;
        private readonly SwaggerSettings settings;
        private readonly ILogger<ConfigureSwaggerUiOptions> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerUiOptions"/> class.
        /// </summary>
        /// <param name="versionDescriptionProvider">versionDescriptionProvider.</param>
        /// <param name="settings">settings.</param>
        public ConfigureSwaggerUiOptions(IApiVersionDescriptionProvider versionDescriptionProvider, IOptions<SwaggerSettings> settings, ILogger<ConfigureSwaggerUiOptions> logger)
        {
            Debug.Assert(versionDescriptionProvider != null, $"{nameof(versionDescriptionProvider)} != null");
            Debug.Assert(settings != null, $"{nameof(versionDescriptionProvider)} != null");
            this.logger = logger;
            this.provider = versionDescriptionProvider;
            logger.LogDebug($"Need to instantiate setting: {settings?.Value == null}");
            this.settings = settings?.Value ?? new SwaggerSettings();
            
        }

        /// <summary>
        /// Configure.
        /// </summary>
        /// <param name="options">options.</param>
        public void Configure(SwaggerUIOptions options)
        {
            this.logger.LogDebug($"Route Prefix: {this.settings.RoutePrefixWithSlash}");
            if (options != null)
            {
                this.provider
                    .ApiVersionDescriptions
                    .ToList()
                    .ForEach(description =>
                    {
                        options.SwaggerEndpoint(
                            $"/{this.settings.RoutePrefixWithSlash}{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());

                        options.RoutePrefix = this.settings.RoutePrefix;
                    });
            }
        }
    }
}
