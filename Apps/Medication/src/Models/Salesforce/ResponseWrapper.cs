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
namespace HealthGateway.Medication.Models.Salesforce
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Special Authority Request.
    /// </summary>
    public class ResponseWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseWrapper"/> class.
        /// </summary>
        public ResponseWrapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseWrapper"/> class.
        /// </summary>
        /// <param name="items">The list of items to initialize.</param>
        [JsonConstructor]
        public ResponseWrapper(IList<SpecialAuthorityRequest> items)
        {
            this.Items = items;
        }

        /// <summary>
        /// Gets the patientIdentifier.
        /// </summary>
        [JsonPropertyName("items")]
        public IList<SpecialAuthorityRequest> Items { get; } = new List<SpecialAuthorityRequest>();
    }
}
