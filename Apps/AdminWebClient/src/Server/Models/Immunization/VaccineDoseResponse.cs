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
namespace HealthGateway.Admin.Models.Immunization
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The data model for an immunization dose.
    /// </summary>
    public class VaccineDoseResponse
    {
        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        [JsonPropertyName("product")]
        public string? Product { get; set; }

        /// <summary>
        /// Gets or sets the lot identifier of the immunization.
        /// </summary>
        [JsonPropertyName("lot")]
        public string? Lot { get; set; }

        /// <summary>
        /// Gets or sets the location where the dose was administered.
        /// </summary>
        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>
        /// Gets or sets the date the dose was administered.
        /// </summary>
        [JsonPropertyName("doseDate")]
        public DateTime? Date { get; set; }
    }
}
