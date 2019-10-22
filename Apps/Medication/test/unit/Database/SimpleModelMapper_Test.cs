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
namespace HealthGateway.Medication.Test
{
    using HealthGateway.Medication.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Database.Models;
    using HealthGateway.Medication.Database;
    using Xunit;


    public class SimpleModelMapper_Test
    {
        [Fact]
        public void ShouldMapSingleProduct()
        {
            // Setup
            DrugProduct drugProduct = new DrugProduct()
            {
                DrugIdentificationNumber = "12345678",
                BrandName = "A Brand Name"
            };

            // Act
            Medication medication = SimpleModelMapper.ToMedication(drugProduct);

            // Verify
            Assert.Equal(drugProduct.DrugIdentificationNumber, medication.DIN);
            Assert.Equal(drugProduct.BrandName, medication.BrandName);
        }

        [Fact]
        public void ShouldMapMultipleProduct()
        {
            // Setup
            List<DrugProduct> drugProductList = new List<DrugProduct>() {
                new DrugProduct()
                {
                    DrugIdentificationNumber = "12345678",
                    BrandName = "A Brand Name"
                },
                new DrugProduct()
                {
                    DrugIdentificationNumber = "22345678",
                    BrandName = "B Brand Name"
                },
                new DrugProduct()
                {
                    DrugIdentificationNumber = "32345678",
                    BrandName = "C Brand Name"
                }
            };

            // Act
            List<Medication> medicationList = SimpleModelMapper.ToMedicationList(drugProductList);

            // Verify
            Assert.Equal(drugProductList.Count, medicationList.Count);
            Assert.Equal(drugProductList[0].DrugIdentificationNumber, medicationList[0].DIN);
            Assert.Equal(drugProductList[0].BrandName, medicationList[0].BrandName);
        }
    }
}
