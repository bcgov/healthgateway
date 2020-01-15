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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents a request to access the service in beta mode.
    /// </summary>
    public class BetaRequest : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the user id for the request.
        /// </summary>
        [Key]
        [Column("BetaRequestId")]
        [MaxLength(52)]
        public string HdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email for the request.
        /// </summary>
        [MaxLength(254)]
        [Required]
        public string EmailAddress { get; set; } = null!;
    }
}