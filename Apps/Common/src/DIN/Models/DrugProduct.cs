﻿//-------------------------------------------------------------------------
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
namespace HealthGateway.DIN.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using HealthGateway.Common.Database.Models;

    public class DrugProduct : AuditableEntity
    {
        /// <summary>
        /// The internal Guid representing the DrugProduct.
        /// </summary>
        public Guid DrugProductId { get; set; }

        /// <summary>
        /// The source Drug Code from the Government extract QRYM_DRUG_PRODUCT
        /// </summary>
        [Required]
        [MaxLength(8)]
        public string DrugCode { get; set; }

        /// <summary>
        /// The Drug Production Categorization.
        /// </summary>
        [MaxLength(80)]
        public string ProductCategorization { get; set; }

        /// <summary>
        /// The Drug Class in French
        /// </summary>
        [MaxLength(40)]
        public string DrugClass { get; set; }

        /// <summary>
        /// The Drug Class
        /// </summary>
        [MaxLength(80)]
        public string DrugClassFrench { get; set; }

        /// <summary>
        /// The Drug Identification Number.
        /// </summary>
        [MaxLength(29)]
        public string DrugIdentificationNumber { get; set; }

        /// <summary>
        /// The Brand name of the Drug.
        /// </summary>
        [MaxLength(200)]
        public string BrandName { get; set; }

        /// <summary>
        /// The Brand name of the Drug in French.
        /// </summary>
        [MaxLength(300)]
        public string BrandNameFrench { get; set; }

        /// <summary>
        /// The Descriptor for the Drug.
        /// </summary>
        [MaxLength(150)]
        public string Descriptor { get; set; }

        /// <summary>
        /// The Descriptor for the Drug in French
        /// </summary>
        [MaxLength(200)]
        public string DescriptorFrench { get; set; }

        /// <summary>
        /// The Pediatric flag.
        /// </summary>
        [MaxLength(1)]
        public string PediatricFlag { get; set; }

        /// <summary>
        /// The Accession number.
        /// </summary>
        [MaxLength(5)]
        public string AccessionNumber { get; set; }

        /// <summary>
        /// The Number of AIS.
        /// </summary>
        [MaxLength(10)]
        public string NumberOfAis { get; set; }

        /// <summary>
        /// The Last Update from the Fed DB.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// The AI Group Number.
        /// </summary>
        [MaxLength(10)]
        public string AiGroupNumber { get; set; }
    }
}
