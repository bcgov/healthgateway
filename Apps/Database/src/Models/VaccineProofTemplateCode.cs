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
    using System.ComponentModel.DataAnnotations;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Object code table for VaccineProofTemplate.
    /// </summary>
    public class VaccineProofTemplateCode : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the template code.
        /// </summary>
        [Key]
        [Required]
        [MaxLength(15)]
        public VaccineProofTemplate TemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the template code description.
        /// </summary>
        [Required]
        [MaxLength(75)]
        public string? Description { get; set; }
    }
}
