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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

public class DashboardEffects
{
    public DashboardEffects(ILogger<DashboardEffects> logger, IDashboardApi dashboardApi)
    {
        this.Logger = logger;
        this.DashboardApi = dashboardApi;
    }

    [Inject]
    private ILogger<DashboardEffects> Logger { get; set; }

    [Inject]
    private IDashboardApi DashboardApi { get; set; }

    [EffectMethod]
    public async Task HandleGetDailyUserRegistrationCountsAction(DashboardActions.GetDailyUserRegistrationCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving daily user registration counts");
        try
        {
            IDictionary<DateOnly, int> response = await this.DashboardApi.GetDailyUserRegistrationCountsAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Daily user registration counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetDailyUserRegistrationCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving daily user registration counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetDailyUserRegistrationCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetDailyDependentRegistrationCountsAction(DashboardActions.GetDailyDependentRegistrationCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving daily dependent registration counts");

        try
        {
            IDictionary<DateOnly, int> response = await this.DashboardApi.GetDailyDependentRegistrationCountsAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Daily dependent registration counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetDailyDependentRegistrationCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving daily dependent registration counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetDailyDependentRegistrationCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetDailyUniqueLoginCountsAction(DashboardActions.GetDailyUniqueLoginCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving daily unique login counts");

        try
        {
            IDictionary<DateOnly, int> response = await this.DashboardApi.GetDailyUniqueLoginCountsAsync(action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Daily unique login counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetDailyUniqueLoginCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving daily unique login counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetDailyUniqueLoginCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetRecurringUserCountAction(DashboardActions.GetRecurringUserCountAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving recurring user count");

        try
        {
            int response = await this.DashboardApi.GetRecurringUserCountAsync(action.Days, action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Recurring user count retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving recurring user count, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRecurringUserCountFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetAppLoginCountsAction(DashboardActions.GetAppLoginCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving app login counts");

        try
        {
            AppLoginCounts response = await this.DashboardApi.GetAppLoginCountsAsync(action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("App login counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving recurring app login counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetAppLoginCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetRatingsSummaryAction(DashboardActions.GetRatingsSummaryAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving ratings summary");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetRatingsSummaryAsync(action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Ratings summary retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetRatingsSummarySuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving ratings summary, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRatingsSummaryFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetYearOfBirthCountsAction(DashboardActions.GetYearOfBirthCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving year of birth counts");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetYearOfBirthCountsAsync(action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Year of birth counts retrieved successfully");
            dispatcher.Dispatch(new DashboardActions.GetYearOfBirthCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving year of birth counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetYearOfBirthCountsFailureAction { Error = error });
        }
    }
}
