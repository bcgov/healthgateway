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

namespace HealthGateway.Admin.Client.Store.Tag;

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
public class TagEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="api">The injected API.</param>
    public TagEffects(ILogger<TagEffects> logger, ITagApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<TagEffects> Logger { get; set; }

    [Inject]
    private ITagApi Api { get; set; }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleAddAction(TagActions.AddAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Adding tag");

        ApiResponse<RequestResult<AdminTagView>> response = await this.Api.Add(action.TagName).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("AdminTagView added successfully!");
            dispatcher.Dispatch(new TagActions.AddSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError($"Error adding tag, reason: {error.Message}");
        dispatcher.Dispatch(new TagActions.AddFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod(typeof(TagActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading Tag");

        ApiResponse<RequestResult<IEnumerable<AdminTagView>>> response = await this.Api.GetAll().ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Tag loaded successfully!");
            dispatcher.Dispatch(new TagActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError($"Error loading Tag, reason: {error.Message}");
        dispatcher.Dispatch(new TagActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleDeleteAction(TagActions.DeleteAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Deleting tag");

        ApiResponse<RequestResult<AdminTagView>> response = await this.Api.Delete(action.AdminTagView).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Tag deleted successfully!");
            dispatcher.Dispatch(new TagActions.DeleteSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError($"Error deleting tag, reason: {error.Message}");
        dispatcher.Dispatch(new TagActions.DeleteFailAction(error));
    }
}
