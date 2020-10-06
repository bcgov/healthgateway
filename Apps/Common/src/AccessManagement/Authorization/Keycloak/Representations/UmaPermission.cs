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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary> A class representing a UMA 2.0 Permission.</summary>
    public class UmaPermission : AbstractPolicy
    {
        /// <summary>Gets or sets the UMA permssion roles.</summary>
        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }

        /// <summary>Gets or sets the UMA permssion groups.</summary>
        [JsonPropertyName("groups")]
        public List<string>? Groups { get; set; }

        /// <summary>Gets or sets the UMA permssion clients.</summary>
        [JsonPropertyName("clients")]
        public List<string>? Clients { get; set; }

        /// <summary>Gets or sets the UMA permssion users.</summary>
        [JsonPropertyName("users")]
        public List<string>? Users { get; set; }

        /// <summary>Gets or sets the UMA permssion condition.</summary>
        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        /// <summary>Adds a new client role to the the UMA permssion roles.</summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="roleName">The role Name.</param>
        public void AddClientRole(string clientId, string roleName)
        {
            Roles = (Roles == null) ? new List<string>() : Roles;
            Roles.Add(clientId + "/" + roleName);
        }

        /// <summary>Creates a new instance of <cref name="UmaPermission"/>.</summary>
        public UmaPermission()
        {
            this.Type = "uma";
        }
    }
}