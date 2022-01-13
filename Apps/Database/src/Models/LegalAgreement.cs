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
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// A represntation of Legal Agreements for Health Gateway.
    /// These agreements are open ended but do have an effective date.
    /// </summary>
    public class LegalAgreement : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        [Column("LegalAgreementsId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value representing the type of legal agreement.
        /// The value is one of <see cref="Constants.LegalAgreementType"/>.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public LegalAgreementType LegalAgreementCode { get; set; } = LegalAgreementType.TermsofService;

        /// <summary>
        /// Gets or sets the legal text.
        /// </summary>
        [Required]
        public string? LegalText { get; set; }

        /// <summary>
        /// Gets or sets the effective date of the agreement.
        /// </summary>
        [Required]
        public DateTime? EffectiveDate { get; set; }
    }
}
