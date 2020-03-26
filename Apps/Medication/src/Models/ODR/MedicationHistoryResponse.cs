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
namespace HealthGateway.Medication.Models.ODR
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Utils;
    using HealthGateway.Medication.Constants;

    /// <summary>
    /// The ODR Medication Response.
    /// </summary>
    public class MedicationHistoryResponse
    {
        /// <summary>
        /// Gets or sets the Id of the request.
        /// </summary>
        [JsonPropertyName("uuid")]
        public Guid Id { get; set; } = default;

        /// <summary>
        /// Gets or sets the total records available from the server for the query excluding page limits.
        /// </summary>
        [JsonPropertyName("totalRecords")]
        public int Records { get; set; }

        ///// <summary>
        ///// Gets or sets the page number of the data currently being returned.
        ///// </summary>
        //[JsonPropertyName("pageNumber")]
        //public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the Total Pages available.
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int Pages { get; set; }

        /// <summary>
        /// Gets or sets the set of MedicationResults.
        /// The set is boud by the other class properties.
        /// </summary>
        [JsonPropertyName("records")]
        public IEnumerable<MedicationResult>? Results { get; set; }

    }
}
