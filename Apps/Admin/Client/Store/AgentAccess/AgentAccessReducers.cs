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
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Fluxor;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Accessed only by Fluxor")]
public static class AgentAccessReducers
{
    [ReducerMethod(typeof(AgentAccessActions.SearchAction))]
    public static AgentAccessState ReduceSearchAction(AgentAccessState state)
    {
        return state with
        {
            Search = state.Search with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceSearchSuccessAction(AgentAccessState state, AgentAccessActions.SearchSuccessAction action)
    {
        return state with
        {
            Search = state.Search with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = action.Data.ToImmutableDictionary(agent => agent.Id),
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceSearchFailAction(AgentAccessState state, AgentAccessActions.SearchFailAction action)
    {
        return state with
        {
            Search = state.Search with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.AddAction))]
    public static AgentAccessState ReduceAddAction(AgentAccessState state)
    {
        return state with
        {
            Add = state.Add with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceAddSuccessAction(AgentAccessState state, AgentAccessActions.AddSuccessAction action)
    {
        IImmutableDictionary<Guid, AdminAgent> data = state.Data ?? new Dictionary<Guid, AdminAgent>().ToImmutableDictionary();

        AdminAgent agent = action.Data;
        data = data.Remove(agent.Id).Add(agent.Id, agent);

        return state with
        {
            Add = state.Add with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = data,
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceAddFailAction(AgentAccessState state, AgentAccessActions.AddFailAction action)
    {
        return state with
        {
            Add = state.Add with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.UpdateAction))]
    public static AgentAccessState ReduceUpdateAction(AgentAccessState state)
    {
        return state with
        {
            Update = state.Update with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceUpdateSuccessAction(AgentAccessState state, AgentAccessActions.UpdateSuccessAction action)
    {
        IImmutableDictionary<Guid, AdminAgent> data = state.Data ?? new Dictionary<Guid, AdminAgent>().ToImmutableDictionary();

        AdminAgent agent = action.Data;
        data = data.Remove(agent.Id).Add(agent.Id, agent);

        return state with
        {
            Update = state.Update with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = data,
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceUpdateFailAction(AgentAccessState state, AgentAccessActions.UpdateFailAction action)
    {
        return state with
        {
            Update = state.Update with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.DeleteAction))]
    public static AgentAccessState ReduceDeleteAction(AgentAccessState state)
    {
        return state with
        {
            Delete = state.Delete with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceDeleteSuccessAction(AgentAccessState state, AgentAccessActions.DeleteSuccessAction action)
    {
        IImmutableDictionary<Guid, AdminAgent> data = state.Data ?? new Dictionary<Guid, AdminAgent>().ToImmutableDictionary();

        Guid id = action.Data;
        data = data.Remove(id);

        return state with
        {
            Delete = state.Delete with
            {
                IsLoading = false,
                Error = null,
            },
            Data = data,
        };
    }

    [ReducerMethod]
    public static AgentAccessState ReduceDeleteFailAction(AgentAccessState state, AgentAccessActions.DeleteFailAction action)
    {
        return state with
        {
            Delete = state.Delete with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.ClearAddErrorAction))]
    public static AgentAccessState ReduceClearAddErrorAction(AgentAccessState state)
    {
        return state with
        {
            Add = state.Add with
            {
                Error = null,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.ClearUpdateErrorAction))]
    public static AgentAccessState ReduceClearUpdateErrorAction(AgentAccessState state)
    {
        return state with
        {
            Update = state.Update with
            {
                Error = null,
            },
        };
    }

    [ReducerMethod(typeof(AgentAccessActions.ResetStateAction))]
    public static AgentAccessState ReduceResetStateAction(AgentAccessState state)
    {
        return state with
        {
            Add = new(),
            Search = new(),
            Update = new(),
            Delete = new(),
            Data = null,
        };
    }
}
