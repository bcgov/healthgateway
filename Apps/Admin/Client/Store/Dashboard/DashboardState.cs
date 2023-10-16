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

namespace HealthGateway.Admin.Client.Store.Dashboard;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Fluxor;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
/// </summary>
[FeatureState]
public record DashboardState
{
    /// <summary>
    /// Gets the request state for retrieving registered users.
    /// </summary>
    public BaseRequestState<IDictionary<DateTime, int>> GetRegisteredUsers { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving logged in users.
    /// </summary>
    public BaseRequestState<IDictionary<DateTime, int>> GetLoggedInUsers { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving dependents.
    /// </summary>
    public BaseRequestState<IDictionary<DateTime, int>> GetDependents { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving user counts.
    /// </summary>
    public BaseRequestState<IDictionary<string, int>> GetUserCounts { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving a rating summary.
    /// </summary>
    public BaseRequestState<IDictionary<string, int>> GetRatingSummary { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving year of birth counts.
    /// </summary>
    public BaseRequestState<IDictionary<string, int>> GetYearOfBirthCounts { get; init; } = new();

    /// <summary>
    /// Gets year of birth counts associated with the most recent query.
    /// </summary>
    public IDictionary<string, int> YearOfBirthCounts { get; init; } = ImmutableDictionary<string, int>.Empty;
}
