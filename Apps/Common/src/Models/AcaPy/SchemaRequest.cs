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
namespace HealthGateway.Common.Models.AcaPy
{
    using System.Collections.ObjectModel;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Schema Request to the aries agent.
    /// </summary>
    public class SchemaRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaRequest"/> class.
        /// </summary>
        public SchemaRequest()
        {
            this.Attributes = new Collection<string>();
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [JsonPropertyName("attributes")]
        public Collection<string> Attributes { get; }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        [JsonPropertyName("schema_name")]
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schema version.
        /// </summary>
        [JsonPropertyName("schema_version")]
        public string SchemaVersion { get; set; } = string.Empty;
    }
}
