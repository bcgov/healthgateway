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
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The dependent audit model.
    /// </summary>
    public class DependentAudit : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid DependentAuditId { get; set; }

        /// <summary>
        /// Gets or sets the delegate hdid.
        /// </summary>
        [MaxLength(52)]
        public string DelegateHdId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the agent username.
        /// </summary>
        [MaxLength(255)]
        public string AgentUsername { get; set; } = null!;

        /// <summary>
        /// Gets or sets the protected reason.
        /// </summary>
        [MaxLength(500)]
        public string ProtectedReason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value representing the type of dependent audit operation.
        /// The value is one of <see cref="DependentAuditOperation"/>.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public DependentAuditOperation OperationCode { get; set; } = DependentAuditOperation.Unprotect;

        /// <summary>
        /// Gets or sets the transaction datetime.
        /// </summary>
        [Required]
        public DateTime TransactionDateTime { get; set; } = DateTime.MaxValue;
    }
}
