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

using System.Collections.Generic;
using System.Linq;
using Fluxor;
using HealthGateway.Admin.Client.Models;
using HealthGateway.Admin.Common.Models;

#pragma warning disable CS1591, SA1600
public static class CommunicationsReducers
{
    [ReducerMethod(typeof(CommunicationsActions.LoadAction))]
    public static CommunicationsState ReduceLoadAction(CommunicationsState state)
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
    public static CommunicationsState ReduceLoadSuccessAction(CommunicationsState state, CommunicationsActions.LoadSuccessAction action)
    {
        return state with
        {
            Load = state.Load with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            Data = action.Data.ResourcePayload?.Select(c => new ExtendedCommunication(c)).ToList(),
        };
    }

    [ReducerMethod]
    public static CommunicationsState ReduceLoadFailAction(CommunicationsState state, CommunicationsActions.LoadFailAction action)
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

    [ReducerMethod(typeof(CommunicationsActions.AddAction))]
    public static CommunicationsState ReduceAddAction(CommunicationsState state)
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
    public static CommunicationsState ReduceAddSuccessAction(CommunicationsState state, CommunicationsActions.AddSuccessAction action)
    {
        IList<ExtendedCommunication> data = state.Data ?? new List<ExtendedCommunication>();

        Communication? communication = action.Data.ResourcePayload;
        if (communication != null)
        {
            data = new List<ExtendedCommunication>(data)
            {
                new(communication),
            };
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
    /// <param name="state">The communications state.</param>
    /// <param name="action">The add fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceAddFailAction(CommunicationsState state, CommunicationsActions.AddFailAction action)
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
    /// The reducer for updating communications.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod(typeof(CommunicationsActions.UpdateAction))]
    public static CommunicationsState ReduceUpdateAction(CommunicationsState state)
    {
        return state with
        {
            Update = state.Update with
            {
                IsLoading = true,
            },
        };
    }

    /// <summary>
    /// The reducer for the update success action.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <param name="action">The update success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceUpdateSuccessAction(CommunicationsState state, CommunicationsActions.UpdateSuccessAction action)
    {
        IList<ExtendedCommunication> data = state.Data ?? new List<ExtendedCommunication>();

        Communication? communication = action.Data.ResourcePayload;
        if (communication != null)
        {
            ExtendedCommunication? existingCommunication = data.SingleOrDefault(c => c.Id == communication.Id);
            if (existingCommunication != null)
            {
                existingCommunication.PopulateFromModel(communication);
            }
            else
            {
                data.Add(new ExtendedCommunication(communication));
            }
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

    /// <summary>
    /// The reducer for the update fail action.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <param name="action">The update fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceUpdateFailAction(CommunicationsState state, CommunicationsActions.UpdateFailAction action)
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

    /// <summary>
    /// The reducer for deleting communications.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod(typeof(CommunicationsActions.DeleteAction))]
    public static CommunicationsState ReduceDeleteAction(CommunicationsState state)
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
    /// <param name="state">The communications state.</param>
    /// <param name="action">The delete success action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceDeleteSuccessAction(CommunicationsState state, CommunicationsActions.DeleteSuccessAction action)
    {
        IList<ExtendedCommunication> data = state.Data ?? new List<ExtendedCommunication>();

        Communication? communication = action.Data.ResourcePayload;
        if (communication != null)
        {
            data = data.Where(x => x.Id != communication.Id).ToList();
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
    /// <param name="state">The communications state.</param>
    /// <param name="action">The delete fail action.</param>
    /// <returns>The new state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceDeleteFailAction(CommunicationsState state, CommunicationsActions.DeleteFailAction action)
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
    /// <param name="state">The communications state.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod(typeof(CommunicationsActions.ResetStateAction))]
    public static CommunicationsState ReduceResetStateAction(CommunicationsState state)
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
    /// <param name="state">The communications state.</param>
    /// <param name="action">The ToggleIsExpanded action.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod]
    public static CommunicationsState ReduceToggleIsExpandedAction(CommunicationsState state, CommunicationsActions.ToggleIsExpandedAction action)
    {
        IEnumerable<ExtendedCommunication> data = state.Data ?? Enumerable.Empty<ExtendedCommunication>();

        ExtendedCommunication? communication = data.SingleOrDefault(c => c.Id == action.Id);
        if (communication != null)
        {
            communication.IsExpanded = !communication.IsExpanded;
        }

        return state;
    }
}
