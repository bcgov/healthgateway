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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Configuration object for the Notify API.
    /// </summary>
    public class NotifyConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether GC Notify is enabled.
        /// </summary>
        [Required]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the API Key.
        /// </summary>
        [Required]
        public required string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the API URL.
        /// </summary>
        [Required]
        public required Uri ApiUrl { get; set; }

        /// <summary>
        /// Gets the template name to GC Notify template ID mapping.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required Dictionary<string, Guid> Templates { get; init; }
    }
}
