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

namespace HealthGateway.Admin.Client.Store.Dashboard;

using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using Microsoft.Extensions.Logging;
using Refit;

public class DashboardEffects(ILogger<DashboardEffects> logger, IDashboardApi dashboardApi)
{
    [EffectMethod(typeof(DashboardActions.GetAllTimeCountsAction))]
    public async Task HandleGetAllTimeCountsAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving all-time counts");
        try
        {
            AllTimeCounts response = await dashboardApi.GetAllTimeCountsAsync();
            logger.LogInformation("All-time counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetAllTimeCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving all-time counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetAllTimeCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetDailyUsageCountsAction(DashboardActions.GetDailyUsageCountsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving daily usage counts");

        try
        {
            DailyUsageCounts response = await dashboardApi.GetDailyUsageCountsAsync(action.StartDateLocal, action.EndDateLocal);
            logger.LogInformation("Daily usage counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetDailyUsageCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving daily usage counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetDailyUsageCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetRecurringUserCountAction(DashboardActions.GetRecurringUserCountAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving recurring user count");

        try
        {
            int response = await dashboardApi.GetRecurringUserCountAsync(action.Days, action.StartDateLocal, action.EndDateLocal);
            logger.LogInformation("Recurring user count retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving recurring user count, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetAppLoginCountsAction(DashboardActions.GetAppLoginCountsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving app login counts");

        try
        {
            AppLoginCounts response = await dashboardApi.GetAppLoginCountsAsync(action.StartDateLocal, action.EndDateLocal);
            logger.LogInformation("App login counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving recurring app login counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetRatingsSummaryAction(DashboardActions.GetRatingsSummaryAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving ratings summary");

        try
        {
            IDictionary<string, int> response = await dashboardApi.GetRatingsSummaryAsync(action.StartDateLocal, action.EndDateLocal);
            logger.LogInformation("Ratings summary retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetRatingsSummarySuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving ratings summary, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRatingsSummaryFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetAgeCountsAction(DashboardActions.GetAgeCountsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving age counts");

        try
        {
            IDictionary<int, int> response = await dashboardApi.GetAgeCountsAsync(action.StartDateLocal, action.EndDateLocal);
            logger.LogInformation("Age counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetAgeCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            logger.LogError("Error retrieving age counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetAgeCountsFailureAction { Error = error });
        }
    }
}
