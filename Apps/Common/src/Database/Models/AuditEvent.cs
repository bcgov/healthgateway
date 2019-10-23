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
namespace HealthGateway.Common.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class AuditEvent : AuditableEntity
    {
        /// <summary>
        /// The surrogate key for the AuditEvent.
        /// </summary>
        [Required]
        public Guid AuditEventId { get; set; }

        /// <summary>
        /// The date/time the audit event occurred.
        /// </summary>
        [Required]
        public DateTime AuditEventDateTime { get; set; }

        /// <summary>
        /// The string representation of the IPV4 address of the client.
        /// </summary>
        [MaxLength(15)]
        [Required]
        public string ClientIP { get; set; }

        /// <summary>
        /// The application specific subject identifer.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string ApplicationSubject { get; set; }

        /// <summary>
        /// The application name recording the event.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public Applications ApplicationName { get; set; }

        /// <summary>
        /// The transacation within the application causing the event.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string TransacationName { get; set; }

        /// <summary>
        /// The version of the transaction.
        /// </summary>
        [MaxLength(5)]
        public string TransactionVersion { get; set; }

        /// <summary>
        /// The trace value for the audit event
        /// </summary>
        ///
        [MaxLength(20)]
        public string Trace { get; set; }

        /// <summary>
        /// The result code/status code from the transaction.
        /// </summary>
        [Required]
        public AuditTransactionResult TransactionResult { get; set; }

        /// <summary>
        /// The duration of the transaction in milliseconds.
        /// </summary>
        public long? TransactionDuration { get; set; }

        public AuditEvent()
        {
        }
    }
}
