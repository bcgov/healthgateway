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
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json.Serialization;

    /// <summary>A PermissionTicket Token for UMA 2.0.</summary>
    public class PermissionTicketToken : JwtPayload
    {

        /// <summary>Initializes a new instance of the <see cref="PermissionTicketToken"/> class. </summary>
        public PermissionTicketToken()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PermissionTicketToken"/> class. </summary>
        /// <param name="permissions">A List of Permission objects.</param>
        public PermissionTicketToken(ICollection<Permission> permissions)
        {
            this.Permissions = permissions;
        }

        /// <summary>Gets the Permissions. </summary>
        [JsonPropertyName("permissions")]
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
