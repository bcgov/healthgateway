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
namespace HealthGateway.Admin.Client.Pages;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Dashboard;
using Microsoft.AspNetCore.Components;
using MudBlazor;

/// <summary>
/// Backing logic for the Dashboard page.
/// </summary>
public partial class DashboardPage : FluxorComponent
{
    private static string StartDate => DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string EndDate => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<DashboardState> DashboardState { get; set; } = default!;

    private IDictionary<DateTime, int> RegisteredUsersResult => this.DashboardState.Value.RegisteredUsers.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private IDictionary<DateTime, int> LoggedInUsersResult => this.DashboardState.Value.LoggedInUsers.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private IDictionary<DateTime, int> DependentsResult => this.DashboardState.Value.Dependents.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private int TotalRecurringUsers => this.DashboardState.Value.RecurringUsers.Result?.TotalRecurringUsers ?? 0;

    private bool RegisteredUsersLoading => this.DashboardState.Value.RegisteredUsers.IsLoading;

    private bool LoggedInUsersLoading => this.DashboardState.Value.LoggedInUsers.IsLoading;

    private bool DependentsLoading => this.DashboardState.Value.Dependents.IsLoading;

    private bool RecurringUsersLoading => this.DashboardState.Value.RecurringUsers.IsLoading;

    private bool RatingSummaryLoading => this.DashboardState.Value.RatingSummary.IsLoading;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int CurrentUniqueDays { get; set; } = 3;

    private bool RegisteredUsersHasError => this.DashboardState.Value.RegisteredUsers.Error != null && this.DashboardState.Value.RegisteredUsers.Error.Message.Length > 0;

    private string RegisteredUsersErrorMessage => this.DashboardState.Value.RegisteredUsers.Error?.Message ?? string.Empty;

    private bool LoggedInUsersHasError => this.DashboardState.Value.LoggedInUsers.Error != null && this.DashboardState.Value.LoggedInUsers.Error.Message.Length > 0;

    private string LoggedInUsersErrorMessage => this.DashboardState.Value.LoggedInUsers.Error?.Message ?? string.Empty;

    private bool DependentsHasError => this.DashboardState.Value.Dependents.Error != null && this.DashboardState.Value.Dependents.Error.Message.Length > 0;

    private string DependentsErrorMessage => this.DashboardState.Value.Dependents.Error?.Message ?? string.Empty;

    private bool RecurringUsersHasError => this.DashboardState.Value.RecurringUsers.Error != null && this.DashboardState.Value.RecurringUsers.Error.Message.Length > 0;

    private string RecurringUsersErrorMessage => this.DashboardState.Value.RecurringUsers.Error?.Message ?? string.Empty;

    private bool RatingSummaryHasError => this.DashboardState.Value.RatingSummary.Error != null && this.DashboardState.Value.RatingSummary.Error.Message.Length > 0;

    private string RatingSummaryErrorMessage => this.DashboardState.Value.RatingSummary.Error?.Message ?? string.Empty;

    private DateRange SelectedDateRange
    {
        get => this.DateRange;

        set
        {
            this.LoadDispatchActions(
                this.UniqueDays,
                value.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                value.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                this.TimeOffset,
                false);
            this.DateRange = value;
        }
    }

    private bool HasError
    {
        get
        {
            bool test = this.RegisteredUsersHasError ||
                        this.LoggedInUsersHasError ||
                        this.DependentsHasError ||
                        this.RecurringUsersHasError ||
                        this.RatingSummaryHasError;
            return test;
        }
    }

    private List<string> ErrorList
    {
        get
        {
            List<string> errors = new();

            if (this.RegisteredUsersHasError)
            {
                errors.Add(this.RegisteredUsersErrorMessage);
            }

            if (this.LoggedInUsersHasError)
            {
                errors.Add(this.LoggedInUsersErrorMessage);
            }

            if (this.DependentsHasError)
            {
                errors.Add(this.DependentsErrorMessage);
            }

            if (this.RecurringUsersHasError)
            {
                errors.Add(this.RecurringUsersErrorMessage);
            }

            if (this.RatingSummaryHasError)
            {
                errors.Add(this.RatingSummaryErrorMessage);
            }

            return errors;
        }
    }

