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
    private static DateOnly StartDate => DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private static DateOnly EndDate => DateOnly.FromDateTime(DateTime.Now);

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<DashboardState> DashboardState { get; set; } = default!;

    private IDictionary<DateTime, int> RegisteredUsers => this.DashboardState.Value.GetRegisteredUsers.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private IDictionary<DateTime, int> LoggedInUsers => this.DashboardState.Value.GetLoggedInUsers.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private IDictionary<DateTime, int> Dependents => this.DashboardState.Value.GetDependents.Result ?? ImmutableDictionary<DateTime, int>.Empty;

    private IDictionary<string, int> UserCounts => this.DashboardState.Value.GetUserCounts.Result ?? ImmutableDictionary<string, int>.Empty;

    private IDictionary<string, int> YearOfBirthCounts => this.DashboardState.Value.YearOfBirthCounts;

    private int RecurringUserCount => this.UserCounts.TryGetValue("RecurringUserCount", out int recurringUserCount) ? recurringUserCount : 0;

    private int MobileUserCount => this.UserCounts.TryGetValue(UserLoginClientType.Mobile.ToString(), out int mobileCount) ? mobileCount : 0;

    private int WebUserCount => this.UserCounts.TryGetValue(UserLoginClientType.Web.ToString(), out int webCount) ? webCount : 0;

    private bool RegisteredUsersLoading => this.DashboardState.Value.GetRegisteredUsers.IsLoading;

    private bool LoggedInUsersLoading => this.DashboardState.Value.GetLoggedInUsers.IsLoading;

    private bool DependentsLoading => this.DashboardState.Value.GetDependents.IsLoading;

    private bool UserCountsLoading => this.DashboardState.Value.GetUserCounts.IsLoading;

    private bool RatingSummaryLoading => this.DashboardState.Value.GetRatingSummary.IsLoading;

    private bool YearOfBirthCountsLoading => this.DashboardState.Value.GetYearOfBirthCounts.IsLoading;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1, 0, 0, 0, DateTimeKind.Local);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int CurrentUniqueDays { get; set; } = 3;

    private string RegisteredUsersErrorMessage => this.DashboardState.Value.GetRegisteredUsers.Error?.Message ?? string.Empty;

    private string LoggedInUsersErrorMessage => this.DashboardState.Value.GetLoggedInUsers.Error?.Message ?? string.Empty;

    private string DependentsErrorMessage => this.DashboardState.Value.GetDependents.Error?.Message ?? string.Empty;

    private string RecurringUsersErrorMessage => this.DashboardState.Value.GetUserCounts.Error?.Message ?? string.Empty;

    private string RatingSummaryErrorMessage => this.DashboardState.Value.GetRatingSummary.Error?.Message ?? string.Empty;

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
                DateOnly.FromDateTime(value.Start!.Value),
                DateOnly.FromDateTime(value.End!.Value),
                this.TimeOffset);
            this.DateRange = value;
        }
    }

    private int UniqueDays
    {
        get => this.CurrentUniqueDays;

        set
        {
            this.Dispatcher.Dispatch(
                new DashboardActions.GetUserCountsAction
                {
                    Days = value,
                    StartDateLocal = DateOnly.FromDateTime(this.SelectedDateRange.Start!.Value),
                    EndDateLocal = DateOnly.FromDateTime(this.SelectedDateRange.End!.Value),
                    TimeOffset = this.TimeOffset,
                });
            this.CurrentUniqueDays = value;
        }
    }

    private int TimeOffset { get; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

    private int TotalRegisteredUsers => this.RegisteredUsers.Sum(r => r.Value);

    private int TotalDependents => this.Dependents.Sum(r => r.Value);

    private IEnumerable<DailyDataRow> TableData
    {
        get
        {
            return this.RegisteredUsers.Select(kvp => new DailyDataRow { DailyDateTime = kvp.Key, TotalRegisteredUsers = kvp.Value })
                .Concat(this.LoggedInUsers.Select(kvp => new DailyDataRow { DailyDateTime = kvp.Key, TotalLoggedInUsers = kvp.Value }))
                .Concat(this.Dependents.Select(kvp => new DailyDataRow { DailyDateTime = kvp.Key, TotalDependents = kvp.Value }))
                .Where(r => this.SelectedDateRange.Start <= r.DailyDateTime && r.DailyDateTime <= this.SelectedDateRange.End)
                .GroupBy(r => r.DailyDateTime)
                .Select(
                    group => new DailyDataRow
                    {
                        DailyDateTime = group.Key,
                        TotalRegisteredUsers = group.Sum(r => r.TotalRegisteredUsers),
                        TotalDependents = group.Sum(r => r.TotalDependents),
                        TotalLoggedInUsers = group.Sum(r => r.TotalLoggedInUsers),
                    });
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
        this.RetrieveData(this.UniqueDays, StartDate, EndDate, this.TimeOffset);
    }

    private void RetrieveData(int days, DateOnly startDate, DateOnly endDate, int timeOffset)
    {
        this.Dispatcher.Dispatch(new DashboardActions.GetRegisteredUsersAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetLoggedInUsersAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetDependentsAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetUserCountsAction { Days = days, StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetRatingSummaryAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetYearOfBirthCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
    }

    private void RefreshData()
    {
        this.RetrieveData(
            this.UniqueDays,
            DateOnly.FromDateTime(this.SelectedDateRange.Start!.Value),
            DateOnly.FromDateTime(this.SelectedDateRange.End!.Value),
            this.TimeOffset);
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
