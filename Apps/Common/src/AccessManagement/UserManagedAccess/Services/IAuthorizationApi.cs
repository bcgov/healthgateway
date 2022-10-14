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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Services
{
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.UserManagedAccess.Models;
    using Refit;

    /// <summary>
    /// An entry point for obtaining permissions from the server.
    /// </summary>
    public interface IAuthorizationApi
    {
        /// <summary>Query the server for permissions given an <see cref="AuthorizationRequest"/>.</summary>
        /// <param name="request">A <see cref="AuthorizationRequest"/> instance.</param>
        /// <returns>An <see cref="AuthorizationRequest"/>with a RPT holding all granted permissions.</returns>
        [Post("/protocol/openid-connect/auth")]
        public Task<AuthorizationResponse> Authorize([Body] AuthorizationRequest request);
    }
}