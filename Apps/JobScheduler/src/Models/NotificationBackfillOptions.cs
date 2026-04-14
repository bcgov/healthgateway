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
namespace HealthGateway.JobScheduler.Models
{
    /// <summary>
    /// Configuration options for the notification backfill job.
    /// </summary>
    public class NotificationBackfillOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the job is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the short logical job name.
        /// </summary>
        public string JobName { get; set; } = "NotificationBackfill";

        /// <summary>
        /// Gets or sets the notification type to backfill.
        /// </summary>
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the batch size.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value indicating whether to target users with a valid SMS number
        /// instead of valid email when selecting user profiles for processing.
        /// </summary>
        public bool UseSmsChannel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether email notifications are enabled.
        /// A value of null indicates the setting has not been set or processed.
        /// </summary>
        public bool? EmailEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS notifications are enabled.
        /// A value of null indicates the setting has not been set or processed.
        /// </summary>
        public bool? SmsEnabled { get; set; }
    }
}
