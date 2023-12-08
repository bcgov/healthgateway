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
namespace HealthGateway.Admin.Client.Store.Analytics;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

/// <summary>
/// Static class that implements all actions for the feature.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
public static class AnalyticsActions
{
    /// <summary>
    /// The action representing the initiation of a user profiles load.
    /// </summary>
    public record LoadUserProfilesAction;

    /// <summary>
    /// The action representing the initiation of a comments load.
    /// </summary>
    public record LoadCommentsAction;

    /// <summary>
    /// The action representing the initiation of a notes load.
    /// </summary>
    public record LoadNotesAction;

    /// <summary>
    /// The action representing the initiation of a ratings load.
    /// </summary>
    public record LoadRatingsAction;

    /// <summary>
    /// The action representing the initiation of an inactive users load.
    /// </summary>
    public record LoadInactiveUsersAction
    {
        /// <summary>
        /// Gets the minimum number of days since the user's last login that would qualify the user as inactive.
        /// </summary>
        public required int InactiveDays { get; init; }

        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing the initiation of a user feedback load.
    /// </summary>
    public record LoadUserFeedbackAction;

    /// <summary>
    /// The action representing the initiation of a year of birth counts load.
    /// </summary>
    public record LoadYearOfBirthCountsAction
    {
        /// <summary>
        /// Gets the start date in local time.
        /// </summary>
        public required DateOnly StartDateLocal { get; init; }

        /// <summary>
        /// Gets the end date in local time.
        /// </summary>
        public required DateOnly EndDateLocal { get; init; }

        /// <summary>
        /// Gets the offset from the client browser to UTC.
        /// </summary>
        public required int TimeOffset { get; init; }
    }

    /// <summary>
    /// The action representing a successful load action.
    /// </summary>
    public record LoadSuccessAction : BaseSuccessAction<HttpContent>;

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public record LoadFailureAction : BaseFailureAction;

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public record ResetStateAction;
}
