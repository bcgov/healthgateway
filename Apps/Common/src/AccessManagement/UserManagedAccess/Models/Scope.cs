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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary>A bounded extent of access that is possible to perform on a resource set. In authorization policy terminology,
    /// a scope is one of the potentially many "verbs" that can logically apply to a resource set ("object").
    /// For more details, see "https://docs.kantarainitiative.org/uma/draft-oauth-resource-reg.html#rfc.section.2.1".</summary>.
    public class Scope
    {
        /// <summary>Gets or sets the scope ID.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the scope name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Gets or sets the scope icon URI.</summary>
        [JsonPropertyName("icon_uri")]
        public Uri? IconUri { get; set; }

        /// <summary>Gets the scope policies.</summary>
        [JsonPropertyName("policies")]
        public List<AccessPolicy>? Policies { get; } = new List<AccessPolicy>();

        /// <summary>Gets the scope resources.</summary>
        [JsonPropertyName("resources")]
        public List<Resource>? Resources { get; } = new List<Resource>();

        /// <summary>Gets or sets the scope displayName.</summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }
}
