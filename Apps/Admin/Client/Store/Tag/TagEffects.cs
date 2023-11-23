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

namespace HealthGateway.Admin.Client.Store.Tag;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using HealthGateway.Admin.Client.Api;
using HealthGateway.Admin.Client.Utils;
using HealthGateway.Admin.Common.Models;
using HealthGateway.Common.Data.ViewModels;
using Microsoft.Extensions.Logging;
using Refit;

public class TagEffects(ILogger<TagEffects> logger, ITagApi api)
{
    [EffectMethod]
    public async Task HandleAddAction(TagActions.AddAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Adding tag");

        try
        {
            RequestResult<AdminTagView> response = await api.AddAsync(action.TagName).ConfigureAwait(true);
            logger.LogInformation("AdminTagView added successfully!");
            dispatcher.Dispatch(new TagActions.AddSuccessAction { Data = response });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error adding tag, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new TagActions.AddFailureAction { Error = error });
        }
    }

    [EffectMethod(typeof(TagActions.LoadAction))]
    public async Task HandleLoadAction(IDispatcher dispatcher)
    {
        logger.LogInformation("Loading Tag");

        try
        {
            RequestResult<IEnumerable<AdminTagView>> response = await api.GetAllAsync().ConfigureAwait(true);
            logger.LogInformation("Tag loaded successfully!");
            dispatcher.Dispatch(new TagActions.LoadSuccessAction { Data = response });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error loading Tag, reason: {Exception}", e.ToString());
            dispatcher.Dispatch(new TagActions.LoadFailureAction { Error = error });
        }
    }

    [EffectMethod]
    public async Task HandleDeleteAction(TagActions.DeleteAction action, IDispatcher dispatcher)
    {
        logger.LogInformation("Deleting tag");

        try
        {
            RequestResult<AdminTagView> response = await api.DeleteAsync(action.AdminTagView).ConfigureAwait(true);
            logger.LogInformation("Tag deleted successfully!");
            dispatcher.Dispatch(new TagActions.DeleteSuccessAction { Data = response });
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            RequestError error = StoreUtility.FormatRequestError(e);
            logger.LogError("Error deleting tag, reason: {ErrorMessage}", e.ToString());
            dispatcher.Dispatch(new TagActions.DeleteFailureAction { Error = error });
        }
    }
}
