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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Util
{
    using System;
    using System.Net.Http;
    using System.Text;


    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client;

/// <summary>Helper class to build a URL from the well-known URL constant templates for Keycloak.</summary>
    public static class KeycloakUriBuilder
    {
        private const string REALM_NAME = "{realm-name}";
        public static string build(IKeycloakConfiguration configuration, string template)
        {
            string realm = configuration.Realm;
            string url = new string(configuration.AuthServerUrl);
            int idx = url.LastIndexOf("/");
            if (idx.Equals(url.Length-1))
            {
                url = url.Substring(0, idx);
            }
            url += template;
            url.Replace(REALM_NAME, realm);

            return url;
        }

        public static Uri buildUri(IKeycloakConfiguration configuration, string template)
        {
            return new Uri(build(configuration, template));
        }
    }
}