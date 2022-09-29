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
namespace Keycloak.Authorization
{
    /// <summary>Keycloak Service URL Constant template Url paths... well known Urls.</summary>
    public static class ServiceUrlConstants
    {
        /// <summary>The path to the authorization endpoint.</summary>
        public const string AuthPath = "/realms/{realm-name}/protocol/openid-connect/auth";

        /// <summary>The path to the token endpoint.</summary>
        public const string TokenPath = "/realms/{realm-name}/protocol/openid-connect/token";

        /// <summary>The path to the token logout endpoint.</summary>
        public const string TokenServiceLogoutPath = "/realms/{realm-name}/protocol/openid-connect/logout";

        /// <summary>The path to the account endpoint.</summary>
        public const string AccountServicePath = "/realms/{realm-name}/account";

        /// <summary>The path to the realm info endpoint.</summary>
        public const string RealmInfoPath = "/realms/{realm-name}";

        /// <summary>The path to the clients management register node endpoint.</summary>
        public const string ClientsManagementRegisterNodePath = "/realms/{realm-name}/clients-managements/register-node";

        /// <summary>The path to the clients management unregister node endpoint.</summary>
        public const string ClientsManagementUnRegisterNodePath = "/realms/{realm-name}/clients-managements/unregister-node";

        /// <summary>The path to the JWKS endpoint.</summary>
        public const string JwksUrl = "/realms/{realm-name}/protocol/openid-connect/certs";

        /// <summary>The path to the openid discovery configuration endpoint.</summary>
        public const string DiscoveryUrl = "/realms/{realm-name}/.well-known/openid-configuration";

        /// <summary>The path to the UMA 2.0 discovery configuration endpoint.</summary>
        public const string Uma2DiscoveryUrl = "/realms/{realm-name}/.well-known/uma2-configuration";
    }
}