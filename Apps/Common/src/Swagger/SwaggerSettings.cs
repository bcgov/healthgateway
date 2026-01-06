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
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.OpenApi;

    /// <summary>
    /// Swagger Configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SwaggerSettings
    {
        /// <summary>
        /// Gets or sets document Name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets swagger Info.
        /// </summary>
        public OpenApiInfo? Info { get; set; }

        /// <summary>
        /// Gets or sets RoutePrefix.
        /// </summary>
        public string? RoutePrefix { get; set; }

        /// <summary>
        /// Gets or sets RoutePrefix.
        /// </summary>
        public string? RouteTemplatePrefix { get; set; } = "swagger";

        /// <summary>
        /// Gets or sets BasePath.
        /// </summary>
        public string? BasePath { get; set; }

        /// <summary>
        /// Gets Route Prefix with tailing slash.
        /// </summary>
        public string RoutePrefixWithSlash =>
            string.IsNullOrWhiteSpace(this.RoutePrefix)
                ? string.Empty
                : this.RoutePrefix + "/";
    }
}
