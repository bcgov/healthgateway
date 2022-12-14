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

/// <summary>
/// The effects for the feature.
/// </summary>
public class DashboardEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="dashboardApi">the injected api to query the dashboard service. </param>
    public DashboardEffects(ILogger<DashboardEffects> logger, IDashboardApi dashboardApi)
    {
        this.Logger = logger;
        this.DashboardApi = dashboardApi;
    }

    [Inject]
    private ILogger<DashboardEffects> Logger { get; set; }

    [Inject]
    private IDashboardApi DashboardApi { get; set; }

    /// <summary>
    /// Handler that calls the dashboard service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(DashboardActions.LoadRegisteredUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading registered users!");
        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetRegisteredUserCountAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Registered users retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.RegisteredUsersSuccessAction(response));
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving registered users, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.RegisteredUsersFailAction(error));
        }
    }

    /// <summary>
    /// Handler that calls the dashboard service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(DashboardActions.LoadLoggedInUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading logged in users!");

        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetLoggedinUsersCountAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Logged in users retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.LoggedInUsersSuccessAction(response));
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving logged in users, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.LoggedInUsersFailAction(error));
        }
    }

    /// <summary>
    /// Handler that calls the dashboard service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(DashboardActions.LoadDependentsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading dependents!");

        try
        {
            IDictionary<DateTime, int> response = await this.DashboardApi.GetDependentCountAsync(action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Dependents retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.DependentsSuccessAction(response));
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving dependents, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.DependentsFailAction(error));
        }
    }

    /// <summary>
    /// Handler that calls the dashboard service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(DashboardActions.LoadRecurringUsersAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading recurring user counts!");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetRecurringUserCountsAsync(action.Days, action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Recurring user counts retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.RecurringUsersSuccessAction(response));
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving recurring user counts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.RecurringUsersFailAction(error));
        }
    }

    /// <summary>
    /// Handler that calls the dashboard service and dispatch the actions.
    /// </summary>
    /// <param name="action">Load the initial action.</param>
    /// <param name="dispatcher">Dispatch the actions.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleLoadAction(DashboardActions.LoadRatingSummaryAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading rating summary!");

        try
        {
            IDictionary<string, int> response = await this.DashboardApi.GetRatingsSummaryAsync(action.StartPeriod, action.EndPeriod, action.TimeOffset).ConfigureAwait(true);
            this.Logger.LogInformation("Rating summary retrieved successfully!");
            dispatcher.Dispatch(new DashboardActions.RatingSummarySuccessAction(response));
        }
        catch (ApiException ex)
        {
            RequestError error = StoreUtility.FormatRequestError(ex);
            this.Logger.LogError("Error retrieving rating summary, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new DashboardActions.RatingSummaryFailAction(error));
        }
    }
}
