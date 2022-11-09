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
    /// Represents configuration for a job instance.
    /// </summary>
    public class JobConfiguration
    {
        /// <summary>
        /// Gets or sets the Job Id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the CRON schedule for the job.
        /// </summary>
        public string? Schedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the job should be run near immediately after scheduling.
        /// </summary>
        public bool Immediate { get; set; }

        /// <summary>
        /// Gets or sets the delay when queueing the immediate job.
        /// This value is required as the jobs are schedued async and the DB may not be setup yet.
        /// </summary>
        public int Delay { get; set; }
    }
}
