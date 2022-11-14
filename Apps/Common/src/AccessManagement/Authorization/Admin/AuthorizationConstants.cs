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
namespace HealthGateway.Common.AccessManagement.Authorization.Admin
{
    /// <summary>
    /// A class with constants representing the various authorization settings not needing to be configurable.
    /// </summary>
    public static class AuthorizationConstants
    {
        /// <summary>
        /// Represents the relative redirect to initiate authentication and authorization challenge.
        /// </summary>
        public const string LoginPath = "/login";

        /// <summary>
        /// Represents the relative redirect to end authentication and authorization.
        /// </summary>
        public const string LogoutPath = "/logout";

        /// <summary>
        /// Represents the name of the auth cookie.
        /// </summary>
        public const string CookieName = "HealthGatewayAdmin";
    }
}
