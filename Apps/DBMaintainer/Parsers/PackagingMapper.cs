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

    /// <summary>
    /// Mapping class to which maps the read file to the relavent model object.
    /// </summary>
    public class PackagingMapper : ClassMap<Packaging>
    {
        /// <summary>
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public PackagingMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.DrugProductId).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First().Id);
            // UPC
            Map(m => m.UPC).Index(1);
            // PACKAGE_SIZE_UNIT
            Map(m => m.PackageSizeUnit).Index(2);
            // PACKAGE_TYPE
            Map(m => m.PackageType).Index(3);
            // PACKAGE_SIZE
            Map(m => m.PackageSize).Index(4);
            // PRODUCT_INFORMATION
            Map(m => m.ProductInformation).Index(5);
            // PACKAGE_SIZE_UNIT_F
            Map(m => m.PackageSizeUnitFrench).Index(6);
            // PACKAGE_TYPE_F
            Map(m => m.PackageTypeFrench).Index(7);
        }
    }
}