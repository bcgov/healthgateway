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
    using HealthGateway.Database.Constants;

    /// <summary>
    /// A system Communication Email.
    /// </summary>
    public class CommunicationEmail : AuditableEntity
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("CommunicationEmailId")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Communication Id.
        /// </summary>
        public Guid CommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the Email Id.
        /// </summary>
        public Guid EmailId { get; set; }

        /// <summary>
        /// Gets or sets the User Profile Id.
        /// </summary>
        public Guid UserProfileId { get; set; }
    }
}
