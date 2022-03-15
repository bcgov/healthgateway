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
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using HealthGateway.Admin.Client.Store.Analytics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Backing logic for the Analytics page.
/// </summary>
public partial class SystemAnalytics : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> UserProfilesState { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> CommentsState { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> NotesState { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> RatingsState { get; set; } = default!;

    [Inject]
    private IState<AnalyticsState> InactiveUsersState { get; set; } = default!;

    [Inject]
    private IActionSubscriber ActionSubscriber { get; set; } = default!;

    private int InactiveDays { get; set; } = 90;

    private int TimeOffset { get; set; } = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes * -1;

    private HttpContent UserProfilesData => this.UserProfilesState.Value.UserProfilesReport.Data ?? default!;

    private HttpContent CommentsData => this.CommentsState.Value.CommentsReport.Data ?? default!;

    private HttpContent NotesData => this.NotesState.Value.NotesReport.Data ?? default!;

    private HttpContent RatingsData => this.RatingsState.Value.RatingsReport.Data ?? default!;

    private HttpContent InactiveUsersData => this.InactiveUsersState.Value.InactiveUsersReport.Data ?? default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        this.ResetAnalyticsStateAction();
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessUserProfilesAction>(this, this.DownloadUserProfilesReport);
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessCommentsAction>(this, this.DownloadCommentsReport);
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessNotesAction>(this, this.DownloadNotesReport);
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessRatingsAction>(this, this.DownloadRatingsReport);
        this.ActionSubscriber.SubscribeToAction<AnalyticsActions.LoadSuccessInactiveUsersAction>(this, this.DownloadInactiveUsersReport);
    }

    private static Type GetResetStateAction()
    {
        return typeof(AnalyticsActions.ResetInactiveUsersStateAction);
    }

    private bool HasError()
    {
        return (this.UserProfilesState.Value.UserProfilesReport.RequestError != null && this.UserProfilesState.Value.UserProfilesReport.RequestError.Message.Length > 0) ||
            (this.InactiveUsersState.Value.InactiveUsersReport.RequestError != null && this.InactiveUsersState.Value.InactiveUsersReport.RequestError.Message.Length > 0);
    }

    private void GetProfilesData()
    {
        this.ResetUserProfilesStateAction();
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadUserProfilesAction(null, null));
    }

    private void GetCommentsData()
    {
        this.ResetCommentsStateAction();
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadCommentsAction(null, null));
    }

    private void GetNotesData()
    {
        this.ResetNotesStateAction();
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadNotesAction(null, null));
    }

    private void GetRatingsData()
    {
        this.ResetRatingsStateAction();
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadRatingsAction(null, null));
    }

    private void GetInactiveUsersData()
    {
        this.ResetInactiveUsersState();
        this.Dispatcher.Dispatch(new AnalyticsActions.LoadInactiveUsersAction(this.InactiveDays, this.TimeOffset));
    }

    private void DownloadUserProfilesReport(AnalyticsActions.LoadSuccessUserProfilesAction action)
    {
        Task.Run(async () => await this.DownloadReport("UserProfiles", this.UserProfilesData).ConfigureAwait(true));
    }

    private void DownloadCommentsReport(AnalyticsActions.LoadSuccessCommentsAction action)
    {
        Task.Run(async () => await this.DownloadReport("Comments", this.CommentsData).ConfigureAwait(true));
    }

    private void DownloadNotesReport(AnalyticsActions.LoadSuccessNotesAction action)
    {
        Task.Run(async () => await this.DownloadReport("Notes", this.NotesData).ConfigureAwait(true));
    }

    private void DownloadRatingsReport(AnalyticsActions.LoadSuccessRatingsAction action)
    {
        Task.Run(async () => await this.DownloadReport("Ratings", this.RatingsData).ConfigureAwait(true));
    }

    private void DownloadInactiveUsersReport(AnalyticsActions.LoadSuccessInactiveUsersAction action)
    {
        Task.Run(async () => await this.DownloadReport("InactiveUsers", this.InactiveUsersData).ConfigureAwait(true));
    }

    private async Task DownloadReport(string reportName, HttpContent content)
    {
        var fileBytes = await content.ReadAsByteArrayAsync().ConfigureAwait(true);
        var fileName = $"{reportName}_export_{DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}.csv";
        await this.JSRuntime.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(fileBytes)).ConfigureAwait(true);
    }

    private void ResetAnalyticsStateAction()
    {
        this.ResetUserProfilesStateAction();
        this.ResetCommentsStateAction();
        this.ResetNotesStateAction();
        this.ResetRatingsStateAction();
        this.ResetInactiveUsersState();
    }

    private void ResetUserProfilesStateAction()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetUserProfilesStateAction());
    }

    private void ResetCommentsStateAction()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetCommentsStateAction());
    }

    private void ResetNotesStateAction()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetNotesStateAction());
    }

    private void ResetRatingsStateAction()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetRatingsStateAction());
    }

    private void ResetInactiveUsersState()
    {
        this.Dispatcher.Dispatch(new AnalyticsActions.ResetInactiveUsersStateAction());
    }
}
