//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Encounter.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a patient Encounter.
    /// </summary>
    public class Encounter
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the EncounterDate.
        /// </summary>
        [JsonPropertyName("encounterDate")]
        public DateTime EncounterDate { get; set; }

        /// <summary>
        /// Gets or sets the Specialty Description.
        /// </summary>
        [JsonPropertyName("specialtyDesc")]
        public string SpecialtyDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Practitioner Name.
        /// </summary>
        [JsonPropertyName("practitionerName")]
        public string PractitionerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Location Name.
        /// </summary>
        [JsonPropertyName("locationName")]
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Location Address.
        /// </summary>
        [JsonPropertyName("locationAddress")]
        public LocationAddress LocationAddress { get; set; } = new LocationAddress();

        /// <summary>
        /// Creates an Encounter object from an ODR model.
        /// </summary>
        /// <param name="model">The claim result to convert.</param>
        /// <returns>The newly created Encounter object.</returns>
        public static Encounter FromODRClaimModel(Claim model)
        {
            return new Encounter()
            {
                Id = model.ClaimId,
                EncounterDate = model.ServiceDate,
                SpecialtyDescription = model.SpecialtyDesc,
                PractitionerName = model.PractitionerName,
                LocationName = model.LocationName,
                LocationAddress = model.LocationAddress
            };
        }

        /// <summary>
        /// Creates an Encounter list from an ODR model.
        /// </summary>
        /// <param name="models">The list of ODR models to convert.</param>
        /// <returns>A list of Encounter objects.</returns>
        public static List<Encounter> FromODRClaimModelList(List<Claim> models)
        {
            List<Encounter> objects = new List<Encounter>();

            foreach (Claim claimModel in models)
            {
                objects.Add(Encounter.FromODRClaimModel(claimModel));
            }

            return objects;
        }
    }
}
