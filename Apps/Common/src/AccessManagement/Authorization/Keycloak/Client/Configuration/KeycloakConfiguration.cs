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

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Class that represents the OpenId Configuration model for the Keycloak Configuration.
    /// </summary>
    public class KeycloakConfiguration : IKeycloakConfiguration
    {
        private readonly IConfiguration configuration;

        private static string ConfigurationSectionKey = "Keycloak";

        ///<inherited/>
        public string Audience { get; set; } = string.Empty;

        ///<inherited/>
        public string AuthServerUrl { get; set; } = string.Empty;

        ///<inherited/>
        public string Realm { get; set; } = string.Empty;

        /// <summary>Creates a new KeycloakConfiguration instance.</summary>
        /// <param name="configuration">The injected <cref name="IConfiguration"/> configuration object.</param>
        public KeycloakConfiguration(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configuration.Bind(ConfigurationSectionKey, this);
        }
    }
}