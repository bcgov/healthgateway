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
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Utils;
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

    private IDictionary<string, int> UserCountsResult => this.DashboardState.Value.UserCounts.Result ?? ImmutableDictionary<string, int>.Empty;

    private IDictionary<string, int> YearOfBirthCounts => this.DashboardState.Value.YearOfBirthCounts;

    private int RecurringUserCount => this.UserCountsResult.TryGetValue("RecurringUserCount", out int recurringUserCount) ? recurringUserCount : 0;

    private int MobileUserCount => this.UserCountsResult.TryGetValue(UserLoginClientType.Mobile.ToString(), out int mobileCount) ? mobileCount : 0;

    private int WebUserCount => this.UserCountsResult.TryGetValue(UserLoginClientType.Web.ToString(), out int webCount) ? webCount : 0;

    private bool RegisteredUsersLoading => this.DashboardState.Value.RegisteredUsers.IsLoading;

    private bool LoggedInUsersLoading => this.DashboardState.Value.LoggedInUsers.IsLoading;

    private bool DependentsLoading => this.DashboardState.Value.Dependents.IsLoading;

    private bool RecurringUsersLoading => this.DashboardState.Value.UserCounts.IsLoading;

    private bool RatingSummaryLoading => this.DashboardState.Value.RatingSummary.IsLoading;

    private bool YearOfBirthCountsLoading => this.DashboardState.Value.GetYearOfBirthCounts.IsLoading;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1, 0, 0, 0, DateTimeKind.Local);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int CurrentUniqueDays { get; set; } = 3;

    private string RegisteredUsersErrorMessage => this.DashboardState.Value.RegisteredUsers.Error?.Message ?? string.Empty;

    private string LoggedInUsersErrorMessage => this.DashboardState.Value.LoggedInUsers.Error?.Message ?? string.Empty;

    private string DependentsErrorMessage => this.DashboardState.Value.Dependents.Error?.Message ?? string.Empty;

    private string RecurringUsersErrorMessage => this.DashboardState.Value.UserCounts.Error?.Message ?? string.Empty;

    private string RatingSummaryErrorMessage => this.DashboardState.Value.RatingSummary.Error?.Message ?? string.Empty;

    private string YearOfBirthCountsErrorMessage => this.DashboardState.Value.GetYearOfBirthCounts.Error?.Message ?? string.Empty;

    private IEnumerable<string> ErrorMessages => StringManipulator.ExcludeBlanks(
        new[]
        {
            this.RegisteredUsersErrorMessage,
            this.DependentsErrorMessage,
            this.LoggedInUsersErrorMessage,
            this.RecurringUsersErrorMessage,
            this.RatingSummaryErrorMessage,
            this.YearOfBirthCountsErrorMessage,
        });

    private DateRange SelectedDateRange
    {
        get => this.DateRange;

        set
        {
            this.RetrieveData(
                this.UniqueDays,
                value.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                value.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                this.TimeOffset,
                false);
            this.DateRange = value;
        }
    }

    private int UniqueDays
    {
        get => this.CurrentUniqueDays;

        set
        {
            this.Dispatcher.Dispatch(
                new DashboardActions.LoadRecurringUsersAction
                {
                    Days = value,
                    StartPeriod = this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                    EndPeriod = this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                    TimeOffset = this.TimeOffset,
                });
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
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
        this.RetrieveData(this.UniqueDays, StartDate, EndDate, this.TimeOffset, true);
    }

    private void RetrieveData(int days, string startPeriod, string endPeriod, int timeOffset, bool initialLoad)
    {
        string endDate = initialLoad ? DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : endPeriod;
        this.Dispatcher.Dispatch(new DashboardActions.LoadRegisteredUsersAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.LoadLoggedInUsersAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.LoadDependentsAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.LoadRecurringUsersAction { Days = days, StartPeriod = startPeriod, EndPeriod = endPeriod, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.LoadRatingSummaryAction { StartPeriod = startPeriod, EndPeriod = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetYearOfBirthCountsAction { StartPeriod = startPeriod, EndPeriod = endPeriod, TimeOffset = timeOffset });
    }

    private void RefreshData()
    {
        this.RetrieveData(
            this.UniqueDays,
            this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            this.TimeOffset,
            false);
    }

    private ChartSeries GetYearOfBirthCountSeries()
    {
        return new ChartSeries
        {
            Name = "Count of Unique Users",
            Data = this.YearOfBirthCounts.Select(kvp => (double)kvp.Value).ToArray(),
        };
    }

    private string[] GetYearOfBirthLabels()
    {
        return this.YearOfBirthCounts.Select((kvp, i) => i % 10 == 0 ? kvp.Key : string.Empty).ToArray();
    }

    private sealed record DailyDataRow
    {
        /// <summary>
        /// Gets the dashboard daily datetime.
        /// </summary>
        public DateTime DailyDateTime { get; init; }

        /// <summary>
        /// Gets the total registered users.
        /// </summary>
        public int TotalRegisteredUsers { get; init; }

        /// <summary>
        /// Gets the total logged in users.
        /// </summary>
        public int TotalLoggedInUsers { get; init; }

        /// <summary>
        /// Gets the total dependents.
        /// </summary>
        public int TotalDependents { get; init; }
    }
}
