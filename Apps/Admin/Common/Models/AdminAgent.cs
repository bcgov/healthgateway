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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using System.Collections.Generic;
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Model representing a user of the admin website.
    /// </summary>
    public class AdminAgent
    {
        /// <summary>
        /// Gets the agent's unique account identifier.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Gets or sets the agent's username.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agent's identity provider.
        /// </summary>
        public KeycloakIdentityProvider IdentityProvider { get; set; } = KeycloakIdentityProvider.Unknown;

        /// <summary>
        /// Gets the roles assigned to the agent.
        /// </summary>
        public ISet<IdentityAccessRole> Roles { get; init; } = new HashSet<IdentityAccessRole>();
    }
}
