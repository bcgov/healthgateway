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
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PharmaCareDrug : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the system generated drug id.
        /// </summary>
        [Column("PharmaCareDrugId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the DIN/PIN.
        /// </summary>
        [Required]
        [MaxLength(8)]
        public string DINPIN { get; set; }

        /// <summary>
        /// Gets or sets the Plan.
        /// </summary>
        [MaxLength(2)]
        public string Plan { get; set; }

        /// <summary>
        /// Gets or sets the Effective date.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the End Date.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the Benefit group list.
        /// </summary>
        [MaxLength(60)]
        public string BenefitGroupList { get; set; }

        /// <summary>
        /// Gets or sets the low cost alternative indicator.
        /// </summary>
        [MaxLength(2)]
        public string LCAIndicator { get; set; }

        /// <summary>
        /// Gets or sets the Pay Generic indicator.
        /// </summary>
        [MaxLength(1)]
        public string PayGenericIndicator { get; set; }

        /// <summary>
        /// Gets or sets the brand name.
        /// </summary>
        [MaxLength(80)]
        public string BrandName { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        [MaxLength(6)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the generic name.
        /// </summary>
        [MaxLength(60)]
        public string GenericName { get; set; }

        /// <summary>
        /// Gets or sets the dosage form.
        /// </summary>
        [MaxLength(20)]
        public string DosageForm { get; set; }

        /// <summary>
        /// Gets or sets the trial flag.
        /// </summary>
        [MaxLength(1)]
        public string TrialFlag { get; set; }

        /// <summary>
        /// Gets or sets the maximum price.
        /// </summary>
        [Column(TypeName = "decimal(8,4)")]
        public decimal? MaximumPrice { get; set; }

        /// <summary>
        /// Gets or sets the Low Cost Alternative price.
        /// </summary>
        [Column(TypeName = "decimal(8,4)")]
        public decimal? LCAPrice { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program category.
        /// </summary>
        [MaxLength(4)]
        public string RDPCategory { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program sub-category.
        /// </summary>
        [MaxLength(4)]
        public string RDPSubCategory { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program price.
        /// </summary>
        [Column(TypeName = "decimal(8,4)")]
        public decimal? RDPPrice { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program execluded plans.
        /// </summary>
        [MaxLength(20)]
        public string RDPExcludedPlans { get; set; }

        /// <summary>
        /// Gets or sets the Canadian Federal Regularatory Code.
        /// </summary>
        [MaxLength(1)]
        public string CFRCode { get; set; }

        /// <summary>
        /// Gets or sets the PharmaCare Plan description.
        /// </summary>
        [MaxLength(80)]
        public string PharmaCarePlanDescription { get; set; }

        /// <summary>
        /// Gets or sets the maximum days supply.
        /// </summary>
        public int? MaximumDaysSupply { get; set; }

        /// <summary>
        /// Gets or sets the quantity limit.
        /// </summary>
        public int? QuantityLimit { get; set; }

        /// <summary>
        /// Gets or sets the formulary list date.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime FormularyListDate { get; set; }

        /// <summary>
        /// Gets or sets the limited use flag.
        /// </summary>
        [MaxLength(1)]
        public string LimitedUseFlag { get; set; }
    }
}
