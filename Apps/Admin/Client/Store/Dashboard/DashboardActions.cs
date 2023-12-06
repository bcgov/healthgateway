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
using System.Diagnostics.CodeAnalysis;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class DashboardActions
{
    /// <summary>
    /// The action representing the initiation of a retrieval of all-time counts.
    /// </summary>
    public record GetAllTimeCountsAction;

    /// <summary>
    /// The action representing a successful retrieval of all-time counts.
    /// </summary>
    public record GetAllTimeCountsSuccessAction : BaseSuccessAction<AllTimeCounts>;

    /// <summary>
    /// The action representing a failed retrieval of all-time counts.
    /// </summary>
    public record GetAllTimeCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of daily usage counts.
    /// </summary>
    public record GetDailyUsageCountsAction
    {
        /// <summary>
        /// Gets the local start date to query.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the local end date to query.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of daily usage counts.
    /// </summary>
    public record GetDailyUsageCountsSuccessAction : BaseSuccessAction<DailyUsageCounts>;

    /// <summary>
    /// The action representing a failed retrieval of daily usage counts.
    /// </summary>
    public record GetDailyUsageCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of a recurring user count.
    /// </summary>
    public record GetRecurringUserCountAction
    {
        /// <summary>
        /// Gets the minimum number of days users must have logged in within the period to count as recurring.
        /// </summary>
        public required int Days { get; init; }

        /// <summary>
        /// Gets the local start date to query.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the local end date to query.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of a recurring user count.
    /// </summary>
    public record GetRecurringUserCountSuccessAction : BaseSuccessAction<int>;

    /// <summary>
    /// The action representing a failed retrieval of a recurring user count.
    /// </summary>
    public record GetRecurringUserCountFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of app login counts.
    /// </summary>
    public record GetAppLoginCountsAction
    {
        /// <summary>
        /// Gets the local start date to query.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the local end date to query.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of app login counts.
    /// </summary>
    public record GetAppLoginCountsSuccessAction : BaseSuccessAction<AppLoginCounts>;

    /// <summary>
    /// The action representing a failed retrieval of app login counts.
    /// </summary>
    public record GetAppLoginCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of a ratings summary.
    /// </summary>
    public record GetRatingsSummaryAction
    {
        /// <summary>
        /// Gets the local start date to query.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the local end date to query.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of a ratings summary.
    /// </summary>
    public record GetRatingsSummarySuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed retrieval of a ratings summary.
    /// </summary>
    public record GetRatingsSummaryFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of age counts.
    /// </summary>
    public record GetAgeCountsAction
    {
        /// <summary>
        /// Gets the local start date to query.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the local end date to query.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of age counts.
    /// </summary>
    public record GetAgeCountsSuccessAction : BaseSuccessAction<IDictionary<int, int>>;

    /// <summary>
    /// The action representing a failed retrieval of age counts.
    /// </summary>
    public record GetAgeCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
