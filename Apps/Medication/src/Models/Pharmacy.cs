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
    /// The Prescription data model.
    /// </summary>
    public class Pharmacy
    {
        /// <summary>
        /// Gets or sets the pharmacy id.
        /// </summary>
        public string PharmacyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address line 1.
        /// </summary>
        public string AddressLine1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address line 2.
        /// </summary>
        public string AddressLine2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the province.
        /// </summary>
        public string Province { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the fax number.
        /// </summary>
        public string FaxNumber { get; set; } = string.Empty;

        /// <summary>
        /// Creates a Pharamacy object from an ODR model.
        /// </summary>
        /// <param name="model">The ODR model to convert.</param>
        /// <returns>A Pharmacy model object.</returns>
        public static Pharmacy FromODRModel(ODR.Pharmacy? model)
        {
            if (model == null)
            {
                return new Pharmacy();
            }

            return new Pharmacy
            {
                PharmacyId = model.PharmacyId,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                FaxNumber = model.FaxNumber,
                AddressLine1 = model.Address.Line1,
                AddressLine2 = model.Address.Line2,
                City = model.Address.City,
                CountryCode = model.Address.Country,
                PostalCode = model.Address.PostalCode,
                Province = model.Address.Province,
            };
        }
    }
}
