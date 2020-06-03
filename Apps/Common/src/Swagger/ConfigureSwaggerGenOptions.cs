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
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <inheritdoc />
    /// <summary>
    /// Implementation of IConfigureOptions&lt;SwaggerGenOptions&gt;.
    /// </summary>
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
            IApiVersionDescriptionProvider versionDescriptionProvider, IOptions<SwaggerSettings> swaggerSettings)
        {
            Debug.Assert(versionDescriptionProvider != null, $"{nameof(versionDescriptionProvider)} != null");
            Debug.Assert(swaggerSettings != null, $"{nameof(swaggerSettings)} != null");

            this.provider = versionDescriptionProvider;
            this.settings = swaggerSettings?.Value ?? new SwaggerSettings();
        }

        /// <inheritdoc />
        public void Configure(SwaggerGenOptions options)
        {
            options.OperationFilter<SwaggerDefaultValues>();
            options.IgnoreObsoleteActions();
            options.IgnoreObsoleteProperties();

            this.AddSwaggerDocumentForEachDiscoveredApiVersion(options);
            SetCommentsPathForSwaggerJsonAndUi(options);
        }

        private static void SetCommentsPathForSwaggerJsonAndUi(SwaggerGenOptions options)
        {
            var xmlFile = $"{Assembly.GetEntryAssembly() !.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        }

        private void AddSwaggerDocumentForEachDiscoveredApiVersion(SwaggerGenOptions options)
        {
            foreach (var description in this.provider.ApiVersionDescriptions)
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