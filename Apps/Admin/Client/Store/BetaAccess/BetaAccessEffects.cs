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
namespace HealthGateway.Admin.Client.Store.BetaAccess;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using Microsoft.Extensions.Logging;
using Refit;

public class BetaAccessEffects(ILogger<BetaAccessEffects> logger, IBetaFeatureApi api)
{
    [EffectMethod]
    public async Task HandleSetUserAccess(BetaAccessActions.SetUserAccessAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Setting user access");
        try
        {
            await api.SetUserAccessAsync(action.Request);
            logger.LogInformation("User access set successfully");
            dispatcher.Dispatch(new BetaAccessActions.SetUserAccessSuccessAction { Request = action.Request });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error setting user access, reason: {Message}", e.Message);
            dispatcher.Dispatch(new BetaAccessActions.SetUserAccessFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleGetUserAccessAction(BetaAccessActions.GetUserAccessAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving user access");
        try
        {
            UserBetaAccess response = await api.GetUserAccessAsync(action.Email);
            logger.LogInformation("User access retrieved successfully");
            dispatcher.Dispatch(new BetaAccessActions.GetUserAccessSuccessAction { Data = response });
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            RequestError error = new() { Message = $"No users found matching the supplied email address ({action.Email})" };
            logger.LogError(e, "No users found matching the supplied email address");
            dispatcher.Dispatch(new BetaAccessActions.GetUserAccessFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error retrieving user access, reason: {Message}", e.Message);
            dispatcher.Dispatch(new BetaAccessActions.GetUserAccessFailureAction { Error = error });
        }
    }

    [EffectMethod(typeof(BetaAccessActions.GetAllUserAccessAction))]
    public async Task HandleGetAllUserAccessAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving all user access");
        try
        {
            IEnumerable<UserBetaAccess> response = await api.GetAllUserAccessAsync();
            logger.LogInformation("All user access retrieved successfully");
            dispatcher.Dispatch(new BetaAccessActions.GetAllUserAccessSuccessAction { Data = response });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error retrieving all user access, reason: {Message}", e.Message);
            dispatcher.Dispatch(new BetaAccessActions.GetAllUserAccessFailureAction { Error = error });
        }
    }
}
