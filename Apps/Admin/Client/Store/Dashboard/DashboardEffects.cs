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
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

#pragma warning disable CS1591, SA1600
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
    public async Task HandleGetRegisteredUsersAction(DashboardActions.GetRegisteredUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving registered users");
        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetRegisteredUserCountAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Registered users retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.GetRegisteredUsersSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving registered users, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRegisteredUsersFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetLoggedInUsersAction(DashboardActions.GetLoggedInUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving logged in users");

        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetLoggedinUsersCountAsync(action.StartDateLocal, action.EndDateLocal, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Logged in users retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.GetLoggedInUsersSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving logged in users, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetLoggedInUsersFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetDependentsAction(DashboardActions.GetDependentsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving dependents");

        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetDependentCountAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Dependents retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.GetDependentsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving dependents, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetDependentsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetUserCountsAction(DashboardActions.GetUserCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving user counts");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetUserCountsAsync(action.Days, action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("User counts retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.GetUserCountsSuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving user counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetUserCountsFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetRatingSummaryAction(DashboardActions.GetRatingSummaryAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving rating summary");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetRatingsSummaryAsync(action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Rating summary retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.GetRatingSummarySuccessAction { Data = response });
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving rating summary, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.GetRatingSummaryFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetYearOfBirthCountsAction(DashboardActions.GetYearOfBirthCountsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving year of birth counts");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetYearOfBirthCountsAsync(action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Year of birth counts retrieved successfully!");
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
