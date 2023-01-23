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

namespace HealthGateway.GatewayApi.Models
{
    using System;

    /// <summary>
    /// Object that defines the request for getting dependents.
    /// </summary>
    public class GetDependentRequest
    {
        /// <summary>
        /// Gets or sets from date. Required.
        /// </summary>
        public DateTime FromDate { get; set; }

        /// <summary>
        /// Gets or sets to date.
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Gets or sets page size to return, max = 5000 rows.
        /// </summary>
        public int? PageSize { get; set; } = 5000;

        /// <summary>
        /// Gets or sets page number for paging.
        /// </summary>
        public int? PageNumber { get; set; } = 0;
    }
}
