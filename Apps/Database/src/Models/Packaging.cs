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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS1591 // self explanatory simple model
#pragma warning disable SA1600 // self explanatory simple model
    public class Packaging : AuditableEntity
    {
        [Column("PackagingId")]
        public Guid Id { get; set; }

        [MaxLength(12)]
        [Column("UPC")]
        public string? Upc { get; set; }

        [MaxLength(40)]
        public string? PackageType { get; set; }

        [MaxLength(80)]
        public string? PackageTypeFrench { get; set; }

        [MaxLength(40)]
        public string? PackageSizeUnit { get; set; }

        [MaxLength(80)]
        public string? PackageSizeUnitFrench { get; set; }

        [MaxLength(5)]
        public string? PackageSize { get; set; }

        [MaxLength(4000)]
        public string? ProductInformation { get; set; }

        /// <summary>
        /// Gets or sets the Drug Product foreign key.
        /// </summary>
        [Required]
        public Guid DrugProductId { get; set; }
    }
}
