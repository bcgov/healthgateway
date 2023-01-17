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
using System.Collections.Immutable;
using Fluxor;
using HealthGateway.Admin.Common.Models;

#pragma warning disable CS1591, SA1600
public static class TagReducers
{
    [ReducerMethod(typeof(TagActions.LoadAction))]
    public static TagState ReduceLoadAction(TagState state)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static TagState ReduceLoadSuccessAction(TagState state, TagActions.LoadSuccessAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = action.Data.ResourcePayload.ToImmutableDictionary(tag => tag.Id),
        };
    }

    [ReducerMethod]
    public static TagState ReduceLoadFailAction(TagState state, TagActions.LoadFailAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(TagActions.AddAction))]
    public static TagState ReduceAddAction(TagState state)
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
    public static TagState ReduceAddSuccessAction(TagState state, TagActions.AddSuccessAction action)
    {
        IImmutableDictionary<Guid, AdminTagView> data = state.Data ?? new Dictionary<Guid, AdminTagView>().ToImmutableDictionary();

        AdminTagView? tag = action.Data.ResourcePayload;
        if (tag != null)
        {
            data = data.Remove(tag.Id).Add(tag.Id, tag);
        }

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
    public static TagState ReduceAddFailAction(TagState state, TagActions.AddFailAction action)
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

    [ReducerMethod(typeof(TagActions.DeleteAction))]
    public static TagState ReduceDeleteAction(TagState state)
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
    public static TagState ReduceDeleteSuccessAction(TagState state, TagActions.DeleteSuccessAction action)
    {
        IImmutableDictionary<Guid, AdminTagView> data = state.Data ?? new Dictionary<Guid, AdminTagView>().ToImmutableDictionary();

        AdminTagView? tag = action.Data.ResourcePayload;
        if (tag != null)
        {
            data = data.Remove(tag.Id);
        }

        return state with
        {
            Delete = state.Delete with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = data,
        };
    }

    [ReducerMethod]
    public static TagState ReduceDeleteFailAction(TagState state, TagActions.DeleteFailAction action)
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

    [ReducerMethod(typeof(TagActions.ResetStateAction))]
    public static TagState ReduceResetStateAction(TagState state)
    {
        return state with
        {
            Add = new(),
            Load = new(),
            Delete = new(),
            Data = null,
        };
    }
}
