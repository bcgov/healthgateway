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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 UMA Permission.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Gets or sets the resource identifier, i.e. the rsid.
        /// </summary>
        [JsonPropertyName("rsid")]
        public string ResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        [JsonPropertyName("rsname")]
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the scopes.
        /// </summary>
        [JsonPropertyName("scope")]
        public List<string>? Scopes { get; } = new List<string>();

        /// <summary>
        /// Gets the claims.
        /// </summary>
        [JsonPropertyName("claims")]
        public Dictionary<string, List<string>> Claims { get; } = new Dictionary<string, List<string>>();

        /// <summary>Convert the Permission object to its string representation.</summary>
        /// <returns>The converted string.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("Permission {");
            builder.Append("id=");
            builder.Append(this.ResourceId);
            builder.Append(", name=");
            builder.Append(this.ResourceName);
            builder.Append(", scopes=");
            builder.Append(this.Scopes);
            builder.Append('}');

            return builder.ToString();
        }
    }
}