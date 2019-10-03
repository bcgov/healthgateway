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
namespace HealthGateway.Medication.Services
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;

    /// <summary>
    /// The custom authorization service interface.
    /// </summary>
    public interface ICustomAuthorizationService
    {
        /// <summary>
        /// Checks if a user meets a specific requirement for the specified resource.
        /// </summary>
        /// <param name="user">The user to evaluate the policy against.</param>
        /// <param name="resource">The resource to evaluate the policy against.</param>
        /// <param name="operation">The requirement to evaluate the policy against.</param>
        /// <returns>A flag indicating whether requirement evaluation has succeeded or failed.</returns>
        Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, string resource, OperationAuthorizationRequirement operation);
    }
}
