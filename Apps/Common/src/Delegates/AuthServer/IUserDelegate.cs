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
namespace HealthGateway.Common.Delegates.AuthServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models.AuthServer;

    /// <summary>
    /// The UserDelegate IAM Admin Service interface.
    /// </summary>
    public interface IUserDelegate
    {
        /// <summary>
        /// Look up a user by username.
        /// </summary>
        /// <param name="username">The username of the User account in the Authorization Server.</param>
        /// <param name="authorization">Authorization JWT for the call.</param>
        /// <returns>A candidate result list of UserRepresentation objects.</returns>
        Task<List<UserRepresentation>> FindUser(string username, string authorization);

        /// <summary>
        /// Look up a user by username.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="authorization">Authorization for the call.</param>
        /// <returns>A resulting UserRepresentation object.</returns>
        Task<UserRepresentation> GetUser(string userId, string authorization);

        /// <summary>
        /// Delete a User from teh IAM system.
        /// </summary>
        /// <param name="userId">The unique userId (surrogate key) of the User account in Authorization Server.</param>
        /// <param name="authorization">Authorization for the call.</param>
        /// <returns></returns>
        Task<int> DeleteUser(string userId, string authorization);
    }
}

