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
namespace HealthGateway.Admin.Client.Store.AdminReport
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Common.Models.AdminReports;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class AdminReportActions
    {
        /// <summary>
        /// The action representing the request to retrieve users with blocked data sources.
        /// </summary>
        public record GetBlockedAccessAction;

        /// <summary>
        /// The action representing a successful request to retrieve users with blocked data sources.
        /// </summary>
        public record GetBlockedAccessSuccessAction : BaseSuccessAction<IEnumerable<BlockedAccessRecord>>;

        /// <summary>
        /// The action representing a failed request to retrieve users with blocked data sources.
        /// </summary>
        public record GetBlockedAccessFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing the request to retrieve protected dependents.
        /// </summary>
        public record GetProtectedDependentsAction;

        /// <summary>
        /// The action representing a successful request to retrieve protected dependents.
        /// </summary>
        public record GetProtectedDependentsSuccessAction : BaseSuccessAction<IEnumerable<string>>;

        /// <summary>
        /// The action representing a failed request to retrieve protected dependents.
        /// </summary>
        public record GetProtectedDependentsFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
