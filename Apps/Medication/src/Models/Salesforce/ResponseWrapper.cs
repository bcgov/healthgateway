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
    /// Represents a Special Autrhority Request.
    /// </summary>
    public class ResponseWrapper
    {
        /// <summary>
        /// Gets or sets the patientIdentifier.
        /// </summary>
        [JsonPropertyName("items")]
        public IList<SpecialAuthorityRequest> Items { get; set; } = new List<SpecialAuthorityRequest>();

        /// <summary>
        /// Creates a List of Medication Request object from an SpecialAuthorityRequest model.
        /// </summary>
        /// <returns>A list of MedicationRequest objects.</returns>
        public IList<MedicationRequest> ToHGModels()
        {
            IList<MedicationRequest> objects = new List<MedicationRequest>();

            foreach (SpecialAuthorityRequest requestModel in this.Items)
            {
                objects.Add(requestModel.ToHGModel());
            }

            return objects;
        }
    }
}
