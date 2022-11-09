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
    using System.Text.Json.Serialization;
    using HealthGateway.Medication.Constants;

    /// <summary>
    /// The Protective Word query/response model.
    /// </summary>
    public class ProtectiveWordQueryResponse
    {
        /// <summary>
        /// Gets or sets the Operation to perform and defaults to the query operation.
        /// </summary>
        [JsonPropertyName("operator")]
        public ProtectiveWordOperator Operator { get; set; } = ProtectiveWordOperator.Get;

        /// <summary>
        /// Gets or sets the PHN for the request.
        /// </summary>
        [JsonPropertyName("phn")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value to be applied to the Get operator.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
