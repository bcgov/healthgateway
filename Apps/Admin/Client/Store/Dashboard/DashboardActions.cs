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
    /// The action representing the initiation of registered users load action.
    /// </summary>
    public class LoadRegisteredUsersAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadRegisteredUsersAction"/> class.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadRegisteredUsersAction(int timeOffset)
        {
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets time offset.
        /// </summary>
        public int TimeOffset { get; set; }
    }

    /// <summary>
    /// The action representing the initiation of logged in users load action.
    /// </summary>
    public class LoadLoggedInUsersAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadLoggedInUsersAction"/> class.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadLoggedInUsersAction(int timeOffset)
        {
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets time offset.
        /// </summary>
        public int TimeOffset { get; set; }
    }

    /// <summary>
    /// The action representing the initiation of dependents load action.
    /// </summary>
    public class LoadDependentsAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadDependentsAction"/> class.
        /// </summary>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadDependentsAction(int timeOffset)
        {
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets time offset.
        /// </summary>
        public int TimeOffset { get; set; }
    }

    /// <summary>
    /// The action representing the initiation of recurring users load action.
    /// </summary>
    public class LoadRecurringUsersAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadRecurringUsersAction"/> class.
        /// </summary>
        /// <param name="days">The number of unique days for evaluating a user.</param>
        /// <param name="startPeriod">The period start over which to evaluate the user.</param>
        /// <param name="endPeriod">The period end over which to evaluate the user.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadRecurringUsersAction(int days, string startPeriod, string endPeriod, int timeOffset)
        {
            this.Days = days;
            this.StartPeriod = startPeriod;
            this.EndPeriod = endPeriod;
            this.TimeOffset = timeOffset;
        }

        /// <summary>
        /// Gets or sets days.
        /// </summary>
        public int Days { get; set; }

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
    /// The action representing the initiation of rating summary load action.
    /// </summary>
    public class LoadRatingSummaryAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadRatingSummaryAction"/> class.
        /// </summary>
        /// <param name="startPeriod">The period start over which to evaluate the user.</param>
        /// <param name="endPeriod">The period end over which to evaluate the user.</param>
        /// <param name="timeOffset">The offset from the client browser to UTC.</param>
        public LoadRatingSummaryAction(string startPeriod, string endPeriod, int timeOffset)
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
    public class LoadSuccessUserAction : BaseSuccessAction<IDictionary<DateTime, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessUserAction"/> class.
        /// </summary>
        /// <param name="data">user data.</param>
        public LoadSuccessUserAction(IDictionary<DateTime, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public class LoadFailUserAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailUserAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailUserAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the user state.
    /// </summary>
    public class ResetUserStateAction
    {
    }

    /// <summary>
    /// The action representing a successful load action.
    /// </summary>
    public class LoadSuccessRecurringUserAction : BaseSuccessAction<RecurringUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessRecurringUserAction"/> class.
        /// </summary>
        /// <param name="data">recurring user data.</param>
        public LoadSuccessRecurringUserAction(RecurringUser data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public class LoadFailRecurringUserAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailRecurringUserAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailRecurringUserAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the recurring user state.
    /// </summary>
    public class ResetRecurringUserStateAction
    {
    }

    /// <summary>
    /// The action representing a successful load action.
    /// </summary>
    public class LoadSuccessRatingSummaryAction : BaseSuccessAction<IDictionary<string, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSuccessRatingSummaryAction"/> class.
        /// </summary>
        /// <param name="data">rating summary data.</param>
        public LoadSuccessRatingSummaryAction(IDictionary<string, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public class LoadFailRatingSummaryAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadFailRatingSummaryAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoadFailRatingSummaryAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the rating summary state.
    /// </summary>
    public class ResetRatingSummaryStateAction
    {
    }

    /// <summary>
    /// The action that clears the all dashboard current state.
    /// </summary>
    public class ResetDashboardStateAction
    {
    }
}
