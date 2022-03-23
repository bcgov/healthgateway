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
namespace HealthGateway.Common.AccessManagement.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.AccessManagement.Administration.Models;
    using HealthGateway.Common.AccessManagement.Authentication.Models;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// The UserDelegate IAM Admin Service interface.
    /// </summary>
    public interface IUserAdminDelegate
    {
        /// <summary>
        /// Look up a User account by username in the Identity and Access Management system.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="jwtModel">Json Web Token model for authenticating the call.</param>
        /// <returns>A resulting UserRepresentation object.</returns>
        UserRepresentation GetUser(Guid userId, JwtModel jwtModel);

        /// <summary>
        /// Returns users for the role passed in.
        /// </summary>
        /// <param name="role">The requested users role.</param>
        /// <param name="jwtModel">Json Web Token model for authenticating the call.</param>
        /// <returns>An Enumerable of UserRepresentation objects.</returns>
        Task<RequestResult<IEnumerable<UserRepresentation>>> GetUsers(IdentityAccessRole role, JwtModel jwtModel);

        /// <summary>
        /// Delete a User account from the Identity and Access Management system.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="jwtModel">Json Web Token model for authenticating the call.</param>
        /// <returns>Returns true when user deleted.</returns>
        bool DeleteUser(Guid userId, JwtModel jwtModel);
    }
}
