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
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 UMA Resource. Information about a user protected resource. A UMA Permission can be assigned to this entity.
    /// </summary>
    public class Resource
    {
        /// <summary>Gets or sets the resource identifier.</summary>
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the resource name.</summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>Gets the resource URIs.</summary>
        [JsonPropertyName("uris")]
        public List<string> Uris { get;  } = new List<string>();

        /// <summary>Gets or sets the resource icon URI.</summary>
        [JsonPropertyName("icon_uri")]
        public Uri? IconUri { get; set; }

         /// <summary>Gets or sets the resource owner.</summary>
        [JsonPropertyName("owner")]
        public ResourceOwner? Owner { get; set; }

         /// <summary>Gets or sets a value indicating whether the resource owner manages access.</summary>
        [JsonPropertyName("ownerManagedAccess")]
        public bool OwnerManagedAccess { get; set; }

         /// <summary>Gets or sets whether the resource display Name.</summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

         /// <summary>Gets the resource attributes.</summary>
        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>> Attributes { get; } = new Dictionary<string, List<string>>();
    }
}