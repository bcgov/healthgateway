// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Utils
{
    using HealthGateway.Admin.Common.Constants;

    /// <summary>
    /// Utilities for interacting with admin agents.
    /// </summary>
    public static class AgentUtility
    {
        /// <summary>
        /// Returns the formatted representation of an agent's identity provider.
        /// </summary>
        /// <param name="identityProvider">The agent's identity provider.</param>
        /// <returns>A string containing the formatted representation of a agent's identity provider.</returns>
        public static string FormatKeycloakIdentityProvider(KeycloakIdentityProvider identityProvider)
        {
            return identityProvider switch
            {
                KeycloakIdentityProvider.Idir => "IDIR",
                KeycloakIdentityProvider.PhsaAzure => "PHSA",
                _ => identityProvider.ToString(),
            };
        }

        /// <summary>
        /// Returns the agent's identity provider.
        /// </summary>
        /// <param name="identityProviderName">The agent's identity provider name from KeyCloak.</param>
        /// <returns>The agent's identity provider.</returns>
        public static KeycloakIdentityProvider MapKeycloakIdentityProvider(string identityProviderName)
        {
            return identityProviderName switch
            {
                "idir" => KeycloakIdentityProvider.Idir,
                "phsaazure" => KeycloakIdentityProvider.PhsaAzure,
                _ => KeycloakIdentityProvider.Unknown,
            };
        }
    }
}
