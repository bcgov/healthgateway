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
namespace HealthGateway.DBMaintainer.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Mapping class to which maps the read file to the relevant model object.
    /// </summary>
    public sealed class ActiveIngredientMapper : ClassMap<ActiveIngredient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveIngredientMapper"/> class.
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public ActiveIngredientMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            this.Map(m => m.DrugProductId).Convert(converter => drugProducts.First(d => d.DrugCode == converter.Row.GetField(0)).Id);

            // ACTIVE_INGREDIENT_CODE
            this.Map(m => m.ActiveIngredientCode).Index(1);

            // INGREDIENT
            this.Map(m => m.Ingredient).Index(2);

            // INGREDIENT_SUPPLIED_IND
            this.Map(m => m.IngredientSuppliedInd).Index(3);

            // STRENGTH
            this.Map(m => m.Strength).Index(4);

            // STRENGTH_UNIT
            this.Map(m => m.StrengthUnit).Index(5);

            // STRENGTH_TYPE
            this.Map(m => m.StrengthType).Index(6);

            // DOSAGE_VALUE
            this.Map(m => m.DosageValue).Index(7);

            // BASE
            this.Map(m => m.Base).Index(8);

            // DOSAGE_UNIT
            this.Map(m => m.DosageUnit).Index(9);

            // NOTES
            this.Map(m => m.Notes).Index(10);

            // INGREDIENT_FFootnote
            this.Map(m => m.IngredientFrench).Index(11);

            // STRENGTH_UNIT_FFootnote
            this.Map(m => m.StrengthUnitFrench).Index(12);

            // STRENGTH_TYPE_FFootnote
            this.Map(m => m.StrengthTypeFrench).Index(13);

            // DOSAGE_UNIT_FFootnote
            this.Map(m => m.DosageUnitFrench).Index(14);
        }
    }
}
