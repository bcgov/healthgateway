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
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The user profile notification setting model.
    /// </summary>
    [Index(nameof(Hdid), nameof(NotificationTypeCode), IsUnique = true)]
    public class UserProfileNotificationSetting : AuditableEntity
    {
        /// <summary>
        /// Gets the unique identifier for this notification setting.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        /// <summary>
        /// Gets the unique Health Data ID (HDID) of the user
        /// to whom these notification settings apply.
        /// </summary>
        [Column("UserProfileId")]
        [MaxLength(52)]
        public string Hdid { get; init; } = null!;

        /// <summary>
        /// Gets the notification type code for which these
        /// delivery channel settings apply.
        /// </summary>
        [MaxLength(50)]
        public ProfileNotificationType NotificationTypeCode { get; init; }

        /// <summary>
        /// Gets and sets a value indicating whether email notifications
        /// are enabled for this notification type.
        /// </summary>
        public bool EmailEnabled { get; set; }

        /// <summary>
        /// Gets and sets a value indicating whether SMS notifications
        /// are enabled for this notification type.
        /// </summary>
        public bool SmsEnabled { get; set; }
    }
}
