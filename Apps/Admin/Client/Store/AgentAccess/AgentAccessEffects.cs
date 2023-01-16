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

#pragma warning disable CS1591
namespace HealthGateway.Admin.Client.Store.AgentAccess;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Accessed only by Fluxor")]
public class AgentAccessEffects
{
    public AgentAccessEffects(ILogger<AgentAccessEffects> logger, IAgentAccessApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<AgentAccessEffects> Logger { get; set; }

    [Inject]
    private IAgentAccessApi Api { get; set; }

    [EffectMethod]
    public async Task HandleAddAction(AgentAccessActions.AddAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Adding agent");
        try
        {
            AdminAgent response = await this.Api.ProvisionAgentAccessAsync(action.Agent).ConfigureAwait(true);
            this.Logger.LogInformation("Agent added successfully");
            dispatcher.Dispatch(new AgentAccessActions.AddSuccessAction(response));
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.Conflict)
        {
            RequestError error = new() { Message = "User already exists" };
            this.Logger.LogInformation("Agent already exists");
            dispatcher.Dispatch(new AgentAccessActions.AddFailAction(error));
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error adding agent, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.AddFailAction(error));
        }
    }

    [EffectMethod]
    public async Task HandleSearchAction(AgentAccessActions.SearchAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Retrieving agents");
        try
        {
            IEnumerable<AdminAgent> response = await this.Api.GetAgentsAsync(action.Query).ConfigureAwait(true);
            this.Logger.LogInformation("Agents retrieved successfully");
            dispatcher.Dispatch(new AgentAccessActions.SearchSuccessAction(response));
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError(e, "Error retrieving agents, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.SearchFailAction(error));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(AgentAccessActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating agent access");
        try
        {
            AdminAgent agent = await this.Api.UpdateAgentAccessAsync(action.Agent).ConfigureAwait(true);
            this.Logger.LogInformation("Agent access updated successfully");
            dispatcher.Dispatch(new AgentAccessActions.UpdateSuccessAction(agent));
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error updating agent access, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.UpdateFailAction(error));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(AgentAccessActions.DeleteAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Removing agent access");
        try
        {
            await this.Api.RemoveAgentAccessAsync(action.Id).ConfigureAwait(true);
            this.Logger.LogInformation("Agent access removed successfully");
            dispatcher.Dispatch(new AgentAccessActions.DeleteSuccessAction(action.Id));
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error removing agent access, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new AgentAccessActions.DeleteFailAction(error));
        }
    }
}
