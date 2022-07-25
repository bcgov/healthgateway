// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Common.AspNetConfiguration.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using HealthGateway.Common.Swagger;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provides ASP.Net Services related to OpenApi (Swagger) documentation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SwaggerDoc
    {
        /// <summary>
        /// Configures the swagger services.
        /// </summary>
        /// <param name="services">The service collection provider.</param>
        /// <param name="configuration">The configuration to use.</param>
        public static void ConfigureSwaggerServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SwaggerSettings>(configuration.GetSection(nameof(SwaggerSettings)));
            string xmlPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            Assembly callingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            // Calling Assembly (Core App) + References + Executing Assembly (Common) References
            string[] xmlDocs = new[] { callingAssembly.GetName() }
                .Union(callingAssembly.GetReferencedAssemblies())
                .Union(executingAssembly.GetReferencedAssemblies())
                .Select(a => Path.Combine(xmlPath, $"{a.Name}.xml"))
                .Where(File.Exists)
                .ToArray();

            services
                .AddApiVersionWithExplorer()
                .AddSwaggerOptions()
                .AddSwaggerGen(options => Array.ForEach(xmlDocs, d => options.IncludeXmlComments(d)));
        }

        /// <summary>
        /// Configures the app to use swagger.
        /// </summary>
        /// <param name="app">The application builder provider.</param>
        /// <param name="logger">The logger to use.</param>
        public static void UseSwagger(IApplicationBuilder app, ILogger logger)
        {
            logger.LogDebug("Use Swagger...");

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            app.UseSwaggerDocuments();
        }
    }
}
