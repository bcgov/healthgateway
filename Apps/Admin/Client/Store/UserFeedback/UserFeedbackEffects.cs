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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.ViewModels;
using Microsoft.Extensions.Logging;
using Refit;

public class UserFeedbackEffects
{
    public UserFeedbackEffects(ILogger<UserFeedbackEffects> logger, IUserFeedbackApi api, IMapper autoMapper)
    {
        this.Logger = logger;
        this.Api = api;
        this.AutoMapper = autoMapper;
    }

    private ILogger<UserFeedbackEffects> Logger { get; }

    private IUserFeedbackApi Api { get; }

    private IMapper AutoMapper { get; }

    [EffectMethod(typeof(UserFeedbackActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading user feedback");

        try
        {
            RequestResult<IEnumerable<UserFeedbackView>> response = await this.Api.GetAllAsync().ConfigureAwait(true);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                this.Logger.LogInformation("User feedback loaded successfully!");
                dispatcher.Dispatch(
                    new UserFeedbackActions.LoadSuccessAction
                    {
                        Data = this.AutoMapper.Map<IEnumerable<UserFeedbackView>, IEnumerable<ExtendedUserFeedbackView>>(response.ResourcePayload),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            this.Logger.LogError("Error loading user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.LoadFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error loading user feedback, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new UserFeedbackActions.LoadFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(UserFeedbackActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating user feedback");

        try
        {
            RequestResult<UserFeedbackView> response = await this.Api.UpdateAsync(action.UserFeedbackView).ConfigureAwait(true);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                this.Logger.LogInformation("User feedback updated successfully!");
                dispatcher.Dispatch(
                    new UserFeedbackActions.UpdateSuccessAction
                    {
                        Data = this.AutoMapper.Map<UserFeedbackView, ExtendedUserFeedbackView>(response.ResourcePayload),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            this.Logger.LogError("Error updating user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.UpdateFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error updating user feedback, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new UserFeedbackActions.UpdateFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleSaveAssociatedTagsAction(UserFeedbackActions.SaveAssociatedTagsAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Associating tags with user feedback!");

        try
        {
            RequestResult<UserFeedbackView> response = await this.Api.AssociateTagsAsync(action.TagIds, action.FeedbackId).ConfigureAwait(true);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                this.Logger.LogInformation("Tags associated to user feedback successfully!");

                dispatcher.Dispatch(
                    new UserFeedbackActions.SaveAssociatedTagsSuccessAction
                    {
                        Data = this.AutoMapper.Map<UserFeedbackView, ExtendedUserFeedbackView>(response.ResourcePayload),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            this.Logger.LogError("Error associating tags to user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.SaveAssociatedTagsFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error associating tags to user feedback, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new UserFeedbackActions.SaveAssociatedTagsFailureAction { Error = error });
        }
    }
}
