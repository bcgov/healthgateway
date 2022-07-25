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
namespace HealthGateway.Admin.Client.Authorization
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
    using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

    /// <summary>
    /// Maps the OIDC roles.
    /// </summary>
    public class RolesClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RolesClaimsPrincipalFactory"/> class.
        /// </summary>
        /// <param name="accessor">The Access Token Provider Accessor.</param>
        public RolesClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
            : base(accessor)
        {
        }

        /// <inheritdoc/>
        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
            RemoteUserAccount account,
            RemoteAuthenticationUserOptions options)
        {
            ClaimsPrincipal? user = await base.CreateUserAsync(account, options).ConfigureAwait(true);
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                return user;
            }

            ClaimsIdentity identity = (ClaimsIdentity)user.Identity;
            IEnumerable<Claim>? roleClaims = identity.FindAll(claim => claim.Type == "roles");
            if (roleClaims == null || !roleClaims.Any())
            {
                return user;
            }

            foreach (Claim existingClaim in roleClaims.ToArray())
            {
                identity.RemoveClaim(existingClaim);
            }

            object? rolesElem = account.AdditionalProperties["roles"];
            if (rolesElem is not JsonElement roles)
            {
                return user;
            }

            if (roles.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement role in roles.EnumerateArray())
                {
                    identity.AddClaim(new Claim(options.RoleClaim, role.GetString()));
                }
            }
            else
            {
                identity.AddClaim(new Claim(options.RoleClaim, roles.GetString()));
            }

            return user;
        }
    }
}
