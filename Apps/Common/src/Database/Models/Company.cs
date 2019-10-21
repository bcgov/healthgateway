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
<<<<<<< HEAD:Apps/Common/src/Database/Models/Company.cs
namespace HealthGateway.Common.Database.Models
=======
namespace HealthGateway.DIN.Models
>>>>>>> dev:Apps/Common/src/DIN/Models/Company.cs
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Company : AuditableEntity
    {
        public Guid CompanyId { get; set; }

        [Required]
        public DrugProduct Drug { get; set; }

        [MaxLength(5)]
        public string ManufacturerCode { get; set; }

        public int CompanyCode { get; set; }

        [MaxLength(80)]
        public string CompanyName { get; set; }

        [MaxLength(40)]
        public string CompanyType { get; set; }

        [MaxLength(1)]
        public string AddressMailingFlag { get; set; }

        [MaxLength(1)]
        public string AddressBillingFlag { get; set; }

        [MaxLength(1)]
        public string AddressNotificationFlag { get; set; }

        [MaxLength(1)]
        public string AddressOther { get; set; }

        [MaxLength(20)]
        public string SuiteNumber { get; set; }

        [MaxLength(80)]
        public string StreetName { get; set; }

        [MaxLength(60)]
        public string CityName { get; set; }

        [MaxLength(40)]
        public string Province { get; set; }

        [MaxLength(100)]
        public string ProvinceFrench { get; set; }

        [MaxLength(40)]
        public string Country { get; set; }

        [MaxLength(100)]
        public string CountryFrench { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(15)]
        public string PostOfficeBox { get; set; }
    }
}
