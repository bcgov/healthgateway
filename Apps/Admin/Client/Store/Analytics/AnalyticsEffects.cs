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
using HealthGateway.Admin.Client.Api;
using Microsoft.Extensions.Logging;

public class AnalyticsEffects(ILogger<AnalyticsEffects> logger, IAnalyticsApi analyticsApi)
{
    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadUserProfilesAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading user profile report");

        HttpResponseMessage response = await analyticsApi.GetUserProfilesAsync(action.StartDate, action.EndDate).ConfigureAwait(true);
        logger.LogInformation("User profiles report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting user profile report",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadCommentsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading comments report");

        HttpResponseMessage response = await analyticsApi.GetCommentsAsync(action.StartDate, action.EndDate).ConfigureAwait(true);
        logger.LogInformation("Comments report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting comments report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadNotesAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading notes report");

        HttpResponseMessage response = await analyticsApi.GetNotesAsync(action.StartDate, action.EndDate).ConfigureAwait(true);
        logger.LogInformation("Notes report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting notes report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadRatingsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading ratings report");

        HttpResponseMessage response = await analyticsApi.GetRatingsAsync(action.StartDate, action.EndDate).ConfigureAwait(true);
        logger.LogInformation("Ratings report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting rating report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadInactiveUsersAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading inactive users report");

        HttpResponseMessage response = await analyticsApi.GetInactiveUsersAsync(action.InactiveDays).ConfigureAwait(true);
        logger.LogInformation("Inactive users report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting inactive users report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadUserFeedbackAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading user feedback report");

        HttpResponseMessage response = await analyticsApi.GetUserFeedbackAsync().ConfigureAwait(true);
        logger.LogInformation("User Feedback report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting user feedback report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadAction(AnalyticsActions.LoadYearOfBirthCountsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading year of birth counts report");

        HttpResponseMessage response = await analyticsApi.GetYearOfBirthCountsAsync(action.StartDateLocal, action.EndDateLocal).ConfigureAwait(true);
        logger.LogInformation("Year of birth counts report exported successfully!");
        if (response.IsSuccessStatusCode)
        {
            dispatcher.Dispatch(new AnalyticsActions.LoadSuccessAction { Data = response.Content });
            return;
        }

        RequestError error = new()
        {
            Message = "Error exporting year of birth counts report.",
        };

        logger.LogError("{ErrorMessage}", error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }
}
