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
namespace HealthGateway.Admin.Server.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.AccessManagement.Administration.Models;

    /// <summary>
    /// Utilities for mapping KeyCloak representations.
    /// </summary>
    public static class KeyCloakUtility
    {
        /// <summary>
        /// Returns a set of IdentityAccessRoles for a user.
        /// </summary>
        /// <param name="userRoles">A list of RoleRepresentations from KeyCloak.</param>
        /// <returns>A set of IdentityAccessRoles to match the user roles.</returns>
        public static ISet<IdentityAccessRole> MapIdentityAccessRoles(List<RoleRepresentation> userRoles)
        {
            HashSet<IdentityAccessRole> identityAccessRoles = new();

            foreach (RoleRepresentation userRole in userRoles)
            {
                IdentityAccessRole identityAccessRole = userRole.Name switch
                {
                    "AdminUser" => IdentityAccessRole.AdminUser,
                    "AdminReviewer" => IdentityAccessRole.AdminReviewer,
                    "SupportUser" => IdentityAccessRole.SupportUser,
                    _ => IdentityAccessRole.Unknown,
                };

                if (identityAccessRole != IdentityAccessRole.Unknown)
                {
                    identityAccessRoles.Add(identityAccessRole);
                }
            }

            return identityAccessRoles;
        }

        /// <summary>
        /// Removes any non-application roles and returns a pruned list of RoleRepresentations for a user.
        /// </summary>
        /// <param name="userRoles">A list of RoleRepresentations to be pruned.</param>
        /// <returns>A pruned list of RoleRepresentations to match the application roles.</returns>
        public static IEnumerable<RoleRepresentation> PruneUserRoles(IEnumerable<RoleRepresentation> userRoles)
        {
            return userRoles.Where(userRole => userRole.Name is "AdminUser" or "AdminReviewer" or "SupportUser").ToList();
        }
    }
}
