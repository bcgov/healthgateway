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
        /// Gets the active ingredient code.
        /// </summary>
        public int ActiveIngredientCode { get; init; }

        /// <summary>
        /// Gets the ingredient.
        /// </summary>
        public string? Ingredient { get; init; }

        /// <summary>
        /// Gets the strength.
        /// </summary>
        public string? Strength { get; init; }

        /// <summary>
        /// Gets the strength unit.
        /// </summary>
        public string? StrengthUnit { get; init; }
    }
}
