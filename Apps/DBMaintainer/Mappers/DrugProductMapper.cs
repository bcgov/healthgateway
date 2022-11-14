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
namespace HealthGateway.DBMaintainer.Mappers
{
    using CsvHelper.Configuration;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Mapping class to which maps the read file to the relavent model object.
    /// </summary>
    public sealed class DrugProductMapper : ClassMap<DrugProduct>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrugProductMapper"/> class.
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="filedownload">The filedownload to map.</param>
        public DrugProductMapper(FileDownload filedownload)
        {
            // DRUG_CODE
            this.Map(m => m.DrugCode).Index(0);

            // PRODUCT_CATEGORIZATION
            this.Map(m => m.ProductCategorization).Index(1);

            // CLASS
            this.Map(m => m.DrugClass).Index(2);

            // DRUG_IDENTIFICATION_NUMBER
            this.Map(m => m.DrugIdentificationNumber).Index(3);

            // BRAND_NAME
            this.Map(m => m.BrandName).Index(4);

            // DESCRIPTOR
            this.Map(m => m.Descriptor).Index(5);

            // PEDIATRIC_FLAG
            this.Map(m => m.PediatricFlag).Index(6);

            // ACCESSION_NUMBER
            this.Map(m => m.AccessionNumber).Index(7);

            // NUMBER_OF_AIS
            this.Map(m => m.NumberOfAis).Index(8);

            // LAST_UPDATE_DATE
            this.Map(m => m.LastUpdate).Index(9);

            // AI_GROUP_NO
            this.Map(m => m.AiGroupNumber).Index(10);

            // CLASS_F
            this.Map(m => m.DrugClassFrench).Index(11);

            // BRAND_NAME_F
            this.Map(m => m.BrandNameFrench).Index(12);

            // DESCRIPTOR_F
            this.Map(m => m.DescriptorFrench).Index(13);

            // Map the Filedownload to each object
            this.Map(m => m.FileDownload).Convert(_ => filedownload);
        }
    }
}