    private int UniqueDays
    {
        get => this.CurrentUniqueDays;

        set
        {
            this.Dispatcher.Dispatch(
                new DashboardActions.LoadRecurringUsersAction(
                    value,
                    this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    this.TimeOffset));
            this.CurrentUniqueDays = value;
        }
    }

    private int TimeOffset { get; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

    private int TotalRegisteredUsers => this.RegisteredUsersResult.Sum(r => r.Value);

    private int TotalDependents => this.DependentsResult.Sum(r => r.Value);

    private IEnumerable<DailyDataRow> TableData
    {
        get
        {
            DateTime? startDate = this.SelectedDateRange.Start;
            DateTime? endDate = this.SelectedDateRange.End;

            List<DailyDataRow> results = new();

            foreach (KeyValuePair<DateTime, int> user in this.RegisteredUsersResult)
            {
                DailyDataRow dashboardDailyData = new()
                {
                    DailyDateTime = user.Key,
                    TotalRegisteredUsers = user.Value,
                };

                results.Add(dashboardDailyData);
            }

            foreach (KeyValuePair<DateTime, int> loggedInUser in this.LoggedInUsersResult)
            {
                DailyDataRow dashboardDailyData = new()
                {
                    DailyDateTime = loggedInUser.Key,
                    TotalLoggedInUsers = loggedInUser.Value,
                };

                results.Add(dashboardDailyData);
            }

            foreach (KeyValuePair<DateTime, int> dependent in this.DependentsResult)
            {
                DailyDataRow dashboardDailyData = new()
                {
                    DailyDateTime = dependent.Key,
                    TotalDependents = dependent.Value,
                };

                results.Add(dashboardDailyData);
            }

            return results
                .Where(r => startDate <= r.DailyDateTime && r.DailyDateTime <= endDate)
                .GroupBy(r => r.DailyDateTime)
                .Select(
                    grp => new DailyDataRow
                    {
                        DailyDateTime = grp.First().DailyDateTime,
                        TotalRegisteredUsers = grp.Sum(s => s.TotalRegisteredUsers),
                        TotalDependents = grp.Sum(d => d.TotalDependents),
                        TotalLoggedInUsers = grp.Sum(l => l.TotalLoggedInUsers),
                    });
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetDashboardState();
        this.LoadDispatchActions(this.UniqueDays, StartDate, EndDate, this.TimeOffset, true);
    }

    private void LoadDispatchActions(int days, string startPeriod, string endPeriod, int timeOffset, bool initialLoad)
    {
        string endDate = initialLoad ? DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : endPeriod;
        this.Dispatcher.Dispatch(new DashboardActions.LoadRegisteredUsersAction(timeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.LoadLoggedInUsersAction(timeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.LoadDependentsAction(timeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.LoadRecurringUsersAction(days, startPeriod, endPeriod, timeOffset));
        this.DispatchRatingSummaryAction(startPeriod, endDate, timeOffset);
    }

    private void ReloadDispatchActions()
    {
        this.LoadDispatchActions(
            this.UniqueDays,
            this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            this.TimeOffset,
            false);
    }

    private void DispatchRatingSummaryAction(string startPeriod, string endPeriod, int timeOffset)
    {
        this.Dispatcher.Dispatch(new DashboardActions.LoadRatingSummaryAction(startPeriod, endPeriod, timeOffset));
    }

    private void ResetDashboardState()
    {
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
    }

    private sealed record DailyDataRow
    {
        /// <summary>
        /// Gets the dashboard daily datetime.
        /// </summary>
        public DateTime DailyDateTime { get; init; }

        /// <summary>
        /// Gets or sets the total registered users.
        /// </summary>
        public int TotalRegisteredUsers { get; set; }

        /// <summary>
        /// Gets or sets the total logged in users.
        /// </summary>
        public int TotalLoggedInUsers { get; set; }

        /// <summary>
        /// Gets or sets the total dependents.
        /// </summary>
        public int TotalDependents { get; set; }
    }
}
