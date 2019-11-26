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
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Swagger;

    /// <inheritdoc />
    public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerOptions>
    {
#pragma warning disable CA1303 //Disable literals
        private readonly SwaggerSettings settings;
        private readonly ILogger<ConfigureSwaggerOptions> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="settings">settings.</param>
        /// <param name="logger">The logger to use.</param>
        public ConfigureSwaggerOptions(IOptions<SwaggerSettings> settings, ILogger<ConfigureSwaggerOptions> logger)
        {
            this.settings = settings?.Value ?? new SwaggerSettings();
            this.logger = logger;
        }

        /// <inheritdoc />
        public void Configure(SwaggerOptions options)
        {
            if (options != null)
            {
                this.logger.LogInformation($"RoutePrefix: {this.settings.RoutePrefixWithSlash}");
                options.RouteTemplate = this.settings.RoutePrefixWithSlash + "{documentName}/swagger.json";
            }
            else
            {
                this.logger.LogInformation("SwaggerOptions are null and will be defaults");
            }
        }
    }
}
