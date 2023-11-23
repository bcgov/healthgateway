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
namespace HealthGateway.Admin.Client.Store.AgentAccess;

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

public class AgentAccessEffects(ILogger<AgentAccessEffects> logger, IAgentAccessApi api)
{
    [EffectMethod]
    public async Task HandleAddAction(AgentAccessActions.AddAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Adding agent");
        try
        {
            AdminAgent response = await api.ProvisionAgentAccessAsync(action.Agent).ConfigureAwait(true);
            logger.LogInformation("Agent added successfully");
            dispatcher.Dispatch(new AgentAccessActions.AddSuccessAction { Data = response });
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.Conflict)
        {
            RequestError error = new() { Message = "User already exists" };
            logger.LogInformation("Agent already exists");
            dispatcher.Dispatch(new AgentAccessActions.AddFailureAction { Error = error });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error adding agent, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.AddFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleSearchAction(AgentAccessActions.SearchAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Retrieving agents");
        try
        {
            IEnumerable<AdminAgent> response = await api.GetAgentsAsync(action.Query).ConfigureAwait(true);
            logger.LogInformation("Agents retrieved successfully");
            dispatcher.Dispatch(new AgentAccessActions.SearchSuccessAction { Data = response });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError(e, "Error retrieving agents, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.SearchFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(AgentAccessActions.UpdateAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Updating agent access");
        try
        {
            AdminAgent agent = await api.UpdateAgentAccessAsync(action.Agent).ConfigureAwait(true);
            logger.LogInformation("Agent access updated successfully");
            dispatcher.Dispatch(new AgentAccessActions.UpdateSuccessAction { Data = agent });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error updating agent access, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.UpdateFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(AgentAccessActions.DeleteAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Removing agent access");
        try
        {
            await api.RemoveAgentAccessAsync(action.Id).ConfigureAwait(true);
            logger.LogInformation("Agent access removed successfully");
            dispatcher.Dispatch(new AgentAccessActions.DeleteSuccessAction { Data = action.Id });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error removing agent access, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.DeleteFailureAction { Error = error });
        }
    }
}
