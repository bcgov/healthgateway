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
namespace HealthGateway.Medication.Models
{
    /// <summary>
    /// Contains active ingredient information for a medication.
    /// </summary>
    public class MedicationActiveIngredient
    {
        /// <summary>
        /// Gets or sets the active ingredient code.
        /// </summary>
        public int ActiveIngredientCode { get; set; }

        /// <summary>
        /// Gets or sets the ingredient.
        /// </summary>
        public string? Ingredient { get; set; }

        /// <summary>
        /// Gets or sets the ingredient in French.
        /// </summary>
        public string? IngredientFrench { get; set; }

        /// <summary>
        /// Gets or sets the ingredient supplied indicator.
        /// </summary>
        public string? IngredientSuppliedInd { get; set; }

        /// <summary>
        /// Gets or sets the strength.
        /// </summary>
        public string? Strength { get; set; }

        /// <summary>
        /// Gets or sets the strength unit.
        /// </summary>
        public string? StrengthUnit { get; set; }

        /// <summary>
        /// Gets or sets the strength unit in French.
        /// </summary>
        public string? StrengthUnitFrench { get; set; }

        /// <summary>
        /// Gets or sets the strength type.
        /// </summary>
        public string? StrengthType { get; set; }

        /// <summary>
        /// Gets or sets the strength type in French.
        /// </summary>
        public string? StrengthTypeFrench { get; set; }

        /// <summary>
        /// Gets or sets the dosage value.
        /// </summary>
        public string? DosageValue { get; set; }

        /// <summary>
        /// Gets or sets the base.
        /// </summary>
        public string? Base { get; set; }

        /// <summary>
        /// Gets or sets the dosage unit.
        /// </summary>
        public string? DosageUnit { get; set; }

        /// <summary>
        /// Gets or sets the dosage unit in French.
        /// </summary>
        public string? DosageUnitFrench { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        public string? Notes { get; set; }
    }
}
