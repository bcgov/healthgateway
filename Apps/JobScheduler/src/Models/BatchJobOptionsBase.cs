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
    using System;

    /// <summary>
    /// Base configuration options for batch jobs that process user data in chunks.
    /// Provides common settings for enabling/disabling execution, identifying the job,
    /// controlling batch size, and optionally filtering records by last login date.
    /// </summary>
    public class BatchJobOptionsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the job is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the short logical job name.
        /// </summary>
        public string JobName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the batch size.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the minimum last login date/time required for processing.
        /// When null, no last login cutoff is applied.
        /// </summary>
        public DateTime LastLoginAfterDate { get; set; }
    }
}
