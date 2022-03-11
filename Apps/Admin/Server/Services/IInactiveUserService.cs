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
namespace HealthGateway.Admin.Server.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Server.Models;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that provides functionality to extract inactive users.
    /// </summary>
    public interface IInactiveUserService
    {
        /// <summary>
        /// Returns inactive users exclusive of the days inactive.
        /// </summary>
        /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
        /// <param name="timeOffset">The clients offset to get to UTC.</param>
        /// <returns>returns a Request Result of List.</returns>
        Task<RequestResult<List<AdminUserProfileView>>> GetInactiveUsers(int inactiveDays, int timeOffset);
    }
}
