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
    /// The action representing the initiation of user profiles action load.
    /// </summary>
    public class LoadUserProfilesAction : AnalyticsBaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadUserProfilesAction"/> class.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        public LoadUserProfilesAction(DateTime? startDate, DateTime? endDate)
            : base(startDate, endDate)
        {
        }
    }

    /// <summary>
    /// The action representing a successful user profiles action load.
    /// </summary>
    public class LoadSuccessUserProfilesAction : BaseLoadSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessUserProfilesAction"/> class.
        /// </summary>
        /// <param name="state">user profiles state.</param>
        public LoadSuccessUserProfilesAction(HttpContent state)
            : base(state)
        {
        }
    }

    /// <summary>
    /// The action representing a failed user profiles action load.
    /// </summary>
    public class LoadFailUserProfileAction : BaseLoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailUserProfileAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailUserProfileAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the user profiles state.
    /// </summary>
    public class ResetUserProfilesStateAction
    {
    }

    /// <summary>
    /// The action representing the initiation of comments action load.
    /// </summary>
    public class LoadCommentsAction : AnalyticsBaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadCommentsAction"/> class.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        public LoadCommentsAction(DateTime? startDate, DateTime? endDate)
            : base(startDate, endDate)
        {
        }
    }

    /// <summary>
    /// The action representing a successful comments action load.
    /// </summary>
    public class LoadSuccessCommentsAction : BaseLoadSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessCommentsAction"/> class.
        /// </summary>
        /// <param name="state">comments state.</param>
        public LoadSuccessCommentsAction(HttpContent state)
            : base(state)
        {
        }
    }

    /// <summary>
    /// The action representing a failed comments action load.
    /// </summary>
    public class LoadFailCommentsAction : BaseLoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailCommentsAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailCommentsAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the comments state.
    /// </summary>
    public class ResetCommentsStateAction
    {
    }

    /// <summary>
    /// The action representing the initiation of notes action load.
    /// </summary>
    public class LoadNotesAction : AnalyticsBaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadNotesAction"/> class.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        public LoadNotesAction(DateTime? startDate, DateTime? endDate)
            : base(startDate, endDate)
        {
        }
    }

    /// <summary>
    /// The action representing a successful notes action load.
    /// </summary>
    public class LoadSuccessNotesAction : BaseLoadSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessNotesAction"/> class.
        /// </summary>
        /// <param name="state">notes state.</param>
        public LoadSuccessNotesAction(HttpContent state)
            : base(state)
        {
        }
    }

    /// <summary>
    /// The action representing a failed notes action load.
    /// </summary>
    public class LoadFailNotesAction : BaseLoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailNotesAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailNotesAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the notes state.
    /// </summary>
    public class ResetNotesStateAction
    {
    }

    /// <summary>
    /// The action representing the initiation of ratings load.
    /// </summary>
    public class LoadRatingsAction : AnalyticsBaseAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadRatingsAction"/> class.
        /// </summary>
        /// <param name="startDate">Optional start date to include in the query.</param>
        /// <param name="endDate">Optional end date to include in the query.</param>
        public LoadRatingsAction(DateTime? startDate, DateTime? endDate)
            : base(startDate, endDate)
        {
        }
    }

    /// <summary>
    /// The action representing a successful rating action load.
    /// </summary>
    public class LoadSuccessRatingsAction : BaseLoadSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessRatingsAction"/> class.
        /// </summary>
        /// <param name="state">ratings state.</param>
        public LoadSuccessRatingsAction(HttpContent state)
            : base(state)
        {
        }
    }

    /// <summary>
    /// The action representing a failed ratings action load.
    /// </summary>
    public class LoadFailRatingsAction : BaseLoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailRatingsAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailRatingsAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the ratings state.
    /// </summary>
    public class ResetRatingsStateAction
    {
    }

    /// <summary>
    /// The action representing the initiation of inactive users load.
    /// </summary>
    public class LoadInactiveUsersAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadInactiveUsersAction"/> class.
        /// </summary>
        /// <param name="inactiveDays">The days inactive to filter the users last login.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadInactiveUsersAction(int inactiveDays, int timeOffset)
        {
            this.InactiveDays = inactiveDays;
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets inactive days.
        /// </summary>
        public int InactiveDays { get; set; }

        /// <summary>
        /// Gets or sets time offset.
        /// </summary>
        public int TimeOffset { get; set; }
    }

    /// <summary>
    /// The action representing a successful inactive users action load.
    /// </summary>
    public class LoadSuccessInactiveUsersAction : BaseLoadSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessInactiveUsersAction"/> class.
        /// </summary>
        /// <param name="state">inactive users state.</param>
        public LoadSuccessInactiveUsersAction(HttpContent state)
            : base(state)
        {
        }
    }

    /// <summary>
    /// The action representing a failed inactive users action load.
    /// </summary>
    public class LoadFailInactiveUsersAction : BaseLoadFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailInactiveUsersAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailInactiveUsersAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the inactive users state.
    /// </summary>
    public class ResetInactiveUsersStateAction
    {
    }
}
