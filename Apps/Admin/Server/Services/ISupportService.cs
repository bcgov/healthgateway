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
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Service that provides functionality to admin support.
    /// </summary>
    public interface ISupportService
    {
        /// <summary>
        /// Retrieves a list of messaging verifications matching the query.
        /// </summary>
        /// <param name="hdid">The HDID associated with the messaging verifications.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>A list of messaging verifications matching the query.</returns>
        Task<RequestResult<IEnumerable<MessagingVerificationModel>>> GetMessageVerificationsAsync(string hdid, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the collection of patients that match the query.
        /// </summary>
        /// <param name="queryType">The type of query to perform.</param>
        /// <param name="queryString">The value to query on.</param>
        /// <param name="ct">A cancellation token.</param>
        /// <returns>The collection of patient support details that match the query.</returns>
        Task<IList<PatientSupportDetails>> GetPatientsAsync(PatientQueryType queryType, string queryString, CancellationToken ct = default);
    }
}
