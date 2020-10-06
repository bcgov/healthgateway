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

    /// <summary>UMA 2.0 Permission Ticket</summary>
    public class PermissionTicket
    {
        /// <summary>Gets or sets the permssion ticket ID.</summary>
        public string? Id { get; set; }

        /// <summary>Gets or sets the permssion ticket onwer.</summary>
        public string? Owner { get; set; }

        /// <summary>Gets or sets the permssion ticket resource.</summary>
        public string? Resource { get; set; }

        /// <summary>Gets or sets the permssion ticket scope.</summary>
        public string? Scope { get; set; }

        /// <summary>Gets or sets whether the permssion ticket granted.</summary>
        public bool Granted { get; set; }

        /// <summary>Gets or sets the permssion ticket scope name.</summary>
        public string? ScopeName { get; set; }

        /// <summary>Gets or sets the permssion ticket resource name.</summary>
        public string? ResourceName { get; set; }

        /// <summary>Gets or sets the permssion ticket requester.</summary>
        public string? Requester { get; set; }

        /// <summary>Gets or sets the permssion ticket owner's name.</summary>
        public string? OwnerName { get; set; }

        /// <summary>Gets or sets the permssion ticket requester's name.</summary>
        public string? RequesterName { get; set; }
    }
}