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
namespace HealthGateway.Common.Data.Extensions
{
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Extension methods for <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns a value that indicates whether the entity (user) represented by this claims principal is in any of the
        /// specified roles.
        /// </summary>
        /// <param name="user">The claims principal in question.</param>
        /// <param name="roles">The roles for which to check.</param>
        /// <returns>true if claims principal is in any of the specified roles; otherwise, false.</returns>
        public static bool IsInAnyRole(this ClaimsPrincipal? user, params string[] roles)
        {
            return user != null && roles.Any(user.IsInRole);
        }
    }
}
