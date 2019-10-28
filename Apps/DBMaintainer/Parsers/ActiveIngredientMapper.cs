// //-------------------------------------------------------------------------
// // Copyright © 2019 Province of British Columbia
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //-------------------------------------------------------------------------
namespace HealthGateway.DrugMaintainer
{
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;
    using System.Collections.Generic;
    using System.Linq;

    public class ActiveIngredientMapper : ClassMap<ActiveIngredient>
    {
        public ActiveIngredientMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.DrugProduct).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First());
            // ACTIVE_INGREDIENT_CODE
            Map(m => m.ActiveIngredientCode).Index(1);
            // INGREDIENT
            Map(m => m.Ingredient).Index(2);
            // INGREDIENT_SUPPLIED_IND
            Map(m => m.IngredientSuppliedInd).Index(3);
            // STRENGTH
            Map(m => m.Strength).Index(4);
            // STRENGTH_UNIT
            Map(m => m.StrengthUnit).Index(5);
            // STRENGTH_TYPE
            Map(m => m.StrengthType).Index(6);
            // DOSAGE_VALUE
            Map(m => m.DosageValue).Index(7);
            // BASE
            Map(m => m.Base).Index(8);
            // DOSAGE_UNIT
            Map(m => m.DosageUnit).Index(9);
            // NOTES
            Map(m => m.Notes).Index(10);
            // INGREDIENT_FFootnote
            Map(m => m.IngredientFrench).Index(11);
            // STRENGTH_UNIT_FFootnote
            Map(m => m.StrengthUnitFrench).Index(12);
            // STRENGTH_TYPE_FFootnote
            Map(m => m.StrengthTypeFrench).Index(13);
            //DOSAGE_UNIT_FFootnote
            Map(m => m.DosageUnitFrench).Index(14);

        }
    }
}