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
    /// The action representing the initiation of a retrieval of daily user registration counts.
    /// </summary>
    public record GetDailyUserRegistrationCountsAction
    {
        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of daily user registration counts.
    /// </summary>
    public record GetDailyUserRegistrationCountsSuccessAction : BaseSuccessAction<IDictionary<DateOnly, int>>;

    /// <summary>
    /// The action representing a failed retrieval of daily user registration counts.
    /// </summary>
    public record GetDailyUserRegistrationCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of daily dependent registration counts.
    /// </summary>
    public record GetDailyDependentRegistrationCountsAction
    {
        /// <summary>
        /// Gets the local timezone offset from UTC in minutes.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of daily dependent registration counts.
    /// </summary>
    public record GetDailyDependentRegistrationCountsSuccessAction : BaseSuccessAction<IDictionary<DateOnly, int>>;

    /// <summary>
    /// The action representing a failed retrieval of daily dependent registration counts.
    /// </summary>
    public record GetDailyDependentRegistrationCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of daily unique login counts.
    /// </summary>
    public record GetDailyUniqueLoginCountsAction
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
    /// The action representing a successful retrieval of daily unique login counts.
    /// </summary>
    public record GetDailyUniqueLoginCountsSuccessAction : BaseSuccessAction<IDictionary<DateOnly, int>>;

    /// <summary>
    /// The action representing a failed retrieval of daily unique login counts.
    /// </summary>
    public record GetDailyUniqueLoginCountsFailureAction : BaseFailureAction;

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
    /// The action representing the initiation of a retrieval of year of birth counts.
    /// </summary>
    public record GetYearOfBirthCountsAction
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
    /// The action representing a successful retrieval of year of birth counts.
    /// </summary>
    public record GetYearOfBirthCountsSuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed retrieval of year of birth counts.
    /// </summary>
    public record GetYearOfBirthCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
