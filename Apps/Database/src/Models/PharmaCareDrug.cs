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
        public Guid PharmaCareDrugId { get; set; }

        [Required]
        [MaxLength(8)]
        public string DINPIN { get; set; }

        [MaxLength(2)]
        public string Plan { get; set; }

        [Column(TypeName = "Date")]
        public DateTime EffectiveDate { get; set; }

        [Column(TypeName = "Date")]
        public DateTime EndDate { get; set; }

        [MaxLength(60)]
        public string BenefitGroupList { get; set; }

        [MaxLength(2)]
        public string LCAIndicator { get; set; }

        [MaxLength(1)]
        public string PayGenericIndicator { get; set; }

        [MaxLength(60)]
        public string BrandName { get; set; }

        [MaxLength(6)]
        public string Manufacturer { get; set; }

        [MaxLength(60)]
        public string GenericName { get; set; }

        [MaxLength(20)]
        public string DosageForm { get; set; }

        [MaxLength(1)]
        public string TrialFlag { get; set; }

        [Column(TypeName = "decimal(8,4)")]
        public decimal? MaximumPrice { get; set; }

        [Column(TypeName = "decimal(8,4)")]
        public decimal? LCAPrice { get; set; }

        [MaxLength(4)]
        public string RDPCategory { get; set; }

        [MaxLength(4)]
        public string RDPSubCategory { get; set; }

        [Column(TypeName = "decimal(8,4)")]
        public decimal? RDPPrice { get; set; }

        [MaxLength(20)]
        public string RDPExcludedPlans { get; set; }

        [MaxLength(1)]
        public string CanadianFederalRegulatoryCode { get; set; }

        [MaxLength(20)]
        public string PharmaCarePlanDescription { get; set; }

        public int? MaximumDaysSupply { get; set; }

        public int? QuantityLimit { get; set; }

        [Column(TypeName = "Date")]
        public DateTime FormularyListDate { get; set; }

        [MaxLength(1)]
        public string LimitedUseFlag { get; set; }
    }
}
