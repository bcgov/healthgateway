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
    /// The action representing the initiation of user profiles load action.
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
    /// The action representing the initiation of comments load action.
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
    /// The action representing the initiation of notes load action.
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
    /// The action representing the initiation of ratings load action.
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
    /// The action representing the initiation of inactive users load action.
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
    /// The action representing the initiation of the user feedback load action.
    /// </summary>
    public class LoadUserFeedbackAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadUserFeedbackAction"/> class.
        /// </summary>
        public LoadUserFeedbackAction()
        {
        }
    }

    /// <summary>
    /// The action representing the initiation of year of birth counts load action.
    /// </summary>
    public class LoadYearOfBirthCountsAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadYearOfBirthCountsAction"/> class.
        /// </summary>
        /// <param name="startPeriod">The start period of the year of birth counts.</param>
        /// <param name="endPeriod">The end period of the year of birth counts.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadYearOfBirthCountsAction(string startPeriod, string endPeriod, int timeOffset)
        {
            this.StartPeriod = startPeriod;
            this.EndPeriod = endPeriod;
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets start period.
        /// </summary>
        public string StartPeriod { get; set; }

        /// <summary>
        /// Gets or sets end period.
        /// </summary>
        public string EndPeriod { get; set; }

        /// <summary>
        /// Gets or sets time offset.
        /// </summary>
        public int TimeOffset { get; set; }
    }

    /// <summary>
    /// The action representing a successful load action.
    /// </summary>
    public class LoadSuccessAction : BaseSuccessAction<HttpContent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessAction"/> class.
        /// </summary>
        /// <param name="data">Analytics data.</param>
        public LoadSuccessAction(HttpContent data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public class LoadFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the generic report state.
    /// </summary>
    public class ResetStateAction
    {
    }
}
