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
namespace HealthGateway.Common.AccessManagement.Authorization.Keycloak.Client.Resource
{
    using System.Threading.Tasks;

    using HealthGateway.Common.AccessManagement.Authorization.Keycloak.Representation;
 
    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public interface IAuthorizationResource
    {
        /// <summary>Query the server for permissions given an <cref name="AuthorizationRequest"/>.</summary>
        /// <param name="request"> an <cref name="AuthorizationRequest"/></param>
        /// <param name="accessToken">A Base64 encoded OAuth 2.0 accessToken from an authentication event.</param>
        /// <returns>An <cref name="AuthorizationRequest"/>with a RPT holding all granted permissions.</returns>
        public Task<AuthorizationResponse> authorize(AuthorizationRequest request, string accessToken);

    }
}