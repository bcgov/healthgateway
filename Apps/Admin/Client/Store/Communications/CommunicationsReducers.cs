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

using Fluxor;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
public static class CommunicationsReducers
{
    /// <summary>
    /// The reducer for loading communications.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <returns>The new state.</returns>
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

    /// <summary>
    /// The reducer for the load success action.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new state.</returns>
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
        };
    }

    /// <summary>
    /// The reducer for the fail action.
    /// </summary>
    /// <param name="state">The communications state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new state.</returns>
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
        };
    }
}
