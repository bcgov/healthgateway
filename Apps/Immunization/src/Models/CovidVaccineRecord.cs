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
namespace HealthGateway.Immunization.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Immunization.Models.PHSA;

    /// <summary>
    /// The Vaccination Record Result model.
    /// </summary>
    public class CovidVaccineRecord
    {
        /// <summary>
        /// Gets or sets a value indicating whether the Vaccination Record result has been retrieved.
        /// Will be set to true if the object has been fully loaded.
        /// When false, only Loaded, and RetryIn will be populated.
        /// </summary>
        [JsonPropertyName("loaded")]
        public bool Loaded { get; set; }

        /// <summary>
        /// Gets or sets the minimal amount of time that should be waited before another request.
        /// The unit of measurement is in milliseconds.
        /// </summary>
        [JsonPropertyName("retryin")]
        public int RetryIn { get; set; }

        /// <summary>
        /// Gets or sets the rendered document.
        /// </summary>
        [JsonPropertyName("document")]
        public EncodedMedia Document { get; set; } = new ();

        /// <summary>
        /// Gets or sets the associated QR code.
        /// </summary>
        [JsonPropertyName("qrCode")]
        public EncodedMedia QRCode { get; set; } = new ();
    }
}
