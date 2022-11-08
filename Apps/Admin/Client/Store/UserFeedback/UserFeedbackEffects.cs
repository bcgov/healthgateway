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
namespace HealthGateway.Admin.Client.Store.UserFeedback;

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
public class UserFeedbackEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeedbackEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="api">The injected user feedback API.</param>
    public UserFeedbackEffects(ILogger<UserFeedbackEffects> logger, IUserFeedbackApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<UserFeedbackEffects> Logger { get; set; }

    [Inject]
    private IUserFeedbackApi Api { get; set; }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod(typeof(UserFeedbackActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading user feedback");

        ApiResponse<RequestResult<IEnumerable<UserFeedbackView>>> response = await this.Api.GetAll().ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("User feedback loaded successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error loading user feedback, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.LoadFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleUpdateAction(UserFeedbackActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating user feedback");

        ApiResponse<RequestResult<UserFeedbackView>> response = await this.Api.Update(action.UserFeedbackView).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("User feedback updated successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.UpdateSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error updating user feedback, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.UpdateFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleAssociateTagsAction(UserFeedbackActions.AssociateTagsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Associating tags with user feedback!");

        ApiResponse<RequestResult<UserFeedbackView>> response = await this.Api.AssociateTags(action.TagIds, action.FeedbackId).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Tags associated to user feedback successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.AssociateTagsSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error associating tags to user feedback, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.AssociateTagsFailAction(error));
    }
}
