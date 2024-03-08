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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using Microsoft.Extensions.Logging;
using Refit;

public class BroadcastsEffects(ILogger<BroadcastsEffects> logger, IBroadcastsApi api)
{
    [EffectMethod]
    public async Task HandleAddAction(BroadcastsActions.AddAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Adding broadcast");
        try
        {
            RequestResult<Broadcast> response = await api.AddAsync(action.Broadcast);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("Broadcast added successfully!");
                dispatcher.Dispatch(new BroadcastsActions.AddSuccessAction { Data = response });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error adding broadcast, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new BroadcastsActions.AddFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error adding broadcast, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new BroadcastsActions.AddFailureAction { Error = error });
        }
    }

    [EffectMethod(typeof(BroadcastsActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading broadcasts");
        try
        {
            RequestResult<IEnumerable<Broadcast>> response = await api.GetAllAsync();
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("Broadcasts loaded successfully!");
                dispatcher.Dispatch(new BroadcastsActions.LoadSuccessAction { Data = response });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error loading broadcasts, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new BroadcastsActions.LoadFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error loading broadcasts, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new BroadcastsActions.LoadFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(BroadcastsActions.UpdateAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Updating broadcast");
        try
        {
            RequestResult<Broadcast> response = await api.UpdateAsync(action.Broadcast);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("Broadcast updated successfully!");
                dispatcher.Dispatch(new BroadcastsActions.UpdateSuccessAction { Data = response });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error updating broadcast, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new BroadcastsActions.UpdateFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error updating broadcasts, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new BroadcastsActions.UpdateFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(BroadcastsActions.DeleteAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Deleting broadcast");
        try
        {
            RequestResult<Broadcast> response = await api.DeleteAsync(action.Broadcast);
            if (response is { ResourcePayload: { }, ResultStatus: ResultType.Success })
            {
                logger.LogInformation("Broadcast deleted successfully!");
                dispatcher.Dispatch(new BroadcastsActions.DeleteSuccessAction { Data = response });
                return;
            }

            RequestError error = StoreUtility.FormatRequestError(null, response.ResultError);
            logger.LogError("Error deleting broadcast, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new BroadcastsActions.DeleteFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error deleting broadcast, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new BroadcastsActions.DeleteFailureAction { Error = error });
        }
    }
}
