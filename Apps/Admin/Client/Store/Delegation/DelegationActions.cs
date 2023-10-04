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
namespace HealthGateway.Admin.Client.Store.Delegation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class DelegationActions
    {
        /// <summary>
        /// The action representing the initiation of a search.
        /// </summary>
        public record SearchAction
        {
            /// <summary>
            /// Gets the PHN to query on.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a failed search.
        /// </summary>
        public record SearchFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful search.
        /// </summary>
        public record SearchSuccessAction
        {
            /// <summary>
            /// Gets the dependent info.
            /// </summary>
            public required DependentInfo Dependent { get; init; }

            /// <summary>
            /// Gets the collection of agent actions.
            /// </summary>
            public required IEnumerable<AgentAction> AgentActions { get; init; }

            /// <summary>
            /// Gets the collection of delegate info.
            /// </summary>
            public required IEnumerable<ExtendedDelegateInfo> Delegates { get; init; }
        }

        /// <summary>
        /// The action representing the initiation of a delegate search.
        /// </summary>
        public record DelegateSearchAction
        {
            /// <summary>
            /// Gets the PHN to query on.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a failed delegate search.
        /// </summary>
        public record DelegateSearchFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful delegate search.
        /// </summary>
        public record DelegateSearchSuccessAction : BaseSuccessAction<ExtendedDelegateInfo>;

        /// <summary>
        /// The action that adds a retrieved delegate in a staged state.
        /// </summary>
        public record AddDelegateAction
        {
            /// <summary>
            /// Gets the staged delegation status the delegate should have.
            /// </summary>
            public required DelegationStatus StagedDelegationStatus { get; init; }
        }

        /// <summary>
        /// The action that sets whether a delegation should be staged to be removed.
        /// </summary>
        public record SetDisallowedDelegationStatusAction
        {
            /// <summary>
            /// Gets the HDID associated with the delegate.
            /// </summary>
            public required string Hdid { get; init; }

            /// <summary>
            /// Gets a value indicating whether the delegation should be disallowed.
            /// </summary>
            public required bool Disallow { get; init; }
        }

        /// <summary>
        /// The action representing the initiation of a protect dependent.
        /// </summary>
        public record ProtectDependentAction
        {
            /// <summary>
            /// Gets the audit reason.
            /// </summary>
            public required string Reason { get; init; }
        }

        /// <summary>
        /// The action representing a protect dependent failure.
        /// </summary>
        public record ProtectDependentFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a protect dependent success.
        /// </summary>
        public record ProtectDependentSuccessAction
        {
            /// <summary>
            /// Gets the agent action entry created from the operation.
            /// </summary>
            public required AgentAction AgentAction { get; init; }
        }

        /// <summary>
        /// The action representing the initiation of an unprotect dependent.
        /// </summary>
        public record UnprotectDependentAction
        {
            /// <summary>
            /// Gets the audit reason.
            /// </summary>
            public required string Reason { get; init; }
        }

        /// <summary>
        /// The action representing a failed unprotect dependent.
        /// </summary>
        public record UnprotectDependentFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful unprotect dependent.
        /// </summary>
        public record UnprotectDependentSuccessAction
        {
            /// <summary>
            /// Gets the agent action entry created from the operation.
            /// </summary>
            public required AgentAction AgentAction { get; init; }
        }

        /// <summary>
        /// The action that changes whether the delegation page is in edit mode.
        /// </summary>
        public record SetEditModeAction
        {
            /// <summary>
            /// Gets a value indicating whether edit mode should be enabled.
            /// </summary>
            public required bool Enabled { get; init; }
        }

        /// <summary>
        /// The action that clears the delegate search.
        /// </summary>
        public record ClearDelegateSearchAction;

        /// <summary>
        /// The action that clears any error encountered during a protect.
        /// </summary>
        public record ClearProtectErrorAction;

        /// <summary>
        /// The action that clears any error encountered during an unprotect.
        /// </summary>
        public record ClearUnprotectErrorAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
