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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Services;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using Microsoft.Extensions.Logging;
using Refit;

public class UserFeedbackEffects(ILogger<UserFeedbackEffects> logger, IUserFeedbackApi api, IAdminClientMappingService mappingService)
{
    [EffectMethod(typeof(UserFeedbackActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading user feedback");

        try
        {
            RequestResult<IEnumerable<UserFeedbackView>> response = await api.GetAllAsync();
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("User feedback loaded successfully!");
                dispatcher.Dispatch(
                    new UserFeedbackActions.LoadSuccessAction
                    {
                        Data = response.ResourcePayload.Select(mappingService.MapToExtendedUserFeedbackView).ToList(),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error loading user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.LoadFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error loading user feedback, reason: {Message}", e.Message);
            dispatcher.Dispatch(new UserFeedbackActions.LoadFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(UserFeedbackActions.UpdateAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Updating user feedback");

        try
        {
            RequestResult<UserFeedbackView> response = await api.UpdateAsync(action.UserFeedbackView);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("User feedback updated successfully!");
                dispatcher.Dispatch(
                    new UserFeedbackActions.UpdateSuccessAction
                    {
                        Data = mappingService.MapToExtendedUserFeedbackView(response.ResourcePayload),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error updating user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.UpdateFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error updating user feedback, reason: {Message}", e.Message);
            dispatcher.Dispatch(new UserFeedbackActions.UpdateFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleSaveAssociatedTagsAction(UserFeedbackActions.SaveAssociatedTagsAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Associating tags with user feedback!");

        try
        {
            RequestResult<UserFeedbackView> response = await api.AssociateTagsAsync(action.TagIds, action.FeedbackId);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("Tags associated to user feedback successfully!");

                dispatcher.Dispatch(
                    new UserFeedbackActions.SaveAssociatedTagsSuccessAction
                    {
                        Data = mappingService.MapToExtendedUserFeedbackView(response.ResourcePayload),
                    });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error associating tags to user feedback, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new UserFeedbackActions.SaveAssociatedTagsFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error associating tags to user feedback, reason: {Message}", e.Message);
            dispatcher.Dispatch(new UserFeedbackActions.SaveAssociatedTagsFailureAction { Error = error });
        }
    }
}
