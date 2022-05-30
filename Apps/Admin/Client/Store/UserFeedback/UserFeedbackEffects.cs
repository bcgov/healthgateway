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
public class UserFeedbackViewEffects
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserFeedbackViewEffects"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    /// <param name="api">The injected API.</param>
    public UserFeedbackViewEffects(ILogger<UserFeedbackViewEffects> logger, IUserFeedbackApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<UserFeedbackViewEffects> Logger { get; set; }

    [Inject]
    private IUserFeedbackApi Api { get; set; }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleAssociateTagAction(UserFeedbackActions.AssociateTagAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Associating tag with feedback!");

        ApiResponse<RequestResult<UserFeedbackTagView>> response = await this.Api.AssociateTag(action.AdminTag, action.FeedbackId).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Tag associated to user feedback successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.AssociateTagSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error associating tag to user feedback, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.AssociateTagFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="action">The triggering action.</param>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod]
    public async Task HandleDissociateTagAction(UserFeedbackActions.DissociateTagAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Dissociating tag with feedback!");

        ApiResponse<PrimitiveRequestResult<bool>> response = await this.Api.DissociateTag(action.FeedbackTag, action.FeedbackId).ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("Tag was disassociated from user feedback successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.DissociateTagSuccessAction(response.Content, action.FeedbackTag, action.FeedbackId));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error dissociating tag from user feedback, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.AssociateTagFailAction(error));
    }

    /// <summary>
    /// Handler that calls the service and dispatches resulting actions.
    /// </summary>
    /// <param name="dispatcher">The injected dispatcher.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [EffectMethod(typeof(UserFeedbackActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading UserFeedbackViews");

        ApiResponse<RequestResult<IEnumerable<UserFeedbackView>>> response = await this.Api.GetAll().ConfigureAwait(true);
        if (response.IsSuccessStatusCode && response.Content != null && response.Content.ResultStatus == ResultType.Success)
        {
            this.Logger.LogInformation("User feedback views loaded successfully!");
            dispatcher.Dispatch(new UserFeedbackActions.LoadSuccessAction(response.Content));
            return;
        }

        RequestError error = StoreUtility.FormatRequestError(response.Error, response.Content?.ResultError);
        this.Logger.LogError("Error loading User feedback views, reason: {ErrorMessage}", error.Message);
        dispatcher.Dispatch(new UserFeedbackActions.LoadFailAction(error));
    }
}
