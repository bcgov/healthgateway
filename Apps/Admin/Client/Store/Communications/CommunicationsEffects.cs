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

namespace HealthGateway.Admin.Client.Store.Communications;

using System.Collections.Generic;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Services;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

/// <summary>
/// The effects for the feature.
/// </summary>
public class CommunicationsEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommunicationsEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="api">The injected API.</param>
    public CommunicationsEffects(ILogger<CommunicationsEffects> logger, ICommunicationsApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<CommunicationsEffects> Logger { get; set; }

    [Inject]
    private ICommunicationsApi Api { get; set; }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleAddAction(CommunicationsActions.AddAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Adding communication");

        ApiResponse<RequestResult<Communication>> response = await this.Api.Add(action.Communication).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Communication added successfully!");
            dispatcher.Dispatch(new CommunicationsActions.AddSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error adding communication, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new CommunicationsActions.AddFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod(typeof(CommunicationsActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading communications");

        ApiResponse<RequestResult<IEnumerable<Communication>>> response = await this.Api.GetAll().ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Communications loaded successfully!");
            dispatcher.Dispatch(new CommunicationsActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error loading communications, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new CommunicationsActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleUpdateAction(CommunicationsActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating communication");

        ApiResponse<RequestResult<Communication>> response = await this.Api.Update(action.Communication).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Communication updated successfully!");
            dispatcher.Dispatch(new CommunicationsActions.UpdateSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error updating communication, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new CommunicationsActions.UpdateFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleDeleteAction(CommunicationsActions.DeleteAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Deleting communication");

        ApiResponse<RequestResult<Communication>> response = await this.Api.Delete(action.Communication).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Communication deleted successfully!");
            dispatcher.Dispatch(new CommunicationsActions.DeleteSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error deleting communication, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new CommunicationsActions.DeleteFailAction(error));
    }
}
