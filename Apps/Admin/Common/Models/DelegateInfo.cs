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
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The delegate info model.
    /// </summary>
    public class DelegateInfo
    {
        /// <summary>
        /// Gets or sets the health directed identifier.
        /// </summary>
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delegate's PHN.
        /// </summary>
        public string PersonalHealthNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delegate's first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delegate's last name.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delegate's date of birth.
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// Gets or sets the physical address for the delegate.
        /// </summary>
        public Address? PhysicalAddress { get; set; }

        /// <summary>
        /// Gets or sets the postal address for the delegate.
        /// </summary>
        public Address? PostalAddress { get; set; }

        /// <summary>
        /// Gets or sets the delegate status.
        /// </summary>
        public DelegationStatus DelegationStatus { get; set; }
    }
}
