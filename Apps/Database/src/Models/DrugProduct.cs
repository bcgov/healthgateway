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
namespace HealthGateway.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Runtime.Serialization;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The entity representing a Drug Product from the Federal Drug Database.
    /// </summary>
    public class DrugProduct : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the internal Guid representing the DrugProduct.
        /// </summary>
        [Column("DrugProductId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the source Drug Code from the Government extract QRYM_DRUG_PRODUCT.
        /// </summary>
        [Required]
        [MaxLength(8)]
        public string? DrugCode { get; set; }

        /// <summary>
        /// Gets or sets the Drug Production Categorization.
        /// </summary>
        [MaxLength(80)]
        public string? ProductCategorization { get; set; }

        /// <summary>
        /// Gets or sets the Drug Class in French.
        /// </summary>
        [MaxLength(40)]
        public string? DrugClass { get; set; }

        /// <summary>
        /// Gets or sets the Drug Class.
        /// </summary>
        [MaxLength(80)]
        public string? DrugClassFrench { get; set; }

        /// <summary>
        /// Gets or sets the Drug Identification Number.
        /// </summary>
        [MaxLength(29)]
        public string DrugIdentificationNumber { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Brand name of the Drug.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string BrandName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Brand name of the Drug in French.
        /// </summary>
        [MaxLength(300)]
        public string? BrandNameFrench { get; set; }

        /// <summary>
        /// Gets or sets the Descriptor for the Drug.
        /// </summary>
        [MaxLength(150)]
        public string? Descriptor { get; set; }

        /// <summary>
        /// Gets or sets the Descriptor for the Drug in French.
        /// </summary>
        [MaxLength(200)]
        public string? DescriptorFrench { get; set; }

        /// <summary>
        /// Gets or sets the Pediatric flag.
        /// </summary>
        [MaxLength(1)]
        public string? PediatricFlag { get; set; }

        /// <summary>
        /// Gets or sets the Accession number.
        /// </summary>
        [MaxLength(5)]
        public string? AccessionNumber { get; set; }

        /// <summary>
        /// Gets or sets the Number of AIS.
        /// </summary>
        [MaxLength(10)]
        public string? NumberOfAis { get; set; }

        /// <summary>
        /// Gets or sets the Last Update from the Fed DB.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets the AI Group Number.
        /// </summary>
        [MaxLength(10)]
        public string? AiGroupNumber { get; set; }

        /// <summary>
        /// Gets or sets the assoicated File Download ID.
        /// </summary>
        [Required]
        public Guid FileDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the FileDownload entity.
        /// Code first mechanism to define the foreign key.
        /// </summary>
        public virtual FileDownload? FileDownload { get; set; }

        /// <summary>
        /// Gets or sets the Manufacturer.
        /// </summary>
        public virtual Company? Company { get; set; }

        /// <summary>
        /// Gets or sets the ActiveIngredient.
        /// </summary>
        public virtual ActiveIngredient? ActiveIngredient { get; set; }

        /// <summary>
        /// Gets or sets the Form.
        /// </summary>
        public virtual Form? Form { get; set; }

        /// <summary>
        /// Gets or sets the Packaging.
        /// </summary>
        public virtual Packaging? Packaging { get; set; }

        /// <summary>
        /// Gets or sets the PharmaceuticalStd.
        /// </summary>
        public virtual PharmaceuticalStd? PharmaceuticalStd { get; set; }

        /// <summary>
        /// Gets or sets the Route.
        /// </summary>
        public virtual Route? Route { get; set; }

        /// <summary>
        /// Gets or sets the related statuses.
        /// </summary>
#pragma warning disable CA2227
        public virtual ICollection<Status>? Statuses { get; set; }
#pragma warning restore CA2227

        /// <summary>
        /// Gets or sets the related TherapeuticClass.
        /// </summary>
        public virtual TherapeuticClass? TherapeuticClass { get; set; }

        /// <summary>
        /// Gets or sets the related VeterinarySpecies.
        /// </summary>
        public virtual VeterinarySpecies? VeterinarySpecies { get; set; }
    }
}
