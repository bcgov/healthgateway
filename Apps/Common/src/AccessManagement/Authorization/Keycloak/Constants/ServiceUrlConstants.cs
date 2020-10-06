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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak
{
    public static class ServiceUrlConstants
    {

        public static string AUTH_PATH = "/realms/{realm-name}/protocol/openid-connect/auth";
        public static string TOKEN_PATH = "/realms/{realm-name}/protocol/openid-connect/token";
        public static string TOKEN_SERVICE_LOGOUT_PATH = "/realms/{realm-name}/protocol/openid-connect/logout";
        public static string ACCOUNT_SERVICE_PATH = "/realms/{realm-name}/account";
        public static string REALM_INFO_PATH = "/realms/{realm-name}";
        public static string CLIENTS_MANAGEMENT_REGISTER_NODE_PATH = "/realms/{realm-name}/clients-managements/register-node";
        public static string CLIENTS_MANAGEMENT_UNREGISTER_NODE_PATH = "/realms/{realm-name}/clients-managements/unregister-node";
        public static string JWKS_URL = "/realms/{realm-name}/protocol/openid-connect/certs";
        public static string DISCOVERY_URL = "/realms/{realm-name}/.well-known/openid-configuration";
        public static string AUTHZ_DISCOVERY_URL = "/realms/{realm-name}/.well-known/uma2-configuration";
    }
}