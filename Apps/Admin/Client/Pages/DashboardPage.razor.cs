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

    private BaseRequestState<IDictionary<DateTime, int>> RegisteredUsersResult => this.DashboardState.Value.RegisteredUsers ?? default!;

    private BaseRequestState<IDictionary<DateTime, int>> LoggedInUsersResult => this.DashboardState.Value.LoggedInUsers ?? default!;

    private BaseRequestState<IDictionary<DateTime, int>> DependentsResult => this.DashboardState.Value.Dependents ?? default!;

    private BaseRequestState<RecurringUser> RecurringUsersResult => this.DashboardState.Value.RecurringUsers ?? default!;

    private BaseRequestState<IDictionary<string, int>> RatingSummaryResult => this.DashboardState.Value.RatingSummary ?? default!;

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; set; } = new DateTime(2019, 06, 1);

    private DateTime MaximumDateTime { get; set; } = DateTime.Now;

    private DateRange SelectedDateRange { get; set; } = new DateRange(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private int CurrentUniqueDays { get; set; } = 3;

    private bool RegisteredUsersHasError => this.DashboardState.Value.RegisteredUsers.Error != null && this.DashboardState.Value.RegisteredUsers.Error.Message.Length > 0;

    private string? RegisteredUsersErrorMessage => this.RegisteredUsersHasError ? this.DashboardState?.Value.RegisteredUsers?.Error?.Message : string.Empty;

    private bool LoggedInUsersHasError => this.DashboardState.Value.LoggedInUsers.Error != null && this.DashboardState.Value.LoggedInUsers.Error.Message.Length > 0;

    private string? LoggedInUsersErrorMessage => this.LoggedInUsersHasError ? this.DashboardState?.Value.LoggedInUsers?.Error?.Message : string.Empty;

    private bool DependentsHasError => this.DashboardState.Value.Dependents.Error != null && this.DashboardState.Value.Dependents.Error.Message.Length > 0;

    private string? DependentsErrorMessage => this.DependentsHasError ? this.DashboardState?.Value.Dependents?.Error?.Message : string.Empty;

    private bool RecurringUsersHasError => this.DashboardState.Value.RecurringUsers.Error != null && this.DashboardState.Value.RecurringUsers.Error.Message.Length > 0;

    private string? RecurringUsersErrorMessage => this.RecurringUsersHasError ? this.DashboardState?.Value.RecurringUsers?.Error?.Message : string.Empty;

    private bool RatingSummaryHasError => this.DashboardState.Value.RatingSummary.Error != null && this.DashboardState.Value.RatingSummary.Error.Message.Length > 0;

    private string? RatingSummaryErrorMessage => this.RatingSummaryHasError ? this.DashboardState?.Value.RatingSummary?.Error?.Message : string.Empty;

    private bool HasError
    {
        get
        {
            return this.RegisteredUsersHasError ||
                this.LoggedInUsersHasError ||
                this.DependentsHasError ||
                this.RecurringUsersHasError ||
                this.RatingSummaryHasError;
        }
    }

    private string ErrorMessage
    {
        get
        {
            string? errorMessage = string.Empty;
            if (this.RegisteredUsersHasError)
            {
                errorMessage += this.RegisteredUsersErrorMessage + ErrorMessageHasNewLine(errorMessage);
            }

            if (this.LoggedInUsersHasError)
            {
                errorMessage += this.LoggedInUsersErrorMessage + ErrorMessageHasNewLine(errorMessage);
            }

            if (this.DependentsHasError)
            {
                errorMessage += this.DependentsErrorMessage + ErrorMessageHasNewLine(errorMessage);
            }

            if (this.RecurringUsersHasError)
            {
                errorMessage += this.RecurringUsersErrorMessage + ErrorMessageHasNewLine(errorMessage);
            }

            if (this.RecurringUsersHasError)
            {
                errorMessage += this.RatingSummaryErrorMessage + ErrorMessageHasNewLine(errorMessage);
            }

            return errorMessage ?? string.Empty;
        }
    }

    private int UniqueDays
    {
        get
        {
            return this.CurrentUniqueDays;
        }

        set
        {
            this.Dispatcher.Dispatch(new DashboardActions.RecurringUsersAction(value, this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), this.TimeOffset));
            this.CurrentUniqueDays = value;
        }
    }

    private List<string> UniquePeriodDates { get; set; } = PeriodDatesList();

    private List<string> RatingPeriodDates { get; set; } = PeriodDatesList();

    private int TimeOffset { get; set; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes;

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

    private IEnumerable<DashboardDailyData> TableData
    {
        get
        {
            DateTime startDate = DateTime.Now.AddDays(-10);
            DateTime endDate = DateTime.Now;
            List<DashboardDailyData> results = new();

            if (this.RegisteredUsersResult?.Result != null)
            {
                var registeredUsers = from result in this.RegisteredUsersResult?.Result
                                        select result;

                foreach (var user in registeredUsers)
                {
                    DashboardDailyData dashboardDailyData = new()
                    {
                        DailyDateTime = user.Key,
                        TotalRegisteredUsers = user.Value,
                    };

                    results.Add(dashboardDailyData);
                }
            }

            if (this.LoggedInUsersResult?.Result != null)
            {
                var loggedInUsers = from result in this.LoggedInUsersResult?.Result
                                 select result;
                foreach (var loggedInUser in loggedInUsers)
                {
                    DashboardDailyData dashboardDailyData = new()
                    {
                        DailyDateTime = loggedInUser.Key,
                        TotalLoggedInUsers = loggedInUser.Value,
                    };

                    results.Add(dashboardDailyData);
                }
            }

            if (this.DependentsResult?.Result != null)
            {
                var dependents = from result in this.DependentsResult?.Result
                                select result;
                foreach (var dependent in dependents)
                {
                    DashboardDailyData dashboardDailyData = new()
                    {
                        DailyDateTime = dependent.Key,
                        TotalDependents = dependent.Value,
                    };

                    results.Add(dashboardDailyData);
                }
            }

            var filteredResults = from result in results
                                  where startDate <= result.DailyDateTime && result.DailyDateTime <= endDate
                                  select result;
            return filteredResults;
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

    private int RatingCount
    {
        get
        {
            if (this.RatingSummaryResult?.Result != null)
            {
                return (from result in this.RatingSummaryResult?.Result
                        select result.Value).Count();
            }

            return 0;
        }
    }

    private IDictionary<string, int>? RatingSummaryResults
    {
        get
        {
            if (this.RatingSummaryResult?.Result != null)
            {
                return this.RatingSummaryResult?.Result;
            }

            return new Dictionary<string, int>();
        }
    }

    // Tuple<index,ProgressBarvalue,RatingTotal>
    private List<Tuple<string, int, int>>? RatingSummary
    {
        get
        {
            var ratingSummary = this.RatingSummaryResults;
            var results = new List<Tuple<string, int, int>>();
            if (ratingSummary != null)
            {
               for (int i = 5; i >= 1; i--)
                {
                    string index = i.ToString(CultureInfo.InvariantCulture);
                    int ratingTotal = (from value in ratingSummary
                                      where value.Key == index
                                      select value).Count();

                    int ratingValue = (from value in ratingSummary
                                       where value.Key == index
                                       select value.Value).Sum();

                    int item = this.RatingCount > 0 ? (ratingValue / this.RatingCount) * 100 : 0;
                    results.Add(Tuple.Create(index, item, ratingTotal));
                }
            }

            return results;
        }
    }

    private string RatingAverage
    {
        get
        {
            if (this.RatingSummaryResult?.Result != null)
            {
                var totalCount = this.RatingCount;

                var ratingSummary = this.RatingSummaryResults;
                decimal totalScore = 0M;
                if (ratingSummary != null)
                {
                    totalScore = (from value in ratingSummary
                                 select Convert.ToInt32(value.Key, CultureInfo.InvariantCulture) * value.Value).Sum();

                    return totalCount != 0 ? (totalScore / totalCount).ToString("0.00", CultureInfo.InvariantCulture) : "N/A";
                }
            }

            return "N/A";
        }
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetDashboardState();
        this.LoadDispatchActions();
    }

    private static List<string> PeriodDatesList()
    {
        return new()
        {
            DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        };
    }

    private static bool ErrorMessageHasNewLine(string errorMessage)
    {
        return errorMessage != null && errorMessage.Length > 0;
    }

    private void LoadDispatchActions()
    {
        this.Dispatcher.Dispatch(new DashboardActions.RegisteredUsersAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.LoggedInUsersAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.DependentsAction(this.TimeOffset));
        this.Dispatcher.Dispatch(new DashboardActions.RecurringUsersAction(this.UniqueDays, this.UniquePeriodDates.FirstOrDefault(), this.UniquePeriodDates.LastOrDefault(), this.TimeOffset));
        this.DispatchRatingSummaryAction();
    }

    private void DispatchRecurringUserActionWithDateChanged()
    {
        this.Dispatcher.Dispatch(new DashboardActions.RecurringUsersAction(this.UniqueDays, this.SelectedDateRange.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), this.SelectedDateRange.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), this.TimeOffset));
        this.SelectedDateRangePicker.Close();
    }

    private void DispatchRatingSummaryAction()
    {
        string endDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        this.Dispatcher.Dispatch(new DashboardActions.RatingSummaryAction(this.RatingPeriodDates.FirstOrDefault(), endDate, this.TimeOffset));
    }

    private void ResetDashboardState()
    {
        this.Dispatcher.Dispatch(new DashboardActions.ResetStateAction());
    }
}
