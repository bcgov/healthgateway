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
    private DateRange? demographicsDateRange = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);
    private DateRange? usageDateRange = new(DateTime.Now.AddDays(-6).Date, DateTime.Now.Date);

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

    private MudDateRangePicker DemographicsDateRangePicker { get; set; } = default!;

    private MudDateRangePicker UsageDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1, 0, 0, 0, DateTimeKind.Local);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private int CurrentUniqueDays { get; set; } = 3;

    private string RegisteredUsersErrorMessage => this.DashboardState.Value.GetDailyUserRegistrationCounts.Error?.Message ?? string.Empty;

    private string LoggedInUsersErrorMessage => this.DashboardState.Value.GetDailyUniqueLoginCounts.Error?.Message ?? string.Empty;

    private string DependentsErrorMessage => this.DashboardState.Value.GetDailyDependentRegistrationCounts.Error?.Message ?? string.Empty;

    private string RecurringUsersErrorMessage => this.DashboardState.Value.GetRecurringUserCount.Error?.Message ?? string.Empty;

    private string AppLoginCountsErrorMessage => this.DashboardState.Value.GetAppLoginCounts.Error?.Message ?? string.Empty;

    private string RatingSummaryErrorMessage => this.DashboardState.Value.GetRatingsSummary.Error?.Message ?? string.Empty;

    private string YearOfBirthCountsErrorMessage => this.DashboardState.Value.GetYearOfBirthCounts.Error?.Message ?? string.Empty;

    private IEnumerable<string> ErrorMessages => StringManipulator.ExcludeBlanks(
    [
        this.RegisteredUsersErrorMessage,
        this.DependentsErrorMessage,
        this.LoggedInUsersErrorMessage,
        this.RecurringUsersErrorMessage,
        this.AppLoginCountsErrorMessage,
        this.RatingSummaryErrorMessage,
        this.YearOfBirthCountsErrorMessage,
    ]);

    private DateRange? DemographicsDateRange
    {
        get => this.demographicsDateRange;

        set
        {
            this.demographicsDateRange = value;
            this.RetrieveDemographicsData();
        }
    }

    private DateRange? UsageDateRange
    {
        get => this.usageDateRange;

        set
        {
            this.usageDateRange = value;
            this.RetrieveUsageData();
        }
    }

    private DateTime DemographicsDateRangeStart => this.DemographicsDateRange?.Start ?? this.MinimumDateTime;

    private DateTime DemographicsDateRangeEnd => this.DemographicsDateRange?.End ?? this.MaximumDateTime;

    private DateTime UsageDateRangeStart => this.DemographicsDateRange?.Start ?? this.MinimumDateTime;

    private DateTime UsageDateRangeEnd => this.DemographicsDateRange?.End ?? this.MaximumDateTime;

    private int UniqueDays
    {
        get => this.CurrentUniqueDays;

        set
        {
            this.CurrentUniqueDays = value;
            this.Dispatcher.Dispatch(
                new DashboardActions.GetRecurringUserCountAction
                {
                    Days = value,
                    StartDateLocal = DateOnly.FromDateTime(this.DemographicsDateRangeStart),
                    EndDateLocal = DateOnly.FromDateTime(this.DemographicsDateRangeEnd),
                    TimeOffset = this.TimeOffset,
                });
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
                .Where(r => this.UsageDateRangeStart <= r.Date.ToDateTime(TimeOnly.MaxValue) && r.Date.ToDateTime(TimeOnly.MinValue) <= this.UsageDateRangeEnd)
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
        this.RetrieveDemographicsData();
        this.RetrieveUsageData();
    }

    private void RetrieveDemographicsData()
    {
        DateOnly startDate = DateOnly.FromDateTime(this.DemographicsDateRangeStart);
        DateOnly endDate = DateOnly.FromDateTime(this.DemographicsDateRangeEnd);
        this.Dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = this.TimeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetRatingsSummaryAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = this.TimeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetYearOfBirthCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = this.TimeOffset });
    }

    private void RetrieveUsageData()
    {
        DateOnly startDate = DateOnly.FromDateTime(this.UsageDateRangeStart);
        DateOnly endDate = DateOnly.FromDateTime(this.UsageDateRangeEnd);
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyUserRegistrationCountsAction { TimeOffset = this.TimeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyDependentRegistrationCountsAction { TimeOffset = this.TimeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyUniqueLoginCountsAction { StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = this.TimeOffset });
        this.Dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountAction { Days = this.UniqueDays, StartDateLocal = startDate, EndDateLocal = endDate, TimeOffset = this.TimeOffset });
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
