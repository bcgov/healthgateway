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
    public static class OAuth2Constants
    {

        public const string CODE = "code";

        public const string CLIENT_ID = "client_id";

        public const string CLIENT_SECRET = "client_secret";

        public const string ERROR = "error";

        public const string ERROR_DESCRIPTION = "error_description";

        public const string REDIRECT_URI = "redirect_uri";

        public const string DISPLAY = "display";

        public const string SCOPE = "scope";

        public const string STATE = "state";

        public const string GRANT_TYPE = "grant_type";

        public const string RESPONSE_TYPE = "response_type";

        public const string ACCESS_TOKEN = "access_token";

        public const string ID_TOKEN = "id_token";

        public const string REFRESH_TOKEN = "refresh_token";

        public const string LOGOUT_TOKEN = "logout_token";

        public const string AUTHORIZATION_CODE = "authorization_code";


        public const string IMPLICIT = "implicit";

        public const string USERNAME = "username";

        public const string PASSWORD = "password";

        public const string CLIENT_CREDENTIALS = "client_credentials";

        // https://tools.ietf.org/html/draft-ietf-oauth-assertions-01#page-5
        public const string CLIENT_ASSERTION_TYPE = "client_assertion_type";
        public const string CLIENT_ASSERTION = "client_assertion";

        // https://tools.ietf.org/html/draft-jones-oauth-jwt-bearer-03#section-2.2
        public const string CLIENT_ASSERTION_TYPE_JWT = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

        // http://openid.net/specs/openid-connect-core-1_0.html#OfflineAccess
        public const string OFFLINE_ACCESS = "offline_access";

        // http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
        public const string SCOPE_OPENID = "openid";

        // http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims
        public const string SCOPE_PROFILE = "profile";
        public const string SCOPE_EMAIL = "email";
        public const string SCOPE_ADDRESS = "address";
        public const string SCOPE_PHONE = "phone";

        public const string UI_LOCALES_PARAM = "ui_locales";

        public const string PROMPT = "prompt";
        public const string ACR_VALUES = "acr_values";

        public const string MAX_AGE = "max_age";

        // OIDC Session Management
        public const string SESSION_STATE = "session_state";

        public const string JWT = "JWT";

        // https://tools.ietf.org/html/rfc7636#section-6.1
        public const string CODE_VERIFIER = "code_verifier";
        public const string CODE_CHALLENGE = "code_challenge";
        public const string CODE_CHALLENGE_METHOD = "code_challenge_method";

        // https://tools.ietf.org/html/rfc7636#section-6.2.2
        public const string PKCE_METHOD_PLAIN = "plain";
        public const string PKCE_METHOD_S256 = "S256";

        public const string TOKEN_EXCHANGE_GRANT_TYPE = "urn:ietf:params:oauth:grant-type:token-exchange";
        public const string AUDIENCE = "audience";
        public const string REQUESTED_SUBJECT = "requested_subject";
        public const string SUBJECT_TOKEN = "subject_token";
        public const string SUBJECT_TOKEN_TYPE = "subject_token_type";
        public const string REQUESTED_TOKEN_TYPE = "requested_token_type";
        public const string ISSUED_TOKEN_TYPE = "issued_token_type";
        public const string REQUESTED_ISSUER = "requested_issuer";
        public const string SUBJECT_ISSUER = "subject_issuer";
        public const string ACCESS_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:access_token";
        public const string REFRESH_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:refresh_token";
        public const string JWT_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:jwt";
        public const string ID_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:id_token";
        public const string SAML2_TOKEN_TYPE = "urn:ietf:params:oauth:token-type:saml2";

        public const string UMA_GRANT_TYPE = "urn:ietf:params:oauth:grant-type:uma-ticket";


        public const string DISPLAY_CONSOLE = "console";
    }
}


