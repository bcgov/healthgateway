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
namespace HealthGateway.Database.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The dependent model.
    /// </summary>
    public class Dependent : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the dependent hdid.
        /// </summary>
        [Key]
        [MaxLength(52)]
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether or not dependent has been protected.
        /// </summary>
        public bool Protected { get; set; }

        /// <summary>
        /// Gets or sets the allowed delegations for this dependent.
        /// </summary>
        public ICollection<AllowedDelegation> AllowedDelegations { get; set; } = [];
    }
}
