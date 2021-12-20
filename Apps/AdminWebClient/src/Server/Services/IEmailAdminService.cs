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
    using HealthGateway.Admin.Models;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Service that provides admin functinoality to emails.
    /// </summary>
    public interface IEmailAdminService
    {
        /// <summary>
        /// Gets all the emails in the system up to the pageSize.
        /// </summary>
        /// <returns>A List of notes wrapped in a RequestResult.</returns>
        RequestResult<IEnumerable<AdminEmail>> GetEmails();
    }
}
