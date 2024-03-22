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

namespace HealthGateway.Admin.Client.Store.BetaAccess;

using System.Collections.Generic;
using System.Collections.Immutable;
using Fluxor;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
/// </summary>
[FeatureState]
public record BetaAccessState
{
    /// <summary>
    /// Gets the request state for setting user access.
    /// </summary>
    public BaseRequestState SetUserAccess { get; init; } = new();

    /// <summary>
    /// Gets the request state for getting user access.
    /// </summary>
    public BaseRequestState<UserBetaAccess> GetUserAccess { get; init; } = new();

    /// <summary>
    /// Gets the request state for getting all user access.
    /// </summary>
    public BaseRequestState<IEnumerable<UserBetaAccess>> GetAllUserAccess { get; init; } = new();

    /// <summary>
    /// Gets all user access, indexed by email address.
    /// </summary>
    public IImmutableDictionary<string, UserBetaAccess>? AllUserAccess { get; init; }

    /// <summary>
    /// Gets the latest result from getting user access.
    /// </summary>
    public UserBetaAccess? SearchResult { get; init; }
}
