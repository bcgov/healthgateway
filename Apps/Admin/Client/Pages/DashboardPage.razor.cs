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

using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store;
using HealthGateway.Admin.Client.Store.Dashboard;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Backing logic for the Dashboard page.
/// </summary>
public partial class DashboardPage : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<DashboardState> DashboardState { get; set; } = default!;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; set; } = new DateTime(2019, 06, 1);

    private DateTime MaximumDateTime { get; set; } = DateTime.Now;

    private DateRange SelectedDateRange { get; set; } = new DateRange(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int UniqueDays { get; set; } = 3;

    private List<string> UniquePeriodDates { get; set; } = new()
    {
        DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
    };

    private int TimeOffset { get; set; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes * -1;

    private BaseRequestState<IDictionary<DateTime, int>> RegisteredUsersResult => this.DashboardState.Value.RegisteredUsers ?? default!;

    private BaseRequestState<IDictionary<DateTime, int>> LoggedInUsersResult => this.DashboardState.Value.LoggedInUsers ?? default!;

    private BaseRequestState<IDictionary<DateTime, int>> DependentsResult => this.DashboardState.Value.Dependents ?? default!;

    private BaseRequestState<RecurringUser> RecurringUsersResult => this.DashboardState.Value.RecurringUsers ?? default!;

    private BaseRequestState<IDictionary<string, int>> RatingSummaryResult => this.DashboardState.Value.RatingSummary ?? default!;

    private int TotalRegisteredUsers
    {
        get
        {
            if (this.RegisteredUsersResult?.Result != null)
            {
                var results = from result in this.RegisteredUsersResult?.Result
                              select result.Value;

                return results.Sum();
            }

            return 0;
        }
    }

    private int TotalDependents
    {
        get
        {
            if (this.DependentsResult?.Result != null)
            {
                var results = from result in this.DependentsResult?.Result
                              select result.Value;

                return results.Sum();
            }

            return 0;
        }
    }

    private int TotalUniqueUsers
    {
        get
        {
            if (this.RecurringUsersResult?.Result != null)
            {
               return this.RecurringUsersResult?.Result.TotalRecurringUsers ?? 0;
            }

            return 0;
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetDashboardState();
        this.LoadDispatchActions();
    }

    private void LoadDispatchActions()
    {
        this.Dispatcher.Dispatch(new DashboardActions.RegisteredUsersAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.LoggedInUsersAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.DependentsAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.RecurringUsersAction(this.UniqueDays, this.UniquePeriodDates.FirstOrDefault(), this.UniquePeriodDates.LastOrDefault(), this.TimeOffset));
    }

    private void ResetDashboardState()
    {
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
    }
}
