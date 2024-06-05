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
using System.Globalization;
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

    private AllTimeCounts AllTimeCounts => this.DashboardState.Value.GetAllTimeCounts.Result ?? new();

    private DailyUsageCounts DailyUsageCounts => this.DashboardState.Value.GetDailyUsageCounts.Result ?? new();

    private int RecurringUserCount => this.DashboardState.Value.GetRecurringUserCount.Result ?? 0;

    private AppLoginCounts AppLoginCounts => this.DashboardState.Value.GetAppLoginCounts.Result ?? new(0, 0, 0, 0, 0);

    private bool ShowAppLoginTooltip => this.AppLoginCounts.Android > 0 || this.AppLoginCounts.Ios > 0;

    private IDictionary<int, int> AgeCounts => this.DashboardState.Value.AgeCounts;

    private bool AllTimeCountsLoading => this.DashboardState.Value.GetAllTimeCounts.IsLoading;

    private bool DailyUsageCountsLoading => this.DashboardState.Value.GetDailyUsageCounts.IsLoading;

    private bool RecurringUserCountLoading => this.DashboardState.Value.GetRecurringUserCount.IsLoading;

    private bool AppLoginCountsLoading => this.DashboardState.Value.GetAppLoginCounts.IsLoading;

    private bool RatingsSummaryLoading => this.DashboardState.Value.GetRatingsSummary.IsLoading;

    private bool AgeCountsLoading => this.DashboardState.Value.GetAgeCounts.IsLoading;

    private MudDateRangePicker DemographicsDateRangePicker { get; set; } = default!;

    private MudDateRangePicker UsageDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1, 0, 0, 0, DateTimeKind.Local);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private int CurrentUniqueDays { get; set; } = 3;

    private string AgeCountsErrorMessage => this.DashboardState.Value.GetAgeCounts.Error?.Message ?? string.Empty;

    private IEnumerable<string> ErrorMessages => StringManipulator.ExcludeBlanks(
    [
        this.DashboardState.Value.GetAllTimeCounts.Error?.Message,
        this.DashboardState.Value.GetDailyUsageCounts.Error?.Message,
        this.DashboardState.Value.GetRecurringUserCount.Error?.Message,
        this.DashboardState.Value.GetAppLoginCounts.Error?.Message,
        this.DashboardState.Value.GetRatingsSummary.Error?.Message,
        this.AgeCountsErrorMessage,
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

    private DateTime UsageDateRangeStart => this.UsageDateRange?.Start ?? this.MinimumDateTime;

    private DateTime UsageDateRangeEnd => this.UsageDateRange?.End ?? this.MaximumDateTime;

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
                    StartDateLocal = DateOnly.FromDateTime(this.UsageDateRangeStart),
                    EndDateLocal = DateOnly.FromDateTime(this.UsageDateRangeEnd),
                });
        }
    }

    private IEnumerable<DailyDataRow> TableData
    {
        get
        {
            return this.DailyUsageCounts.UserRegistrations.Select(x => new DailyDataRow { Date = x.Key, UserRegistrations = x.Value })
                .Concat(this.DailyUsageCounts.UserLogins.Select(x => new DailyDataRow { Date = x.Key, UserLogins = x.Value }))
                .Concat(this.DailyUsageCounts.DependentRegistrations.Select(x => new DailyDataRow { Date = x.Key, DependentRegistrations = x.Value }))
                .GroupBy(x => x.Date)
                .Select(
                    g => new DailyDataRow
                    {
                        Date = g.Key,
                        UserRegistrations = g.Sum(x => x.UserRegistrations),
                        UserLogins = g.Sum(x => x.UserLogins),
                        DependentRegistrations = g.Sum(x => x.DependentRegistrations),
                    });
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
        this.Dispatcher.Dispatch(new DashboardActions.GetAllTimeCountsAction());
        this.RetrieveDemographicsData();
        this.RetrieveUsageData();
    }

    private static string FormatNumber(int number)
    {
        return number.ToString("N0", CultureInfo.InvariantCulture);
    }

    private void RetrieveDemographicsData()
    {
        DateOnly startDate = DateOnly.FromDateTime(this.DemographicsDateRangeStart);
        DateOnly endDate = DateOnly.FromDateTime(this.DemographicsDateRangeEnd);
        this.Dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsAction { StartDateLocal = startDate, EndDateLocal = endDate });
        this.Dispatcher.Dispatch(new DashboardActions.GetRatingsSummaryAction { StartDateLocal = startDate, EndDateLocal = endDate });
        this.Dispatcher.Dispatch(new DashboardActions.GetAgeCountsAction { StartDateLocal = startDate, EndDateLocal = endDate });
    }

    private void RetrieveUsageData()
    {
        DateOnly startDate = DateOnly.FromDateTime(this.UsageDateRangeStart);
        DateOnly endDate = DateOnly.FromDateTime(this.UsageDateRangeEnd);
        this.Dispatcher.Dispatch(new DashboardActions.GetDailyUsageCountsAction { StartDateLocal = startDate, EndDateLocal = endDate });
        this.Dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountAction { Days = this.UniqueDays, StartDateLocal = startDate, EndDateLocal = endDate });
    }

    private ChartSeries GetAgeCountSeries()
    {
        return new ChartSeries
        {
            Name = "User Age Demographics",
            Data = this.AgeCounts.Select(kvp => (double)kvp.Value).ToArray(),
        };
    }

    private string[] GetAgeLabels()
    {
        return this.AgeCounts.Select((kvp, i) => i % 10 == 0 ? kvp.Key.ToString(CultureInfo.InvariantCulture) : string.Empty).ToArray();
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
        /// Gets the user login count.
        /// </summary>
        public int UserLogins { get; init; }

        /// <summary>
        /// Gets the dependent registration count.
        /// </summary>
        public int DependentRegistrations { get; init; }
    }
}
