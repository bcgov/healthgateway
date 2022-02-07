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
namespace HealthGateway.Laboratory.Models.PHSA
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model object representing a public lab test kit.
    /// </summary>
    public class PublicLabTestKit
    {
        /// <summary>
        /// Gets or sets the Personal Health Number this test will be registered against.
        /// </summary>
        [JsonPropertyName("phn")]
        public string Phn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date of birth for the identified PHN.
        /// </summary>
        [JsonPropertyName("dob")]
        public string Dob { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name for the identified PHN.
        /// </summary>
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the surname for the identified PHN.
        /// </summary>
        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the test was used.
        /// </summary>
        [JsonPropertyName("testTakenMinutesAgo")]
        public int TestTakenMinutesAgo { get; set; }

        /// <summary>
        /// Gets or sets the test kit id.
        /// </summary>
        [JsonPropertyName("testKitCId")]
        public string TestKitId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets first portion of the short code.
        /// </summary>
        [JsonPropertyName("testKitId")]
        public string shortCodeFirst { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the second partion of the short code.
        /// </summary>
        [JsonPropertyName("testKitId")]
        public string shortCodeSecond { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contacts phone number.
        /// </summary>
        [JsonPropertyName("contactPhoneNumber")]
        public string ContactPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contacts street address.
        /// This field is optional if the PHN is supplied.
        /// </summary>
        [JsonPropertyName("streetAddress")]
        public string StreetAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city for the contact.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code for the contact.
        /// </summary>
        [JsonPropertyName("postalOrZip")]
        public string PostalOrZip { get; set; } = string.Empty;
    }
}
