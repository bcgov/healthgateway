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
using System.Collections.Immutable;
using System.Linq;
using Fluxor;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Common.Data.Models;

#pragma warning disable CS1591, SA1600
public static class BroadcastsReducers
{
    [ReducerMethod(typeof(BroadcastsActions.LoadAction))]
    public static BroadcastsState ReduceLoadAction(BroadcastsState state)
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
    public static BroadcastsState ReduceLoadSuccessAction(BroadcastsState state, BroadcastsActions.LoadSuccessAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = action.Data.ResourcePayload.Select(b => new ExtendedBroadcast(b)).ToImmutableDictionary(b => b.Id),
        };
    }

    [ReducerMethod]
    public static BroadcastsState ReduceLoadFailAction(BroadcastsState state, BroadcastsActions.LoadFailAction action)
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

    [ReducerMethod(typeof(BroadcastsActions.AddAction))]
    public static BroadcastsState ReduceAddAction(BroadcastsState state)
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
    public static BroadcastsState ReduceAddSuccessAction(BroadcastsState state, BroadcastsActions.AddSuccessAction action)
    {
        IImmutableDictionary<Guid, ExtendedBroadcast> data = state.Data ?? new Dictionary<Guid, ExtendedBroadcast>().ToImmutableDictionary();

        Broadcast? broadcast = action.Data.ResourcePayload;
        if (broadcast != null)
        {
            data = data.Remove(broadcast.Id).Add(broadcast.Id, new(broadcast));
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
    public static BroadcastsState ReduceAddFailAction(BroadcastsState state, BroadcastsActions.AddFailAction action)
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

    [ReducerMethod(typeof(BroadcastsActions.UpdateAction))]
    public static BroadcastsState ReduceUpdateAction(BroadcastsState state)
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
    public static BroadcastsState ReduceUpdateSuccessAction(BroadcastsState state, BroadcastsActions.UpdateSuccessAction action)
    {
        IImmutableDictionary<Guid, ExtendedBroadcast> data = state.Data ?? new Dictionary<Guid, ExtendedBroadcast>().ToImmutableDictionary();

        Broadcast? broadcast = action.Data.ResourcePayload;
        if (broadcast != null)
        {
            data = data.Remove(broadcast.Id).Add(broadcast.Id, new(broadcast));
        }

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
    public static BroadcastsState ReduceUpdateFailAction(BroadcastsState state, BroadcastsActions.UpdateFailAction action)
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

    [ReducerMethod(typeof(BroadcastsActions.DeleteAction))]
    public static BroadcastsState ReduceDeleteAction(BroadcastsState state)
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
    /// <param name="state">The broadcasts state.</param>
    /// <param name="action">The delete success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static BroadcastsState ReduceDeleteSuccessAction(BroadcastsState state, BroadcastsActions.DeleteSuccessAction action)
    {
        IImmutableDictionary<Guid, ExtendedBroadcast> data = state.Data ?? new Dictionary<Guid, ExtendedBroadcast>().ToImmutableDictionary();

        Broadcast? broadcast = action.Data.ResourcePayload;
        if (broadcast != null)
        {
            data = data.Remove(broadcast.Id);
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

    [ReducerMethod(typeof(BroadcastsActions.ClearAddErrorAction))]
    public static BroadcastsState ReduceClearAddErrorAction(BroadcastsState state)
    {
        return state with
        {
            Add = state.Add with
            {
                Error = null,
            },
        };
    }

    [ReducerMethod(typeof(BroadcastsActions.ClearUpdateErrorAction))]
    public static BroadcastsState ReduceClearUpdateErrorAction(BroadcastsState state)
    {
        return state with
        {
            Update = state.Update with
            {
                Error = null,
            },
        };
    }

    /// <summary>
    /// The reducer for the delete fail action.
    /// </summary>
    /// <param name="state">The broadcasts state.</param>
    /// <param name="action">The delete fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static BroadcastsState ReduceDeleteFailAction(BroadcastsState state, BroadcastsActions.DeleteFailAction action)
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
    /// <param name="state">The broadcasts state.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod(typeof(BroadcastsActions.ResetStateAction))]
    public static BroadcastsState ReduceResetStateAction(BroadcastsState state)
    {
        return state with
        {
            Add = new(),
            Load = new(),
            Update = new(),
            Delete = new(),
            Data = null,
        };
    }

    /// <summary>
    /// The reducer for the ToggleIsExpanded action.
    /// </summary>
    /// <param name="state">The broadcasts state.</param>
    /// <param name="action">The ToggleIsExpanded action.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod]
    public static BroadcastsState ReduceToggleIsExpandedAction(BroadcastsState state, BroadcastsActions.ToggleIsExpandedAction action)
    {
        IImmutableDictionary<Guid, ExtendedBroadcast> data = state.Data ?? new Dictionary<Guid, ExtendedBroadcast>().ToImmutableDictionary();

        ExtendedBroadcast? broadcast = data.GetValueOrDefault(action.Id);
        if (broadcast != null)
        {
            broadcast.IsExpanded = !broadcast.IsExpanded;
        }

        return state;
    }
}
