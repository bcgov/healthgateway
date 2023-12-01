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
using HealthGateway.Admin.Common.Models;

/// <summary>
/// The state for the feature.
/// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
/// </summary>
[FeatureState]
public record DashboardState
{
    /// <summary>
    /// Gets the request state for retrieving daily counts of user registrations.
    /// </summary>
    public BaseRequestState<IDictionary<DateOnly, int>> GetDailyUserRegistrationCounts { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving daily counts of dependent registrations.
    /// </summary>
    public BaseRequestState<IDictionary<DateOnly, int>> GetDailyDependentRegistrationCounts { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving daily counts of unique user logins.
    /// </summary>
    public BaseRequestState<IDictionary<DateOnly, int>> GetDailyUniqueLoginCounts { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving a recurring user count.
    /// </summary>
    public BaseRequestState<int?> GetRecurringUserCount { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving app login counts.
    /// </summary>
    public BaseRequestState<AppLoginCounts> GetAppLoginCounts { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving a ratings summary.
    /// </summary>
    public BaseRequestState<IDictionary<string, int>> GetRatingsSummary { get; init; } = new();

    /// <summary>
    /// Gets the request state for retrieving age counts.
    /// </summary>
    public BaseRequestState<IDictionary<int, int>> GetAgeCounts { get; init; } = new();

    /// <summary>
    /// Gets age counts associated with the most recent query.
    /// </summary>
    public IDictionary<int, int> AgeCounts { get; init; } = ImmutableDictionary<int, int>.Empty;
}
