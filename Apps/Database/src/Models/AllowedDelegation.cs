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
    using System.ComponentModel.DataAnnotations;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The allowed delegation model.
    /// </summary>
    [PrimaryKey(nameof(DependentHdId), nameof(DelegateHdId))]
    public class AllowedDelegation : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the dependent hdid.
        /// </summary>
        [MaxLength(52)]
        public string DependentHdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the delegate hdid.
        /// </summary>
        [MaxLength(52)]
        public string DelegateHdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the dependent associated to this allowed delegation.
        /// </summary>
        public Dependent Dependent { get; set; } = null!;
    }
}
