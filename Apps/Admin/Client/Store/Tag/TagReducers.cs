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
using Fluxor;
using HealthGateway.Admin.Common.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
public static class TagReducers
{
    /// <summary>
    /// The reducer for loading Tag.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the load success action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the load fail action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for adding Tag.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the add success action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <param name="action">The add success action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the add fail action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <param name="action">The add fail action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for deleting Tag.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the delete success action.
    /// </summary>
    /// <param name="state">The tag state.</param>
    /// <param name="action">The delete success action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the delete fail action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <param name="action">The delete fail action.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the reset state action.
    /// </summary>
    /// <param name="state">The Tag state.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod(typeof(TagActions.ResetStateAction))]
    public static TagState ReduceResetStateAction(TagState state)
    {
        return state with
        {
            Add = new(),
            Load = new(),
            Delete = new(),
        };
    }
}
