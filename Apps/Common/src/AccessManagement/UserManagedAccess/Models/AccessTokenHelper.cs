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
namespace HealthGateway.Common.UserManagedAccess.Models
{
    using System.Collections.Generic;
    using namespace HealthGateway.Common.UserManagedAccess.Models.Tokens;

    /// <summary>Some handy extensions to the serializable AccessToken. These were part of AuthorizationContext.java in the original
    /// keycloak client library. </summary>
    public static class AccessTokenHelper
    {
        /// <summary>Checks whether the AccessToken has the permissions for the given resource and scope.</summary>
        /// <param name="accessToken">this.</param>
        /// <param name="resourceName">A resource name.</param>
        /// <param name="scopeName">A scope name.</param>
        /// <returns>Returns true if the AccessToken has the permissions for the scope and resourceName.</returns>
        public static bool HasPermission(this AccessToken accessToken, string? resourceName, string? scopeName)
        {
            ResourceAuthorization? authorization = accessToken.ResourceAuthorization;

            if (authorization != null)
            {
                foreach (Permission permission in authorization.Permissions!)
                {
                    if ((resourceName! == permission.ResourceName) || (resourceName! == permission.ResourceId))
                    {
                        if ((scopeName == null) || permission.Scopes!.Contains(scopeName))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>Checks whether the AccessToken has the permissions for the given resource.</summary>
        /// <param name="accessToken">this.</param>
        /// <param name="resourceName">A resource name.</param>
        /// <returns>Returns true if the AccessToken has the permissions for the resourceName.</returns>
        public static bool HasResourcePermission(this AccessToken accessToken, string resourceName)
        {
            return accessToken.HasPermission(resourceName, null);
        }

        /// <summary>Checks whether the AccessToken has the permissions for the given scope.</summary>
        /// <param name="accessToken">this.</param>
        /// <param name="scopeName">A resource name.</param>
        /// <returns>Returns true if the AccessToken has the permissions for the scopeName.</returns>
        public static bool HasScopePermission(this AccessToken accessToken, string scopeName)
        {
            ResourceAuthorization authorization = accessToken.ResourceAuthorization!;

            if (authorization != null)
            {
                foreach (Permission permission in authorization.Permissions!)
                {
                    if (permission.Scopes!.Contains(scopeName))
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>Checks whether the AccessToken has the permissions for the given resource and scope.</summary>
        /// <param name="accessToken">this.</param>
        /// <returns>Returns true if the AccessToken has the permissions for the scopeName.</returns>
        public static List<Permission> GetPermissions(this AccessToken accessToken)
        {
            ResourceAuthorization authorization = accessToken.ResourceAuthorization!;

            if (authorization == null)
            {
                return new List<Permission>();
            }

            List<Permission> list = authorization.Permissions!;

            return new List<Permission>(list);
        }
    }
}