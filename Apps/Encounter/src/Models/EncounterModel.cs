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
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a patient Encounter.
    /// </summary>
    public class EncounterModel
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the EncounterDate.
        /// </summary>
        [JsonPropertyName("encounterDate")]
        public DateTime EncounterDate { get; set; }

        /// <summary>
        /// Gets or sets the Specialty Description.
        /// </summary>
        [JsonPropertyName("specialtyDescription")]
        public string SpecialtyDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Practitioner Name.
        /// </summary>
        [JsonPropertyName("practitionerName")]
        public string PractitionerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Location Name.
        /// </summary>
        [JsonPropertyName("clinic")]
        public Clinic Clinic { get; set; } = new Clinic();

        /// <summary>
        /// Creates an Encounter object from an ODR model.
        /// </summary>
        /// <param name="model">The claim result to convert.</param>
        /// <returns>The newly created Encounter object.</returns>
        public static EncounterModel FromODRClaimModel(Claim model)
        {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
#pragma warning disable SCS0006 // Weak hashing function
            using var md5CryptoService = MD5.Create();
#pragma warning restore SCS0006 // Weak hashing function
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            StringBuilder sourceId = new();
            sourceId.Append($"{model.ServiceDate:yyyyMMdd}");
            sourceId.Append($"{model.SpecialtyDesc}");
            sourceId.Append($"{model.PractitionerName}");
            sourceId.Append($"{model.LocationName}");
            sourceId.Append($"{model.LocationAddress.Province}");
            sourceId.Append($"{model.LocationAddress.City}");
            sourceId.Append($"{model.LocationAddress.PostalCode}");
            sourceId.Append($"{model.LocationAddress.AddrLine1}");
            sourceId.Append($"{model.LocationAddress.AddrLine2}");
            sourceId.Append($"{model.LocationAddress.AddrLine3}");
            sourceId.Append($"{model.LocationAddress.AddrLine4}");
            return new EncounterModel()
            {
                Id = new Guid(md5CryptoService.ComputeHash(Encoding.Default.GetBytes(sourceId.ToString()))).ToString(),
                EncounterDate = model.ServiceDate,
                SpecialtyDescription = model.SpecialtyDesc,
                PractitionerName = model.PractitionerName,
                Clinic = new Clinic()
                {
                    Name = model.LocationName,
                },
            };
        }

        /// <summary>
        /// Creates an Encounter list from an ODR model.
        /// </summary>
        /// <param name="models">The list of ODR models to convert.</param>
        /// <returns>A list of Encounter objects.</returns>
        public static IEnumerable<EncounterModel> FromODRClaimModelList(IEnumerable<Claim> models)
        {
            List<EncounterModel> objects = new();
            HashSet<string> encounterIds = new();
            foreach (Claim claimModel in models)
            {
                var encounter = FromODRClaimModel(claimModel);
                if (!encounterIds.Contains(encounter.Id))
                {
                    objects.Add(encounter);
                    encounterIds.Add(encounter.Id);
                }
            }

            return objects;
        }
    }
}
