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
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Analytics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

/// <summary>
/// Backing logic for the Analytics page.
/// </summary>
public partial class AnalyticsPage : FluxorComponent
{
    private const string YearOfBirthReportName = "YearOfBirthCounts";

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> AnalyticsState { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    private int InactiveDays { get; set; } = 90;

    private string? StartDate { get; set; } = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private string? EndDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private int TimeOffset { get; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes * -1;

    private DateRange DateRange { get; set; } = new(DateTime.Now.AddDays(-30).Date, DateTime.Now.Date);

    private DateRange SelectedDateRange
    {
        get => this.DateRange;

        set
        {
            this.StartDate = value.Start?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.EndDate = value.End?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.DateRange = value;
        }
    }

    private MudDateRangePicker SelectedDateRangePicker { get; set; } = default!;

    private DateTime MinimumDateTime { get; } = new(2019, 06, 1);

    private DateTime MaximumDateTime { get; } = DateTime.Now;

    private HttpContent AnalyticsStateData => this.AnalyticsState.Value.Result ?? default!;

    private bool HasError => this.AnalyticsState.Value.Error != null && this.AnalyticsState.Value.Error.Message.Length > 0;

    private string? ReportName { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetAnalyticsState();
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessAction>(this, this.DownloadAnalyticsReport);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        this.ActionSubscriber.UnsubscribeFromAllActions(this);
        base.Dispose(disposing);
    }

    private void GetProfilesData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "UserProfile";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadUserProfilesAction(null, null));
    }

    private void GetCommentsData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "Comments";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadCommentsAction(null, null));
    }

    private void GetNotesData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "Notes";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadNotesAction(null, null));
    }

    private void GetRatingsData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "Ratings";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadRatingsAction(null, null));
    }

    private void GetInactiveUsersData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "InactiveUsers";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadInactiveUsersAction(this.InactiveDays, this.TimeOffset));
    }

    private void GetUserFeedbackData()
    {
        this.ResetAnalyticsState();
        this.ReportName = "UserFeedback";
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadUserFeedbackAction());
    }

    private void GetYearOfBirthCountsData()
    {
        this.ResetAnalyticsState();
        this.ReportName = YearOfBirthReportName;
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadYearOfBirthCountsAction(this.StartDate, this.EndDate, this.TimeOffset));
    }

    private void DownloadAnalyticsReport(AnalyticsActions.LoadSuccessAction action)
    {
        Task.Run(async () => await this.DownloadReport(this.AnalyticsStateData).ConfigureAwait(true));
    }

    private void ResetAnalyticsState()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetStateAction());
    }

    private async Task DownloadReport(HttpContent content)
    {
        byte[] fileBytes = await content.ReadAsByteArrayAsync().ConfigureAwait(true);
        string fileName = this.ReportName == YearOfBirthReportName
            ? $"{this.ReportName}_export_{this.StartDate}_to_{this.EndDate}.csv"
            : $"{this.ReportName}_export_{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.csv";

        await this.JsRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes)).ConfigureAwait(true);
    }
}
