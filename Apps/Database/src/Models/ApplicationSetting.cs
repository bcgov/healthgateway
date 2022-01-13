// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// A data store for application settings that are updated at runntime.
    /// </summary>
    public class ApplicationSetting : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Column("ApplicationSettingsId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string? Application { get; set; }

        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string? Component { get; set; }

        /// <summary>
        /// Gets or sets the Key name.
        /// </summary>
        [Required]
        [MaxLength(250)]
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string? Value { get; set; }
    }
}
