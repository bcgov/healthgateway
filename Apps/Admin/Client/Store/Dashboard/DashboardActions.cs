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

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class DashboardActions
{
    /// <summary>
    /// The action representing the initiation of a registered users load.
    /// </summary>
    public record LoadRegisteredUsersAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful registered users load.
    /// </summary>
    public record RegisteredUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed registered users load.
    /// </summary>
    public record RegisteredUsersFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a logged in users load.
    /// </summary>
    public record LoadLoggedInUsersAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful logged in users load.
    /// </summary>
    public record LoggedInUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed logged in users load.
    /// </summary>
    public record LoggedInUsersFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a dependents load.
    /// </summary>
    public record LoadDependentsAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful dependents load.
    /// </summary>
    public record DependentsSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed dependents load.
    /// </summary>
    public record DependentsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a recurring users load.
    /// </summary>
    public record LoadRecurringUsersAction
    {
        /// <summary>
        /// Gets the minimum number of unique days logged in required to qualify as a recurring user.
        /// </summary>
        public required int Days { get; init; }

        /// <summary>
        /// Gets the start of the period to evaluate.
        /// </summary>
        public required string StartPeriod { get; init; }

        /// <summary>
        /// Gets the end of the period to evaluate.
        /// </summary>
        public required string EndPeriod { get; init; }

        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful recurring users load.
    /// </summary>
    public record RecurringUsersSuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed recurring users load.
    /// </summary>
    public record RecurringUsersFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a rating summary load.
    /// </summary>
    public record LoadRatingSummaryAction
    {
        /// <summary>
        /// Gets the start of the period to evaluate.
        /// </summary>
        public required string StartPeriod { get; init; }

        /// <summary>
        /// Gets the end of the period to evaluate.
        /// </summary>
        public required string EndPeriod { get; init; }

        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful rating summary load.
    /// </summary>
    public record RatingSummarySuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed rating summary load.
    /// </summary>
    public record RatingSummaryFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of year of birth counts.
    /// </summary>
    public record GetYearOfBirthCountsAction
    {
        /// <summary>
        /// Gets the start of the period to evaluate.
        /// </summary>
        public required string StartPeriod { get; init; }

        /// <summary>
        /// Gets the end of the period to evaluate.
        /// </summary>
        public required string EndPeriod { get; init; }

        /// <summary>
        /// Gets the offset from the client browser to UTC.
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
