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
using HealthGateway.Admin.Common.Models;
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

    private IDictionary<DateOnly, int> UserRegistrationCounts => this.DashboardState.Value.GetDailyUserRegistrationCounts.Result ?? ImmutableDictionary<DateOnly, int>.Empty;

    private IDictionary<DateOnly, int> DailyDependentRegistrationCounts => this.DashboardState.Value.GetDailyDependentRegistrationCounts.Result ?? ImmutableDictionary<DateOnly, int>.Empty;

    private IDictionary<DateOnly, int> DailyUniqueLoginCounts => this.DashboardState.Value.GetDailyUniqueLoginCounts.Result ?? ImmutableDictionary<DateOnly, int>.Empty;

    private int RecurringUserCount => this.DashboardState.Value.GetRecurringUserCount.Result ?? 0;

    private AppLoginCounts AppLoginCounts => this.DashboardState.Value.GetAppLoginCounts.Result ?? new(0, 0);

    private IDictionary<string, int> YearOfBirthCounts => this.DashboardState.Value.YearOfBirthCounts;

    private bool DailyUserRegistrationCountsLoading => this.DashboardState.Value.GetDailyUserRegistrationCounts.IsLoading;

    private bool DailyDependentRegistrationCountsLoading => this.DashboardState.Value.GetDailyDependentRegistrationCounts.IsLoading;

    private bool DailyUniqueLoginCountsLoading => this.DashboardState.Value.GetDailyUniqueLoginCounts.IsLoading;

    private bool RecurringUserCountLoading => this.DashboardState.Value.GetRecurringUserCount.IsLoading;

    private bool AppLoginCountsLoading => this.DashboardState.Value.GetAppLoginCounts.IsLoading;

    private bool RatingsSummaryLoading => this.DashboardState.Value.GetRatingsSummary.IsLoading;

    private bool YearOfBirthCountsLoading => this.DashboardState.Value.GetYearOfBirthCounts.IsLoading;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1, 0, 0, 0, DateTimeKind.Local);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int CurrentUniqueDays { get; set; } = 3;

    private string RegisteredUsersErrorMessage => this.DashboardState.Value.GetDailyUserRegistrationCounts.Error?.Message ?? string.Empty;

    private string LoggedInUsersErrorMessage => this.DashboardState.Value.GetDailyUniqueLoginCounts.Error?.Message ?? string.Empty;

    private string DependentsErrorMessage => this.DashboardState.Value.GetDailyDependentRegistrationCounts.Error?.Message ?? string.Empty;

    private string RecurringUsersErrorMessage => this.DashboardState.Value.GetRecurringUserCount.Error?.Message ?? string.Empty;

    private string AppLoginCountsErrorMessage => this.DashboardState.Value.GetAppLoginCounts.Error?.Message ?? string.Empty;

    private string RatingSummaryErrorMessage => this.DashboardState.Value.GetRatingsSummary.Error?.Message ?? string.Empty;

    private string YearOfBirthCountsErrorMessage => this.DashboardState.Value.GetYearOfBirthCounts.Error?.Message ?? string.Empty;

    private IEnumerable<string> ErrorMessages => StringManipulator.ExcludeBlanks(
        new[]
        {
            this.RegisteredUsersErrorMessage,
            this.DependentsErrorMessage,
            this.LoggedInUsersErrorMessage,
            this.RecurringUsersErrorMessage,
            this.AppLoginCountsErrorMessage,
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
                new DashboardActions.GetRecurringUserCountAction
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

    private int TotalRegisteredUsers => this.UserRegistrationCounts.Sum(r => r.Value);

    private int TotalDependents => this.DailyDependentRegistrationCounts.Sum(r => r.Value);

    private IEnumerable<DailyDataRow> TableData
    {
        get
        {
            return this.UserRegistrationCounts.Select(kvp => new DailyDataRow { Date = kvp.Key, UserRegistrations = kvp.Value })
                .Concat(this.DailyUniqueLoginCounts.Select(kvp => new DailyDataRow { Date = kvp.Key, UniqueLogins = kvp.Value }))
                .Concat(this.DailyDependentRegistrationCounts.Select(kvp => new DailyDataRow { Date = kvp.Key, DependentRegistrations = kvp.Value }))
                .Where(r => this.SelectedDateRange.Start <= r.Date.ToDateTime(TimeOnly.MaxValue) && r.Date.ToDateTime(TimeOnly.MinValue) <= this.SelectedDateRange.End)
                .GroupBy(r => r.Date)
                .Select(
                    group => new DailyDataRow
                    {
                        Date = group.Key,
                        UserRegistrations = group.Sum(r => r.UserRegistrations),
                        DependentRegistrations = group.Sum(r => r.DependentRegistrations),
                        UniqueLogins = group.Sum(r => r.UniqueLogins),
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
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyUserRegistrationCountsAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyDependentRegistrationCountsAction { TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyUniqueLoginCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountAction { Days = days, StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetRatingsSummaryAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = timeOffset });
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
        /// Gets the date.
        /// </summary>
        public DateOnly Date { get; init; }

        /// <summary>
        /// Gets the user registration count.
        /// </summary>
        public int UserRegistrations { get; init; }

        /// <summary>
        /// Gets the unique login count.
        /// </summary>
        public int UniqueLogins { get; init; }

        /// <summary>
        /// Gets the dependent registration count.
        /// </summary>
        public int DependentRegistrations { get; init; }
    }
}
