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
using HealthGateway.Common.Data.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Refit;

#pragma warning disable CS1591, SA1600
public class CommunicationsEffects
{
    public CommunicationsEffects(ILogger<CommunicationsEffects> logger, ICommunicationsApi api)
    {
        this.Logger = logger;
        this.Api = api;
    }

    [Inject]
    private ILogger<CommunicationsEffects> Logger { get; set; }

    [Inject]
    private ICommunicationsApi Api { get; set; }

    [EffectMethod]
    public async Task HandleAddAction(CommunicationsActions.AddAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Adding communication");

        try
        {
            RequestResult<Communication> response = await this.Api.AddAsync(action.Communication).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Success)
            {
                this.Logger.LogInformation("Communication added successfully!");
                dispatcher.Dispatch(new CommunicationsActions.AddSuccessAction(response));
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                this.Logger.LogError("Error adding communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.AddFailAction(error));
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.Logger.LogError("Error adding communication...{Error}", e);
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error adding communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.AddFailAction(error));
        }
    }

    [EffectMethod(typeof(CommunicationsActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Loading communications");

        try
        {
            RequestResult<IEnumerable<Communication>> response = await this.Api.GetAllAsync().ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Success)
            {
                this.Logger.LogInformation("Communications loaded successfully!");
                dispatcher.Dispatch(new CommunicationsActions.LoadSuccessAction(response));
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                this.Logger.LogError("Error loading communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.LoadFailAction(error));
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.Logger.LogError("Error loading communications...{Error}", e);
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error loading communications, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.LoadFailAction(error));
        }
    }

    [EffectMethod]
    public async Task HandleUpdateAction(CommunicationsActions.UpdateAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Updating communication");

        try
        {
            RequestResult<Communication> response = await this.Api.UpdateAsync(action.Communication).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Success)
            {
                this.Logger.LogInformation("Communication updated successfully!");
                dispatcher.Dispatch(new CommunicationsActions.UpdateSuccessAction(response));
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                this.Logger.LogError("Error updating communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.UpdateFailAction(error));
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.Logger.LogError("Error updating communication...{Error}", e);
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error updating communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.UpdateFailAction(error));
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(CommunicationsActions.DeleteAction action, IDispatcher dispatcher)
    {
        this.Logger.LogInformation("Deleting communication");

        try
        {
            RequestResult<Communication> response = await this.Api.DeleteAsync(action.Communication).ConfigureAwait(true);
            if (response.ResultStatus == ResultType.Success)
            {
                this.Logger.LogInformation("Communication deleted successfully!");
                dispatcher.Dispatch(new CommunicationsActions.DeleteSuccessAction(response));
            }
            else
            {
                RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                this.Logger.LogError("Error deleting communication, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new CommunicationsActions.DeleteFailAction(error));
            }
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            this.Logger.LogError("Error deleting communication...{Error}", e);
            RequestError error = StoreUtility.FormatRequestError(e);
            this.Logger.LogError("Error deleting communication, reason: {ErrorMessage}", error.Message);
            dispatcher.Dispatch(new CommunicationsActions.DeleteFailAction(error));
        }
    }
}
