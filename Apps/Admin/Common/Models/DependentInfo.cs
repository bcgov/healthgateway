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
namespace HealthGateway.Admin.Common.Models
{
    using System;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The dependent info model.
    /// </summary>
    public class DependentInfo
    {
        /// <summary>
        /// Gets or sets the dependent's first name.
        /// </summary>
        [JsonPropertyName("firstname")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dependent's last name.
        /// </summary>
        [JsonPropertyName("lastname")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dependent's date of birth.
        /// </summary>
        [JsonPropertyName("birthdate")]
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the physical address for the dependent.
        /// </summary>
        public Address? PhysicalAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal address for the dependent.
        /// </summary>
        public Address? PostalAddress { get; set; }

        /// <summary>
        /// Gets or sets the protected state of the dependent.
        /// </summary>
        public bool? Protected { get; set; }
    }
}
