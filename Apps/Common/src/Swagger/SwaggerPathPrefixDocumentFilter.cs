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
namespace HealthGateway.Common.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Swagger;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Experimental: Processes the Swagger docs and updates the path with a static path prefix.
    /// </summary>
    public class SwaggerPathPrefixDocumentFilter : IDocumentFilter
    {
        private readonly string pathPrefix;
        private readonly ILogger<SwaggerPathPrefixDocumentFilter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerPathPrefixDocumentFilter"/> class.
        /// </summary>
        /// <param name="prefix">The prefix to use.</param>
        /// <param name="logger">The logger to use.</param>
        public SwaggerPathPrefixDocumentFilter(string prefix, ILogger<SwaggerPathPrefixDocumentFilter> logger)
        {
            this.pathPrefix = prefix;
            this.logger = logger;
        }

        /// <summary>
        /// Applies the document filter to the Swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The document filter context.</param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            Contract.Requires(swaggerDoc != null);
            IEnumerator<string> e = swaggerDoc.Paths.Keys.GetEnumerator();
            while (e.MoveNext())
            {
                string path = e.Current;
                this.logger.LogInformation($"Path: {path}");
                var pathToChange = swaggerDoc.Paths[path];
                swaggerDoc.Paths.Remove(path);
                swaggerDoc.Paths.Add(new KeyValuePair<string, PathItem>("/" + this.pathPrefix + path, pathToChange));
            }
        }
    }
}
