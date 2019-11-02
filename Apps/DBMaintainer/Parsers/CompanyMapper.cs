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
    public class CompanyMapper : ClassMap<Company>
    {
        /// <summary>
        /// Performs the mapping of the read file to the to the model.
        /// </summary>
        /// <param name="drugProducts">The DrugProduct to relate the object to.</param>
        public CompanyMapper(IEnumerable<DrugProduct> drugProducts)
        {
            // DRUG_CODE
            Map(m => m.DrugProductId).ConvertUsing(row => drugProducts.Where(d => d.DrugCode == row.GetField(0)).First().Id);
            // MFR_CODE
            Map(m => m.ManufacturerCode).Index(1);
            // COMPANY_CODE
            Map(m => m.CompanyCode).Index(2);
            // COMPANY_NAME
            Map(m => m.CompanyName).Index(3);
            // COMPANY_TYPE
            Map(m => m.CompanyType).Index(4);
            // ADDRESS_MAILING_FLAG
            Map(m => m.AddressMailingFlag).Index(5);
            // ADDRESS_BILLING_FLAG
            Map(m => m.AddressBillingFlag).Index(6);
            // ADDRESS_NOTIFICATION_FLAG
            Map(m => m.AddressNotificationFlag).Index(7);
            // ADDRESS_OTHER
            Map(m => m.AddressOther).Index(8);
            // SUITE_NUMBER
            Map(m => m.SuiteNumber).Index(9);
            // STREET_NAME
            Map(m => m.StreetName).Index(10);
            // CITY_NAME
            Map(m => m.CityName).Index(11);
            // PROVINCE
            Map(m => m.Province).Index(12);
            // COUNTRY
            Map(m => m.Country).Index(13);
            // POSTAL_CODE
            Map(m => m.PostalCode).Index(14);
            // POST_OFFICE_BOX
            Map(m => m.PostOfficeBox).Index(15);
            // PROVINCE_F
            Map(m => m.ProvinceFrench).Index(16);
            // COUNTRY_F
            Map(m => m.CountryFrench).Index(17);
        }
    }
}