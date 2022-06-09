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
namespace HealthGateway.Common.AccessManagement.Authorization.Claims
{
    /// <summary>
    /// Claims specific to Health Gateway for Shared Access from Resource Owner to a Relying Party.
    /// This is UMA 2.0. 
    /// </summary>
    public static class ResourceAuthorizationClaims
    {
        /// <summary>
        /// Policy claim representing the scopes the relying party has been assigned by the resource owner.
        /// </summary>
        public const string Scopes = "scopes";

        /// <summary>
        /// Policy claims requiring a resource id, which will resolve to a resource URI with Hdid as resource owner.
        /// </summary>
        public const string ResourceId = "rsid";

        /// <summary>
        /// Policy claims requiring a resource name.
        /// </summary>
        public const string ResourceName = "rsname";

        /// <summary>
        /// Policy claim for UMA Relying Party Token (RPT) authorization claim entry point.
        /// </summary>
        public const string Authorization = "authorization";

        /// <summary>
        /// Policy claim entry point UMA Relying Party Token (RPT) permissions claims.
        /// </summary>
        public const string Permissions = "permissions";

        /// <summary>
        /// Policy claim entry point Relying Party Token (RPT) roles claims.
        /// </summary>
        public const string Roles = "roles";
    }
}
