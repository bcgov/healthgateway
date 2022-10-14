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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models.Tokens
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary>UMA 2.0 Permission Ticket.</summary>
    public class PermissionTicket
    {
        /// <summary>Gets or sets the permssion ticket ID.</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>Gets or sets the permssion ticket owner.</summary>
        [JsonPropertyName("owner")]
        public string? Owner { get; set; }

        /// <summary>Gets or sets the permssion ticket resource.</summary>
        [JsonPropertyName("resource")]
        public string? Resource { get; set; }

        /// <summary>Gets or sets the permssion ticket scope.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>Gets or sets a value indicating whether the permssion ticket is granted.</summary>
        [JsonPropertyName("granted")]
        public bool Granted { get; set; }

        /// <summary>Gets or sets the permssion ticket scope name.</summary>
        [JsonPropertyName("scopeName")]
        public string? ScopeName { get; set; }

        /// <summary>Gets or sets the permssion ticket resource name.</summary>
        [JsonPropertyName("resourceName")]
        public string? ResourceName { get; set; }

        /// <summary>Gets or sets the permssion ticket requester.</summary>
        [JsonPropertyName("requester")]
        public string? Requester { get; set; }

        /// <summary>Gets or sets the permssion ticket owner's name.</summary>
        [JsonPropertyName("ownerName")]
        public string? OwnerName { get; set; }

        /// <summary>Gets or sets the permssion ticket requester's name.</summary>
        [JsonPropertyName("requesterName")]
        public string? RequesterName { get; set; }
    }
}