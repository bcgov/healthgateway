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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.Constants;
using HealthGateway.Common.Data.Models;
using Microsoft.Extensions.Logging;
using Refit;

public class CommunicationsEffects(ILogger<CommunicationsEffects> logger, ICommunicationsApi api)
{
    [EffectMethod]
    public async Task HandleAddAction(CommunicationsActions.AddAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Adding communication");

        try
        {
            RequestResult<Communication> response = await api.AddAsync(action.Communication);
            if (response.ResultStatus == ResultType.Success)
            {
                logger.LogInformation("Communication added successfully!");
                dispatcher.Dispatch(new CommunicationsActions.AddSuccessAction { Data = response });
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                logger.LogError("Error adding communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.AddFailureAction { Error = error });
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error adding communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.AddFailureAction { Error = error });
        }
    }

    [EffectMethod(typeof(CommunicationsActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading communications");

        try
        {
            RequestResult<IEnumerable<Communication>> response = await api.GetAllAsync();
            if (response.ResultStatus == ResultType.Success)
            {
                logger.LogInformation("Communications loaded successfully!");
                dispatcher.Dispatch(new CommunicationsActions.LoadSuccessAction { Data = response });
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                logger.LogError("Error loading communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.LoadFailureAction { Error = error });
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error loading communications, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.LoadFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(CommunicationsActions.UpdateAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Updating communication");

        try
        {
            RequestResult<Communication> response = await api.UpdateAsync(action.Communication);
            if (response.ResultStatus == ResultType.Success)
            {
                logger.LogInformation("Communication updated successfully!");
                dispatcher.Dispatch(new CommunicationsActions.UpdateSuccessAction { Data = response });
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                logger.LogError("Error updating communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.UpdateFailureAction { Error = error });
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error updating communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.UpdateFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(CommunicationsActions.DeleteAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Deleting communication");

        try
        {
            RequestResult<Communication> response = await api.DeleteAsync(action.Communication);
            if (response.ResultStatus == ResultType.Success)
            {
                logger.LogInformation("Communication deleted successfully!");
                dispatcher.Dispatch(new CommunicationsActions.DeleteSuccessAction { Data = response });
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                logger.LogError("Error deleting communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.DeleteFailureAction { Error = error });
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error deleting communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.DeleteFailureAction { Error = error });
        }
    }
}
