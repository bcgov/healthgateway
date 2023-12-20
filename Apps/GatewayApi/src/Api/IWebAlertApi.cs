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
namespace HealthGateway.GatewayApi.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.GatewayApi.Models.Phsa;
    using Refit;

    /// <summary>
    /// Interface to interact with PHSA Web Alert API.
    /// </summary>
    public interface IWebAlertApi
    {
        /// <summary>
        /// Retrieves all web alerts for a patient.
        /// </summary>
        /// <param name="pid">The patient's pid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>The collection of web alerts for the specified patient.</returns>
        [Get("/personal-accounts/{pid}/web-alerts")]
        Task<IList<PhsaWebAlert>> GetWebAlertsAsync(Guid pid, CancellationToken ct = default);

        /// <summary>
        /// Dismisses all web alerts for a patient.
        /// </summary>
        /// <param name="pid">The patient's pid.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Delete("/personal-accounts/{pid}/web-alerts")]
        Task DeleteWebAlertsAsync(Guid pid, CancellationToken ct = default);

        /// <summary>
        /// Dismisses a web alert for a patient.
        /// </summary>
        /// <param name="pid">The patient's pid.</param>
        /// <param name="id">The ID of the web alert to be deleted.</param>
        /// <param name="ct"><see cref="CancellationToken"/> to manage the async request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Delete("/personal-accounts/{pid}/web-alerts/{id}")]
        Task DeleteWebAlertAsync(Guid pid, Guid id, CancellationToken ct = default);
    }
}
