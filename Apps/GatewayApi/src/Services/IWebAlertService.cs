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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.GatewayApi.Models;

    /// <summary>
    /// Service to interact with the the PHSA web alert API.
    /// </summary>
    public interface IWebAlertService
    {
        /// <summary>
        /// Gets all web alerts for a user.
        /// </summary>
        /// <param name="hdid">The HDID of the user.</param>
        /// <returns>The list of web alerts for the user.</returns>
        Task<IList<WebAlert>> GetWebAlertsAsync(string hdid);

        /// <summary>
        /// Dismisses all web alert for a user.
        /// </summary>
        /// <param name="hdid">The HDID of the user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DismissWebAlertsAsync(string hdid);

        /// <summary>
        /// Dismisses a web alert for a user.
        /// </summary>
        /// <param name="hdid">The HDID of the user.</param>
        /// <param name="webAlertId">The ID of the web alert to be dismissed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DismissWebAlertAsync(string hdid, Guid webAlertId);
    }
}
