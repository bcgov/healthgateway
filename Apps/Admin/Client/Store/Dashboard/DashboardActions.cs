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
    /// The action representing the initiation of a retrieval of registered users.
    /// </summary>
    public record GetRegisteredUsersAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of registered users.
    /// </summary>
    public record GetRegisteredUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed retrieval of registered users.
    /// </summary>
    public record GetRegisteredUsersFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of logged in users.
    /// </summary>
    public record GetLoggedInUsersAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of logged in users.
    /// </summary>
    public record GetLoggedInUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed retrieval of logged in users.
    /// </summary>
    public record GetLoggedInUsersFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of dependents.
    /// </summary>
    public record GetDependentsAction
    {
        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful retrieval of dependents.
    /// </summary>
    public record GetDependentsSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>;

    /// <summary>
    /// The action representing a failed retrieval of dependents.
    /// </summary>
    public record GetDependentsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of user counts.
    /// </summary>
    public record GetUserCountsAction
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
    /// The action representing a successful retrieval of user counts.
    /// </summary>
    public record GetUserCountsSuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed retrieval of user counts.
    /// </summary>
    public record GetUserCountsFailureAction : BaseFailureAction;

    /// <summary>
    /// The action representing the initiation of a retrieval of a ratings summary.
    /// </summary>
    public record GetRatingSummaryAction
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
    /// The action representing a successful retrieval of a ratings summary.
    /// </summary>
    public record GetRatingSummarySuccessAction : BaseSuccessAction<IDictionary<string, int>>;

    /// <summary>
    /// The action representing a failed retrieval of a ratings summary.
    /// </summary>
    public record GetRatingSummaryFailureAction : BaseFailureAction;

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
