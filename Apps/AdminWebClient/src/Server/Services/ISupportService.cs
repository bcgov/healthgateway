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
namespace HealthGateway.Admin.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Service that provides functionality to admin support.
    /// </summary>
    public interface ISupportService
    {
        /// <summary>
        /// Retrieves a list of message verifications matching the query.
        /// </summary>
        /// <param name="hdid">The hdid associated with the messaging verification.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A list of users matching the query.</returns>
        Task<RequestResult<IEnumerable<MessagingVerificationModel>>> GetMessageVerificationsAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the collection of patients that match the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The collection of patient support details that match the query.</returns>
        Task<RequestResult<IEnumerable<PatientSupportDetails>>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default);
    }
}
