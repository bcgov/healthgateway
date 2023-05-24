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
    using System.Collections.Immutable;
    using Fluxor;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// The state for the feature.
    /// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
    /// </summary>
    [FeatureState]
    public record DelegationState
    {
        /// <summary>
        /// Gets the request state for searches.
        /// </summary>
        public BaseRequestState<DelegationInfo> Search { get; init; } = new();

        /// <summary>
        /// Gets the request state for delegate searches.
        /// </summary>
        public BaseRequestState<ExtendedDelegateInfo> DelegateSearch { get; init; } = new();

        /// <summary>
        /// Gets the request state for protects.
        /// </summary>
        public BaseRequestState Protect { get; init; } = new();

        /// <summary>
        /// Gets the request state for unprotects.
        /// </summary>
        public BaseRequestState Unprotect { get; init; } = new();

        /// <summary>
        /// Gets the dependent info.
        /// </summary>
        public DependentInfo? Dependent { get; init; }

        /// <summary>
        /// Gets the collection of agent actions.
        /// </summary>
        public IImmutableList<AgentAction> AgentActions { get; init; } = ImmutableList<AgentAction>.Empty;

        /// <summary>
        /// Gets the collection of delegate info.
        /// </summary>
        public IImmutableList<ExtendedDelegateInfo> Delegates { get; init; } = ImmutableList<ExtendedDelegateInfo>.Empty;

        /// <summary>
        /// Gets a value indicating whether the delegation page is in edit mode.
        /// </summary>
        public bool InEditMode { get; init; }
    }
}
