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
namespace HealthGateway.Admin.Client.Store.Analytics;

using Fluxor;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
public static class AnalyticsReducers
{
    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The user profiles state.</param>
    /// <returns>The new user profiles state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.LoadUserProfilesAction))]
    public static AnalyticsState ReduceLoadUserProfilesAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The comments state.</param>
    /// <returns>The new comments state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.LoadCommentsAction))]
    public static AnalyticsState ReduceLoadCommentsAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The notes state.</param>
    /// <returns>The new notes state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.LoadNotesAction))]
    public static AnalyticsState ReduceLoadNotesAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The ratings state.</param>
    /// <returns>The new ratings state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.LoadRatingsAction))]
    public static AnalyticsState ReduceLoadRatingsAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The inactive users state.</param>
    /// <returns>The new inactive users state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.LoadInactiveUsersAction))]
    public static AnalyticsState ReduceLoadInactiveUsersAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The analytics state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new analytics state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessAction(AnalyticsState state, AnalyticsActions.LoadSuccessAction action)
    {
        return state with
        {
            Data = action.State,
            IsLoading = false,
            RequestError = null,
        };
    }

    /// <summary>
    /// The Reducer for the analytics load fail action.
    /// </summary>
    /// <param name="state">The analytics state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new analytics state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailAction(AnalyticsState state, AnalyticsActions.LoadFailAction action)
    {
        return state with
        {
            Data = null,
            IsLoading = false,
            RequestError = action.Error,
        };
    }

    /// <summary>
    /// The Reducer for the reset inactive users state action.
    /// </summary>
    /// <param name="state">The inactive users state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetStateAction))]
    public static AnalyticsState ReduceResetAction(AnalyticsState state)
    {
        return state with
        {
            Data = null,
            IsLoading = false,
            RequestError = null,
        };
    }
}
