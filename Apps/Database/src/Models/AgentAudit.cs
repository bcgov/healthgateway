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
    using HealthGateway.Common.Data.Constants;

    /// <summary>
    /// The agent audit model.
    /// </summary>
    public class AgentAudit : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("AgentAuditId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the hdid that is affected by the audit operation.
        /// </summary>
        [Required]
        [MaxLength(52)]
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agent username.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string AgentUsername { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value representing the type of audit operation.
        /// The value is one of <see cref="AuditOperation"/>.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public AuditOperation OperationCode { get; set; } = AuditOperation.UnprotectDependent;

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public AuditGroup GroupCode { get; set; } = AuditGroup.Dependent;

        /// <summary>
        /// Gets or sets the transaction datetime.
        /// </summary>
        [Required]
        public DateTime TransactionDateTime { get; set; } = DateTime.MaxValue;
    }
}
