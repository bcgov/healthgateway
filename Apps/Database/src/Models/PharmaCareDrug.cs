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
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Represents a row in the Pharmacare drug file
    /// https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/health-industry-professionals/downloadable-drug-data-files.
    /// </summary>
    public class PharmaCareDrug : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the system generated drug id.
        /// </summary>
        [Column("PharmaCareDrugId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the DIN/PIN.
        /// Drug Identification Number assigned by Health Canada or Product Identification Number assigned by BC PharmaCare.
        /// </summary>
        [Required]
        [MaxLength(8)]
        [Column("DINPIN")]
        public string DinPin { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Plan.
        /// Plans under which the drug is an eligible benefit.
        /// Drugs with no benefit groups are identified as ‘NB’ for ‘Non Benefit’.
        /// </summary>
        [MaxLength(2)]
        public string? Plan { get; set; }

        /// <summary>
        /// Gets or sets the Effective date.
        /// The date that the information on the record becomes in effect.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the End Date.
        /// The last date that the information on the record is effective.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the Benefit group list.
        /// Benefit group(s) under which the drug is eligible. In general, benefit groups are identified by 2-characters.
        /// For those benefit groups identified by one character, a space precedes the character.
        /// </summary>
        [MaxLength(60)]
        public string? BenefitGroupList { get; set; }

        /// <summary>
        /// Gets or sets the low cost alternative indicator.
        /// Full Partial Benefit – “P” indicates a partial benefit, and P* indicates a RDP drug that is a partial benefit,
        /// but a full benefit with a RDP special authority.
        /// See LCA transition information at main site under “Caveats”.
        /// </summary>
        [MaxLength(2)]
        [Column("LCAIndicator")]
        public string? LcaIndicator { get; set; }

        /// <summary>
        /// Gets or sets the Pay Generic indicator.
        /// An alternative to LCA Indicator but contains only ‘Y’ or ‘N’.
        /// ‘Y’ indicates a partial LCA benefit and ‘N’ indicates a full LCA benefit OR not part of LCA program.
        /// </summary>
        [MaxLength(1)]
        public string? PayGenericIndicator { get; set; }

        /// <summary>
        /// Gets or sets the brand name.
        /// Brand name (manufacturer’s name) of the drug or product.
        /// </summary>
        [Required]
        [MaxLength(80)]
        public string BrandName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the manufacturer.
        /// Drug Manufacturer Code for the manufacturer of the drug.
        /// </summary>
        [MaxLength(6)]
        public string? Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the generic name.
        /// Active Ingredient of the Drug.
        /// This field contains the chemical name as well as the strength, dose and form of the drug.
        /// </summary>
        [MaxLength(60)]
        public string? GenericName { get; set; }

        /// <summary>
        /// Gets or sets the dosage form.
        /// Dose form description such as “TABLETS” to explain the abbreviation used in the Generic Name.
        /// </summary>
        [MaxLength(20)]
        public string? DosageForm { get; set; }

        /// <summary>
        /// Gets or sets the trial flag.
        /// The drug and plan combination is eligible for the Trial Prescription Program.
        /// </summary>
        [MaxLength(1)]
        public string? TrialFlag { get; set; }

        /// <summary>
        /// Gets or sets the maximum price.
        /// BC PharmaCare maximum allowable price.
        /// </summary>
        [Column(TypeName = "decimal(8,4)")]
        public decimal? MaximumPrice { get; set; }

        /// <summary>
        /// Gets or sets the Low Cost Alternative price.
        /// LCA price in effect for this drug. A generic price of $0 indicates the drug is (a) a full benefit under the
        /// Low Cost Alternative (LCA) program or (b) is not included in the LCA program.
        /// An entry in this field other than zero indicates that the drug is a partial benefit under the Low Cost Alternative
        /// (LCA) program
        /// and the LCA price indicated is the highest amount PharmaCare will recognize for the drug.
        /// Note: a lower price may supersede the LCA price listed here for claim adjudication.Most often, this would be the RDP
        /// price.
        /// </summary>
        [Column("LCAPrice", TypeName = "decimal(8,4)")]
        public decimal? LcaPrice { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program category.
        /// RDP Category (e.g., “0024”).
        /// </summary>
        [MaxLength(4)]
        [Column("RDPCategory")]
        public string? RdpCategory { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program sub-category.
        /// Reference Drug Sub-category (e.g., “0001”).
        /// </summary>
        [MaxLength(4)]
        [Column("RDPSubCategory")]
        public string? RdpSubCategory { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program price.
        /// Reference Drug Price per Day.
        /// </summary>
        [Column("RDPPrice", TypeName = "decimal(8,4)")]
        public decimal? RdpPrice { get; set; }

        /// <summary>
        /// Gets or sets the Reference Drug Program execluded plans.
        /// Plan(s) under which the drug is not subject to RDP price reductions.
        /// </summary>
        [MaxLength(20)]
        [Column("RDPExcludedPlans")]
        public string? RdpExcludedPlans { get; set; }

        /// <summary>
        /// Gets or sets the Canadian Federal Regularatory Code.
        /// Valid values:
        /// A–Homeopathic Drug Products
        /// C–Controlled drugs(Controlled Drugs fall under Schedule G [all sections] in the Food and Drug Regulations)
        /// D–Biologicals(Biologicals fall under Schedule G[all sections] in the Food and Drug Regulations)
        /// E–Ethical
        /// N–Narcotic drugs(Narcotics are covered under the Narcotics Control Regulations)
        /// P–Prescription drugs(see Schedule F in the Food and Drug Regulations)
        /// O–Over the counter
        /// R–CDSA Recommended(See CDSA Recommended in the Food and Drug Regulations)
        /// T–Targeted(Targeted drugs fall under the Benzodiazepines and Other Targeted Substances Regulations)
        /// NULL–The product is listed as a Not Marketed Item.
        /// -
        /// Note: Notice of Compliance (NOC) items are not listed in the DPD. NOC lists are a separate entity on the
        /// Health Canada website and do not include federal schedule codes.Since the NOC list does not include the federal
        /// schedule code, NOCs are added to the MedKnowledge database as Not Marketed products and usually without a federal
        /// schedule code.
        /// -
        /// Note: Products that fall under two or more regulatory codes are listed under the code for the category that is most
        /// restrictive.
        /// </summary>
        [MaxLength(1)]
        [Column("CFRCode")]
        public string? CfrCode { get; set; }

        /// <summary>
        /// Gets or sets the PharmaCare Plan description.
        /// A description of the PharmaCare Plan (e.g., ”Income Based”).
        /// </summary>
        [MaxLength(80)]
        public string? PharmaCarePlanDescription { get; set; }

        /// <summary>
        /// Gets or sets the maximum days supply.
        /// The maximum days supply recognized for reimbursement for the DIN / Plan combination.
        /// </summary>
        public int? MaximumDaysSupply { get; set; }

        /// <summary>
        /// Gets or sets the quantity limit.
        /// The maximum quantity recognized for reimbursement for the DIN / Plan combination.
        /// </summary>
        public int? QuantityLimit { get; set; }

        /// <summary>
        /// Gets or sets the formulary list date.
        /// The first date that PharmaCare treated the DIN as an eligible benefit for the Plan indicated.
        /// </summary>
        [Column(TypeName = "Date")]
        public DateTime FormularyListDate { get; set; }

        /// <summary>
        /// Gets or sets the limited use flag.
        /// An indicator that the DIN is a Limited Use product.
        /// </summary>
        [MaxLength(1)]
        public string? LimitedUseFlag { get; set; }

        /// <summary>
        /// Gets or sets the assoicated File Download ID.
        /// </summary>
        [Required]
        public Guid FileDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the FileDownload entity.
        /// Code first mechanism to define the foreign key.
        /// </summary>
        public virtual FileDownload? FileDownload { get; set; }
    }
}
