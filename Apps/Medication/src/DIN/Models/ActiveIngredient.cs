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
namespace HealthGateway.DIN.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ActiveIngredient : AuditableEntity
    {
        public Guid ActiveIngredientId { get; set; }

        public DrugProduct Drug { get; set; }

        public int ActiveIngredientCode { get; set; }

        [MaxLength(240)]
        public string Ingredient { get; set; }

        [MaxLength(1)]
        public string IngredientSuppliedInd { get; set; }

        [MaxLength(20)]
        public string Strength { get; set; }

        [MaxLength(40)]
        public string StrengthUnit { get; set; }

        [MaxLength(40)]
        public string StrengthType { get; set; }

        [MaxLength(20)]
        public string DosageValue { get; set; }

        [MaxLength(1)]
        public string Base { get; set; }

        [MaxLength(40)]
        public string DosageUnit { get; set; }

        [MaxLength(2000)]
        public string Notes { get; set; }

        //Foreign Keys
        //Ingredient, Strength Unit, Type, and DosageUnit
    }
}
