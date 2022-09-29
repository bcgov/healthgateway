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
namespace HealthGateway.Common.UserManagedAccess.Models.Tokens
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json.Serialization;

    /// <summary>
    /// OAuth 2.0 Access part of AccessToken.
    /// </summary>
    public class Access
    {
        /// <summary>
        /// Gets the roles.
        /// </summary>
        [JsonPropertyName("roles")]
        public List<string> Roles { get; } = new List<string>();

        /// <summary>
        /// Gets a value indicating whether verify_caller flag is set.
        /// </summary>
        [JsonPropertyName("verify_caller")]
        public bool VerifyCaller { get; }

        /// <summary>
        /// Checks if the Access Token has the specified role.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <returns>A boolean whether the user is in role.</returns>
        public bool IsUserInRole(string role)
        {
            return this.Roles.Contains(role);
        }
    }
}