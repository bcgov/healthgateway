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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.Swagger;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerOptions>
    {
        private readonly SwaggerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
        /// </summary>
        /// <param name="settings">settings.</param>
        public ConfigureSwaggerOptions(IOptions<SwaggerSettings> settings)
        {
            this.settings = settings?.Value ?? new SwaggerSettings();
        }

        /// <inheritdoc/>
        public void Configure(SwaggerOptions options)
        {
            if (options != null)
            {
                options.RouteTemplate = this.settings.RouteTemplatePrefix + "/{documentName}/swagger.json";
                if (!string.IsNullOrEmpty(this.settings.BasePath))
                {
                    options.PreSerializeFilters.Add(
                        (swaggerDoc, httpReq) => swaggerDoc.Servers = new List<OpenApiServer>
                        {
                            new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{this.settings.BasePath}" },
                        });
                }
            }
        }
    }
}
