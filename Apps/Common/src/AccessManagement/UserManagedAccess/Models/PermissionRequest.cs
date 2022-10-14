//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
    using System.Text.Json.Serialization;

    /// <summary>
    /// UMA 2.0 PermissionRequest.
    /// </summary>
    public class PermissionRequest
    {
        /// <summary>Gets or sets the resource id.</summary>
        [JsonPropertyName("resource_id")]
        public string? ResourceId { get; set; }

        /// <summary>Gets the resource scopes.</summary>
        [JsonPropertyName("resource_scopes")]
        public List<string>? Scopes { get; } = new List<string>();

        /// <summary>Gets or sets the resource_server_id.</summary>
        [JsonPropertyName("resource_server_id")]
        public string? ResourceServerId { get; set; }

        /// <summary>Gets the claims.</summary>
        [JsonPropertyName("claims")]
        public Dictionary<string, List<string>>? Claims { get; } = new Dictionary<string, List<string>>();
    }
}