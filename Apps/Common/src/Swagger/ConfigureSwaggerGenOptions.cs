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
    using System.Diagnostics.CodeAnalysis;
    using Asp.Versioning.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <inheritdoc/>
    /// <summary>
    /// Implementation of IConfigureOptions&lt;SwaggerGenOptions&gt;.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider provider;
        private readonly SwaggerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigureSwaggerGenOptions"/> class.
        /// </summary>
        /// <param name="versionDescriptionProvider">IApiVersionDescriptionProvider.</param>
        /// <param name="swaggerSettings">App Settings for Swagger.</param>
        public ConfigureSwaggerGenOptions(
            IApiVersionDescriptionProvider versionDescriptionProvider,
            IOptions<SwaggerSettings> swaggerSettings)
        {
#pragma warning disable S3236
            Debug.Assert(versionDescriptionProvider != null, "The versionDescriptionProvider parameter cannot be null.");
            Debug.Assert(swaggerSettings != null, "The swaggerSettings parameter cannot be null.");
#pragma warning restore S3236

            this.provider = versionDescriptionProvider;
            this.settings = swaggerSettings.Value;
        }

        /// <inheritdoc/>
        public void Configure(SwaggerGenOptions options)
        {
            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            options.AddSecurityDefinition(
                "bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer",
                });

            // Add auth header filter
            options.OperationFilter<AuthenticationRequirementsOperationFilter>();

            this.AddSwaggerDocumentForEachDiscoveredApiVersion(options);
        }

        private void AddSwaggerDocumentForEachDiscoveredApiVersion(SwaggerGenOptions options)
        {
            foreach (ApiVersionDescription description in this.provider.ApiVersionDescriptions)
            {
                this.settings.Info!.Version = description.ApiVersion.ToString();

                if (description.IsDeprecated)
                {
                    this.settings.Info.Description = $"{this.settings.Info.Description} - DEPRECATED";
                }

                options.SwaggerDoc(description.GroupName, this.settings.Info);
            }
        }
    }
}
