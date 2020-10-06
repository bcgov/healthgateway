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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Configuration
{
    /// <summary>
    /// Interface that represents the OpenId Configuration model for the Keycloak Configuration.
    /// </summary>
    public interface IKeycloakConfiguration
    {

        ///<summary>Gets or sets a single valid audience value for any received OpenIdConnect token.
        /// This value is passed into TokenValidationParameters.ValidAudience if that property is empty.
        /// Value:  The expected audience for any received OpenIdConnect token.</summary>
        public string Audience { get; set; }

        ///<summary>Gets or sets the Keycloak Authorization Server Url.</summary>
        public string AuthServerUrl { get; set; }

        /// <summary>Gets or set the Keycloak Realm name.</summary>
        public string Realm { get; set; }
    }
}