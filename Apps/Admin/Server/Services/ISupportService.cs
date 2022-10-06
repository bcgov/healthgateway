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
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that provides functionality to admin support.
    /// </summary>
    public interface ISupportService
    {
        /// <summary>
        /// Retrieves a list of message verifications matching the query.
        /// </summary>
        /// <param name="hdid">The hdid associated with the messaging verification.</param>
        /// <returns>A list of users matching the query.</returns>
        RequestResult<IEnumerable<MessagingVerificationModel>> GetMessageVerifications(string hdid);

        /// <summary>
        /// Retrieves a list of support users matching the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <returns>A list of support users matching the query.</returns>
        RequestResult<IEnumerable<SupportUser>> GetSupportUsers(UserQueryType queryType, string queryString);
    }
}
