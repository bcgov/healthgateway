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

    /// <summary>
    /// The Location Address data model.
    /// </summary>
    public class LocationAddress
    {
        /// <summary>
        /// Gets or sets the Address Line 1.
        /// </summary>
        public string AddrLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 2.
        /// </summary>
        public string AddrLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 3.
        /// </summary>
        public string AddrLine3 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Address Line 4.
        /// </summary>
        public string AddrLine4 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the City.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Postal Code.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Province.
        /// </summary>
        public string Province { get; set; } = string.Empty;
    }
}
