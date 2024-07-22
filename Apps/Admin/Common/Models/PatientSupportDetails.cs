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
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Represents details associated with a patient retrieved by a support query.
    /// </summary>
    public class PatientSupportDetails
    {
        /// <summary>
        /// Gets or sets the messaging verifications.
        /// </summary>
        public IEnumerable<MessagingVerificationModel>? MessagingVerifications { get; set; } = [];

        /// <summary>
        /// Gets or sets the agent actions.
        /// </summary>
        public IEnumerable<AgentAction>? AgentActions { get; set; } = [];

        /// <summary>
        /// Gets or sets the blocked data sources.
        /// </summary>
        public IEnumerable<DataSource>? BlockedDataSources { get; set; } = [];

        /// <summary>
        /// Gets or sets the dependents.
        /// </summary>
        public IEnumerable<PatientSupportDependentInfo>? Dependents { get; set; } = [];

        /// <summary>
        /// Gets the vaccine details.
        /// </summary>
        public VaccineDetails? VaccineDetails { get; init; }
    }
}
