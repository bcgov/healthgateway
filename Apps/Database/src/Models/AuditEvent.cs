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
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Database.Constants;

    /// <summary>
    /// Represents an AuditEvent entity
    /// These events are recorded on all interactions with the Health Gateway controllers.
    /// </summary>
    public class AuditEvent : AuditableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditEvent"/> class.
        /// </summary>
        public AuditEvent()
        {
        }

        /// <summary>
        /// Gets or sets the surrogate key for the AuditEvent.
        /// </summary>
        [Required]
        [Column("AuditEventId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the date/time the audit event occurred.
        /// </summary>
        [Required]
        public DateTime AuditEventDateTime { get; set; }

        /// <summary>
        /// Gets or sets the string representation of the IPV4 address of the client.
        /// </summary>
        [MaxLength(15)]
        [Required]
        [Column("ClientIP")]
        public string? ClientIp { get; set; }

        /// <summary>
        /// Gets or sets the application specific subject identifer.
        /// </summary>
        [MaxLength(100)]
        public string? ApplicationSubject { get; set; }

        /// <summary>
        /// Gets or sets the application name recording the event.
        /// </summary>
        [MaxLength(10)]
        [Required]
        public string? ApplicationType { get; set; }

        /// <summary>
        /// Gets or sets the transacation within the application causing the event.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string? TransactionName { get; set; }

        /// <summary>
        /// Gets or sets the version of the transaction.
        /// </summary>
        [MaxLength(5)]
        public string? TransactionVersion { get; set; }

        /// <summary>
        /// Gets or sets the trace value for the audit event.
        /// </summary>
        [MaxLength(200)]
        public string? Trace { get; set; }

        /// <summary>
        /// Gets or sets the result code/status code from the transaction.
        /// </summary>
        [Required]
        [MaxLength(10)]
        public AuditTransactionResult TransactionResultCode { get; set; }

        /// <summary>
        /// Gets or sets the duration of the transaction in milliseconds.
        /// </summary>
        public long? TransactionDuration { get; set; }
    }
}
