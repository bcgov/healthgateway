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

namespace HealthGateway.Admin.Client.Store.Broadcasts;

using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Services;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using HealthGateway.Common.Data.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

/// <summary>
/// The effects for the feature.
/// </summary>
public class BroadcastsEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BroadcastsEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="api">The injected API.</param>
    public BroadcastsEffects(ILogger<BroadcastsEffects> logger, IBroadcastsApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<BroadcastsEffects> Logger { get; set; }

    [Inject]
    private IBroadcastsApi Api { get; set; }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleAddAction(BroadcastsActions.AddAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Adding broadcast");

        ApiResponse<RequestResult<Broadcast>> response = await this.Api.Add(action.Broadcast).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Broadcast added successfully!");
            dispatcher.Dispatch(new BroadcastsActions.AddSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error adding broadcast, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new BroadcastsActions.AddFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod(typeof(BroadcastsActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading broadcasts");

        ApiResponse<RequestResult<IEnumerable<Broadcast>>> response = await this.Api.GetAll().ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Broadcasts loaded successfully!");
            dispatcher.Dispatch(new BroadcastsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error loading broadcasts, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new BroadcastsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleUpdateAction(BroadcastsActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating broadcast");

        ApiResponse<RequestResult<Broadcast>> response = await this.Api.Update(action.Broadcast).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Broadcast updated successfully!");
            dispatcher.Dispatch(new BroadcastsActions.UpdateSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error updating broadcast, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new BroadcastsActions.UpdateFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleDeleteAction(BroadcastsActions.DeleteAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Deleting broadcast");

        ApiResponse<RequestResult<Broadcast>> response = await this.Api.Delete(action.Broadcast).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Broadcast deleted successfully!");
            dispatcher.Dispatch(new BroadcastsActions.DeleteSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error deleting broadcast, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new BroadcastsActions.DeleteFailAction(error));
    }
}
