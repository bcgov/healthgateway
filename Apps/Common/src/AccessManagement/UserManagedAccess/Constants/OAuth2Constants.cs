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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Constants
{
    /// <summary> Keycloak's OAuth 2.0 constant values.</summary>
    public static class OAuth2Constants
    {
        /// <summary>The access_token label.</summary>
        public const string AccessToken = "access_token";

        /// <summary>The access token type.</summary>
        public const string AccessTokenType = "urn:ietf:params:oauth:token-type:access_token";

        /// <summary>The acr_values label.</summary>
        public const string AcrValues = "acr_values";

       /// <summary>The audience label.</summary>
        public const string Audience = "audience";

        /// <summary>The authorization_code label.</summary>
        public const string AuthorizationCode = "authorization_code";

        /// <summary>The client_assertion label.
        /// See https://tools.ietf.org/html/draft-jones-oauth-jwt-bearer-03#section-2.2.</summary>
        public const string ClientAssertion = "client_assertion";

        /// <summary>The client_assertion_type label.
        /// See https://tools.ietf.org/html/draft-ietf-oauth-assertions-01#page-5.</summary>
        public const string ClientAssertionType = "client_assertion_type";

        /// <summary>The client_assertion_type JWT label.
        /// See https://tools.ietf.org/html/draft-ietf-oauth-assertions-01#page-5.</summary>
        public const string ClientAssertionTypeJwt = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

        /// <summary>The client_credentials label.</summary>
        public const string ClientCredentials = "client_credentials";

        /// <summary>The client_id label.</summary>
        public const string ClientId = "client_id";

        /// <summary>The client_secret label.</summary>
        public const string ClientSecret = "client_secret";

        /// <summary>The code label.</summary>
        public const string Code = "code";

        /// <summary>The code_challenge label.</summary>
        public const string CodeChallenge = "code_challenge";

        /// <summary>The code_challenge_method label.</summary>
        public const string CodeChallengeMethod = "code_challenge_method";

        /// <summary>The code_verifier label.
        /// See https://tools.ietf.org/html/rfc7636#section-6.1.</summary>
        public const string CodeVerifier = "code_verifier";

        /// <summary>The display label.</summary>
        public const string Display = "display";

        /// <summary>The display console.</summary>
        public const string DisplayConsole = "console";

        /// <summary>The error label.</summary>
        public const string Error = "error";

        /// <summary>The error_description label.</summary>
        public const string ErrorDescription = "error_description";

        /// <summary>The grant_type label.</summary>
        public const string GrantType = "grant_type";

        /// <summary>The id_token label.</summary>
        public const string IdToken = "id_token";

        /// <summary>The ID token type.</summary>
        public const string IdTokenType = "urn:ietf:params:oauth:token-type:id_token";

        /// <summary>The implicit label.</summary>
        public const string Implicit = "implicit";

        /// <summary>The issued_token_type label.</summary>
        public const string IssuedTokenType = "issued_token_type";

        /// <summary>The JWT label for OIDC Session Management.</summary>
        public const string Jwt = "JWT";

        /// <summary>The JWT token type.</summary>
        public const string JwtTokenType = "urn:ietf:params:oauth:token-type:jwt";

        /// <summary>The logout_token label.</summary>
        public const string LogoutToken = "logout_token";

        /// <summary>The max_age label.</summary>
        public const string MaxAge = "max_age";

        /// <summary>The offline_access label.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#OfflineAccess.</summary>
        public const string OfflineAccess = "offline_access";

        /// <summary>The password label.</summary>
        public const string Password = "password";

       /// <summary>The plain PKCE label.
        /// See https://tools.ietf.org/html/rfc7636#section-6.2.2.</summary>
        public const string PkceMethodPlain = "plain";

        /// <summary>The S256 PKCE label.
        /// See https://tools.ietf.org/html/rfc7636#section-6.2.2.</summary>
        public const string PkceMethodS256 = "S256";

        /// <summary>The prompt label.</summary>
        public const string Prompt = "prompt";

        /// <summary>The redirect_uri label.</summary>
        public const string RedirectUri = "redirect_uri";

        /// <summary>The refresh_token label.</summary>
        public const string RefreshToken = "refresh_token";

        /// <summary>The refresh token type.</summary>
        public const string RefreshTokenType = "urn:ietf:params:oauth:token-type:refresh_token";

        /// <summary>The response_type label.</summary>
        public const string ResponseType = "response_type";

        /// <summary>The requested_issuer label.</summary>
        public const string RequestedIssuer = "requested_issuer";

        /// <summary>The requested_subject label.</summary>
        public const string RequestedSubject = "requested_subject";

        /// <summary>The requested_token_type label.</summary>
        public const string RequestedTokenType = "requested_token_type";

        /// <summary>The SAML 2 token type.</summary>
        public const string Saml2TokenType = "urn:ietf:params:oauth:token-type:saml2";

        /// <summary>The scope label.</summary>
        public const string Scope = "scope";

        /// <summary>The address scope.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims.</summary>
        public const string ScopeAddress = "address";

        /// <summary>The email scope.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims.</summary>
        public const string ScopEmail = "email";

        /// <summary>The openid scope.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#AuthRequest.</summary>
        public const string ScopeOpenid = "openid";

        /// <summary>The phone scope.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims.</summary>
        public const string ScopePhone = "phone";

        /// <summary>The profile scope.
        /// See http://openid.net/specs/openid-connect-core-1_0.html#ScopeClaims.</summary>
        public const string ScopeProfile = "profile";

        /// <summary>The session_state labe for OIDC Session Management.</summary>
        public const string SessionState = "session_state";

        /// <summary>The state label.</summary>
        public const string State = "state";

        /// <summary>The subject_issuer label.</summary>
        public const string SubjectIssuer = "subject_issuer";

        /// <summary>The subject_token label.</summary>
        public const string SubjectToken = "subject_token";

        /// <summary>The subject_token_type label.</summary>
        public const string SubjectTokenType = "subject_token_type";

       /// <summary>The token exchange grant type.</summary>
        public const string TokenExchangeGrantType = "urn:ietf:params:oauth:grant-type:token-exchange";

        /// <summary>A Tokenn Introspectiion Hint</summary>
        public const string TokenTypeHint = "token_type_hint";

        /// <summary>A Requesting Party Token Hint Value</summary>
        public const string TokenTypeHintRpt = "requesting_party_token";

       /// <summary>The ui_locales label.</summary>
        public const string UiLocalesParam = "ui_locales";

        /// <summary>The UMA ticket token type.</summary>
        public const string UmaGrantType = "urn:ietf:params:oauth:grant-type:uma-ticket";

        /// <summary>The username label.</summary>
        public const string Username = "username";
    }
}
