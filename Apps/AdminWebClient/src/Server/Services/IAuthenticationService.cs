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
namespace HealthGateway.Admin.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Authentication and Authorization Service.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticates the request based on the current context.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The AuthData containing the token and user information.</returns>
        Task<AuthenticationData> GetAuthenticationDataAsync(CancellationToken ct = default);

        /// <summary>
        /// Clears the authorization data from the context.
        /// </summary>
        /// <returns>The signout confirmation followed by the redirect uri.</returns>
        SignOutResult Logout();

        /// <summary>
        /// Sets last login date time for admin user profile.
        /// </summary>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetLastLoginDateTimeAsync(CancellationToken ct = default);
    }
}
