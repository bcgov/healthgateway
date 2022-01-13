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
    /// The entity representing a row in the Federal Government
    /// Active Ingredient file.
    /// </summary>
    public class ActiveIngredient : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the active ingredient id.
        /// </summary>
        [Column("ActiveIngredientId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the active ingredient code.
        /// </summary>
        public int ActiveIngredientCode { get; set; }

        /// <summary>
        /// Gets or sets the ingredient.
        /// </summary>
        [MaxLength(240)]
        public string? Ingredient { get; set; }

        /// <summary>
        /// Gets or sets the ingredient in French.
        /// </summary>
        [MaxLength(400)]
        public string? IngredientFrench { get; set; }

        /// <summary>
        /// Gets or sets the ingredient supplied indicator.
        /// </summary>
        [MaxLength(1)]
        public string? IngredientSuppliedInd { get; set; }

        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        [MaxLength(20)]
        public string? Strength { get; set; }

        /// <summary>
        /// Gets or sets the strength unit.
        /// </summary>
        [MaxLength(40)]
        public string? StrengthUnit { get; set; }

        /// <summary>
        /// Gets or sets the strengthunit in French.
        /// </summary>
        [MaxLength(80)]
        public string? StrengthUnitFrench { get; set; }

        /// <summary>
        /// Gets or sets the strength type.
        /// </summary>
        [MaxLength(40)]
        public string? StrengthType { get; set; }

        /// <summary>
        /// Gets or sets the strengthtype in French.
        /// </summary>
        [MaxLength(80)]
        public string? StrengthTypeFrench { get; set; }

        /// <summary>
        /// Gets or sets the Dosage value.
        /// </summary>
        [MaxLength(20)]
        public string? DosageValue { get; set; }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        [MaxLength(1)]
        public string? Base { get; set; }

        /// <summary>
        /// Gets or sets the dosage unit.
        /// </summary>
        [MaxLength(40)]
        public string? DosageUnit { get; set; }

        /// <summary>
        /// Gets or sets the dosage unit in French.
        /// </summary>
        [MaxLength(80)]
        public string? DosageUnitFrench { get; set; }

        /// <summary>
        /// Gets or sets the Notes.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the Drug Product foreign key.
        /// </summary>
        [Required]
        public Guid DrugProductId { get; set; }
    }
}
