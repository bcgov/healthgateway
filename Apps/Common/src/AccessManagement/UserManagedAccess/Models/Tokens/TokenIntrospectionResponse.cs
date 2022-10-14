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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models.Tokens
{
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An authorization response in form of an OAuth2 access token.
    /// </summary>
    public class TokenIntrospectionResponse : JwtPayload
    {
        /// <summary>Gets or sets a value indicating whether permissions are active.</summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; } = false;

        /// <summary>Gets a list of permissions.</summary>
        [JsonPropertyName("permissions")]
        public List<Permission> Permissions { get; } = new List<Permission>();
    }
}