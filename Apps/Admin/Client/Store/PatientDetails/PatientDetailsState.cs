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

namespace HealthGateway.Admin.Client.Store.PatientDetails
{
    using System.Collections.Immutable;
    using Fluxor;
    using HealthGateway.Admin.Common.Models;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// The state for the feature.
    /// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
    /// </summary>
    [FeatureState]
    public record PatientDetailsState : BaseRequestState<PatientSupportDetails>
    {
        /// <summary>
        /// Gets the message verifications linked to the patient support details.
        /// </summary>
        public ImmutableList<MessagingVerificationModel>? MessagingVerifications { get; init; }

        /// <summary>
        /// Gets the agent actions linked to the patient support details.
        /// </summary>
        public ImmutableList<AgentAction>? AgentActions { get; init; }

        /// <summary>
        /// Gets the dependents linked to the patient support details.
        /// </summary>
        public ImmutableList<PatientSupportDependentInfo>? Dependents { get; init; }

        /// <summary>
        /// Gets the blocked data sources linked to the patient support details.
        /// </summary>
        public ImmutableList<DataSource>? BlockedDataSources { get; init; }

        /// <summary>
        /// Gets the vaccine details linked to the patient support details.
        /// </summary>
        public VaccineDetails? VaccineDetails { get; init; }

        /// <summary>
        /// Gets the request state for block access requests.
        /// </summary>
        public BaseRequestState BlockAccess { get; init; } = new();
    }
}
