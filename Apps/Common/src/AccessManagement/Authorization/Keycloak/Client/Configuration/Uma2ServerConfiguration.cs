//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License")];
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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Class that represents the user model in the UMA2 Configuration as fetched from the well-known endpoint.
    /// </summary>
    public class Uma2ServerConfiguration
    {
        /// <summary>
        /// Gets or sets the user issuer.
        /// </summary>
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the authorization_endpoint.
        /// </summary>
        [JsonPropertyName("authorization_endpoint")]
        public String AuthorizationEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token_endpoint.
        /// </summary>
        [JsonPropertyName("token_endpoint")]
        public String TokenEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the introspection_endpoint.
        /// </summary>
        [JsonPropertyName("introspection_endpoint")]
        public string IntrospectionEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the userinfo_endpoint.
        /// </summary>
        [JsonPropertyName("userinfo_endpoint")]
        public String UserinfoEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the end_session_endpoint.
        /// </summary>
        [JsonPropertyName("end_session_endpoint")]
        public String LogoutEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the jwks_uri.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public String JwksUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the check_session_iframe.
        /// </summary>
        [JsonPropertyName("check_session_iframe")]
        public String CheckSessionIframe { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the grant_types_supported.
        /// </summary>
        [JsonPropertyName("grant_types_supported")]
        public List<string> GrantTypesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the response_types_supported.
        /// </summary>
        [JsonPropertyName("response_types_supported")]
        public List<string> ResponseTypesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the subject_types_supported.
        /// </summary>
        [JsonPropertyName("subject_types_supported")]
        public List<string> SubjectTypesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the id_token_signing_alg_values_supported.
        /// </summary>
        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public List<string> IdTokenSigningAlgValuesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the userinfo_signing_alg_values_supported.
        /// </summary>
        [JsonPropertyName("userinfo_signing_alg_values_supported")]
        public List<string> UserInfoSigningAlgValuesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the request_object_signing_alg_values_supported.
        /// </summary>
        [JsonPropertyName("request_object_signing_alg_values_supported")]
        public List<string> RequestObjectSigningAlgValuesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the response_modes_supported.
        /// </summary>
        [JsonPropertyName("response_modes_supported")]
        public List<string> ResponseModesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the registration_endpoint.
        /// </summary>
        [JsonPropertyName("registration_endpoint")]
        public string RegistrationEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token_endpoint_auth_methods_supported.
        /// </summary>
        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public List<string> TokenEndpointAuthMethodsSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the token_endpoint_auth_signing_alg_values_supported.
        /// </summary>
        [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
        public List<string> TokenEndpointAuthSigningAlgValuesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the claims_supported.
        /// </summary>
        [JsonPropertyName("claims_supported")]
        public List<string> ClaimsSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the claim_types_supported.
        /// </summary>
        [JsonPropertyName("claim_types_supported")]
        public List<string> ClaimTypesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the claims_parameter_supported.
        /// </summary>
        [JsonPropertyName("claims_parameter_supported")]
        public bool ClaimsParameterSupported { get; set; } = false;

        /// <summary>
        /// Gets or sets the scopes_supported.
        /// </summary>
        [JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the request_parameter_supported.
        /// </summary>
        [JsonPropertyName("request_parameter_supported")]
        public bool RequestParameterSupported  { get; set; } = false;

        /// <summary>
        /// Gets or sets the request_uri_parameter_supported.
        /// </summary>
        [JsonPropertyName("request_uri_parameter_supported")]
        public bool RequestUriParameterSupported { get; set; } = false;

        /// <summary>
        /// Gets or sets the resource_registration_endpoint.
        /// </summary>
        [JsonPropertyName("resource_registration_endpoint")]
        public string ResourceRegistrationEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permission_endpoint.
        /// </summary>
        [JsonPropertyName("permission_endpoint")]
        public string PermissionEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the policy_endpoint.
        /// </summary>
        [JsonPropertyName("policy_endpoint")]
        public string PolicyEndpoint { get; set; } = string.Empty;
    }
}