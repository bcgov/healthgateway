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
namespace HealthGateway.Admin.Client.Store.Analytics;

using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

/// <summary>
/// The effects for the feature.
/// </summary>
public class AnalyticsEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="analyticsApi">the injected api to query the analytics service. </param>
    public AnalyticsEffects(ILogger<AnalyticsEffects> logger, IAnalyticsApi analyticsApi)
    {
        this.Logger = logger;
        this.AnalyticsApi = analyticsApi;
    }

    [Inject]
    private ILogger<AnalyticsEffects> Logger { get; set; }

    [Inject]
    private IAnalyticsApi AnalyticsApi { get; set; }

    /// <summary>
    /// Handler that calls the user profiles service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadUserProfilesAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading user profile report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetUserProfiles(action.StartDate, action.EndDate).ConfigureAwait(true);
        this.Logger.LogInformation("User profiles report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting user profile report",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the comments service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadCommentsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading comments report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetComments(action.StartDate, action.EndDate).ConfigureAwait(true);
        this.Logger.LogInformation("Comments report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting comments report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the notes service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadNotesAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading notes report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetNotes(action.StartDate, action.EndDate).ConfigureAwait(true);
        this.Logger.LogInformation("Notes report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting notes report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the ratings service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadRatingsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading ratings report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetRatings(action.StartDate, action.EndDate).ConfigureAwait(true);
        this.Logger.LogInformation("Ratings report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting rating report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the inactive users service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadInactiveUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading inactive users report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetInactiveUsers(action.InactiveDays, action.TimeOffset).ConfigureAwait(true);
        this.Logger.LogInformation("Inactive users report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting inactive users report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the user feedback service and dispatches the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadUserFeedbackAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading user feedback report");

        HttpResponseMessage response = await this.AnalyticsApi.GetUserFeedback().ConfigureAwait(true);
        this.Logger.LogInformation("User Feedback report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting user feedback report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the year of birth counts service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadYearOfBirthCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading year of birth counts report.");

        HttpResponseMessage response = await this.AnalyticsApi.GetYearOfBirthCounts(action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
        this.Logger.LogInformation("Year of birth counts report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting year of birth counts report.",
        };

        this.Logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailAction(error));
    }
}
