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
    private const string ErrorMessage = "{ErrorMessage}";

    [EffectMethod(typeof(AnalyticsActions.LoadUserProfilesAction))]
    public async Task HandleLoadUserProfilesAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading user profile report");

        HttpResponseMessage response = await analyticsApi.GetUserProfilesAsync();
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod(typeof(AnalyticsActions.LoadCommentsAction))]
    public async Task HandleLoadCommentsAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading comments report");

        HttpResponseMessage response = await analyticsApi.GetCommentsAsync();
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod(typeof(AnalyticsActions.LoadNotesAction))]
    public async Task HandleLoadNotesAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading notes report");

        HttpResponseMessage response = await analyticsApi.GetNotesAsync();
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod(typeof(AnalyticsActions.LoadRatingsAction))]
    public async Task HandleLoadRatingsAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading ratings report");

        HttpResponseMessage response = await analyticsApi.GetRatingsAsync();
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadInactiveUsersAction(AnalyticsActions.LoadInactiveUsersAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading inactive users report");

        HttpResponseMessage response = await analyticsApi.GetInactiveUsersAsync(action.InactiveDays);
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod(typeof(AnalyticsActions.LoadUserFeedbackAction))]
    public async Task HandleLoadUserFeedbackAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading user feedback report");

        HttpResponseMessage response = await analyticsApi.GetUserFeedbackAsync();
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }

    [EffectMethod]
    public async Task HandleLoadYearOfBirthCountsAction(AnalyticsActions.LoadYearOfBirthCountsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Loading year of birth counts report");

        HttpResponseMessage response = await analyticsApi.GetYearOfBirthCountsAsync(action.StartDateLocal, action.EndDateLocal);
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

        logger.LogError(ErrorMessage, error.Message);
        dispatcher.Dispatch(new AnalyticsActions.LoadFailureAction { Error = error });
    }
}
