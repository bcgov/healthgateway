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
namespace HealthGateway.Admin.Common.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// Represents details associated with a patient retrieved by a support query.
    /// </summary>
    public class PatientSupportDetails
    {
        /// <summary>
        /// Gets or sets the patient's status.
        /// </summary>
        public IEnumerable<MessagingVerificationModel> MessagingVerifications { get; set; } = Enumerable.Empty<MessagingVerificationModel>();

        /// <summary>
        /// Gets or sets a warning message associated with the patient.
        /// </summary>
        public IEnumerable<AgentAction> AgentActions { get; set; } = Enumerable.Empty<AgentAction>();

        /// <summary>
        /// Gets or sets the blocked access data sources.
        /// </summary>
        public IEnumerable<DataSource> BlockedDataSources { get; set; } = Enumerable.Empty<DataSource>();
    }
}
