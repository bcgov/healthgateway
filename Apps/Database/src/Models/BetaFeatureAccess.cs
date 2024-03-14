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
namespace HealthGateway.Database.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.EntityFrameworkCore;
    using BetaFeature = HealthGateway.Database.Constants.BetaFeature;

    /// <summary>
    /// The beta feature access model.
    /// </summary>
    [PrimaryKey(nameof(Hdid), nameof(BetaFeatureCode))]
    public class BetaFeatureAccess : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        [Column("UserProfileId")]
        [MaxLength(52)]
        public string Hdid { get; set; } = null!;

        /// <summary>
        /// Gets or sets the beta feature code.
        /// </summary>
        [MaxLength(50)]
        public BetaFeature BetaFeatureCode { get; set; }

        /// <summary>
        /// Gets or sets the UserProfile associated with <see cref="Hdid"/>.
        /// </summary>
        public virtual UserProfile UserProfile { get; set; } = null!;
    }
}
