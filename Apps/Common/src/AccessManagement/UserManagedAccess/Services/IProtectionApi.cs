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
namespace Keycloak.Client.Resource
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Keycloak.Client.Representation;

    ///
    /// <summary>An entry point for managing permission tickets using the Protection API.</summary>
    ///
    public interface IProtectionResource
    {
        /// <summary>Introspects the given rpt using the token introspection endpoint.</summary>
        /// <param name="rpt">the Requesting Party Token to Introspect.</param>
        /// <param name="token">The bearer token to use for authorization.</param>
        /// <returns>A TokenIntrospectionResponse.</returns>
        public Task<TokenIntrospectionResponse> IntrospectRequestingPartyToken(string rpt, string token);
    }
}