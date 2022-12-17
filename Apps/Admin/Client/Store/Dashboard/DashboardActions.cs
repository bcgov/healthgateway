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
    /// The action representing a successful registered user action.
    /// </summary>
    public class RegisteredUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredUsersSuccessAction"/> class.
        /// </summary>
        /// <param name="data">user data.</param>
        public RegisteredUsersSuccessAction(IDictionary<DateTime, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed registered user action.
    /// </summary>
    public class RegisteredUsersFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredUsersFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public RegisteredUsersFailAction(RequestError error)
            : base(error)
        {
        }
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
    /// The action representing a successful logged in user action.
    /// </summary>
    public class LoggedInUsersSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggedInUsersSuccessAction"/> class.
        /// </summary>
        /// <param name="data">user data.</param>
        public LoggedInUsersSuccessAction(IDictionary<DateTime, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed logged in user action.
    /// </summary>
    public class LoggedInUsersFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggedInUsersFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public LoggedInUsersFailAction(RequestError error)
            : base(error)
        {
        }
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
    /// The action representing a successful dependents action.
    /// </summary>
    public class DependentsSuccessAction : BaseSuccessAction<IDictionary<DateTime, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentsSuccessAction"/> class.
        /// </summary>
        /// <param name="data">user data.</param>
        public DependentsSuccessAction(IDictionary<DateTime, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed dependent action.
    /// </summary>
    public class DependentsFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentsFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public DependentsFailAction(RequestError error)
            : base(error)
        {
        }
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
    /// The action representing a successful recurring user counts action.
    /// </summary>
    public class RecurringUsersSuccessAction : BaseSuccessAction<IDictionary<string, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringUsersSuccessAction"/> class.
        /// </summary>
        /// <param name="data">user data.</param>
        public RecurringUsersSuccessAction(IDictionary<string, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed recurring users action.
    /// </summary>
    public class RecurringUsersFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringUsersFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public RecurringUsersFailAction(RequestError error)
            : base(error)
        {
        }
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
    /// The action representing a successful rating summary action.
    /// </summary>
    public class RatingSummarySuccessAction : BaseSuccessAction<IDictionary<string, int>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RatingSummarySuccessAction"/> class.
        /// </summary>
        /// <param name="data">rating summary data.</param>
        public RatingSummarySuccessAction(IDictionary<string, int> data)
            : base(data)
        {
        }
    }

    /// <summary>
    /// The action representing a failed load action.
    /// </summary>
    public class RatingSummaryFailAction : BaseFailAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RatingSummaryFailAction"/> class.
        /// </summary>
        /// <param name="error">The request error.</param>
        public RatingSummaryFailAction(RequestError error)
            : base(error)
        {
        }
    }

    /// <summary>
    /// The action that clears the state.
    /// </summary>
    public class ResetStateAction
    {
    }
}
