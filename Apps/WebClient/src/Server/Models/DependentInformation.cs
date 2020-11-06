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
namespace HealthGateway.WebClient.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models;

    /// <summary>
    /// Represents a Dependent Information model.
    /// </summary>
    public class DependentInformation
    {
        /// <summary>
        /// Gets or sets the hdid of the dependent.
        /// </summary>
        [JsonPropertyName("hdid")]
        public string HdId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the dependent.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Masked PHN of the dependent.
        /// </summary>
        [JsonPropertyName("maskedPHN")]
        public string MaskedPHN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Date Of Birth.
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Constructs a new DependentInformation based on a PatientModel.
        /// </summary>
        /// <param name="patientModel">The Patien Model to be converted.</param>
        /// <returns>The Dependent Model.</returns>
        public static DependentInformation FromPatientModel(PatientModel patientModel)
        {
            DependentInformation result = new DependentInformation()
            {
                HdId = patientModel.HdId,
                Name = $"{patientModel.FirstName} {patientModel.LastName} ",
                Gender = patientModel.Gender,
                DateOfBirth = patientModel.Birthdate,
            };

            if (patientModel.PersonalHealthNumber.Length > 3)
            {
                result.MaskedPHN = patientModel.PersonalHealthNumber.Remove(patientModel.PersonalHealthNumber.Length - 5, 4) + "****";
            }
            else
            {
                result.MaskedPHN = "****";
            }

            return result;
        }
    }
}
