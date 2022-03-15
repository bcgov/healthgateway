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
    /// The Reducer for the user profile load success action.
    /// </summary>
    /// <param name="state">The user profile state.</param>
    /// <param name="action">The user profile load success action.</param>
    /// <returns>The new user profile state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessUserProfilesAction(AnalyticsState state, AnalyticsActions.LoadSuccessUserProfilesAction action)
    {
        return state with
        {
           UserProfilesReport = state.UserProfilesReport with { Data = action.State , IsLoading = false },
        };
    }

    /// <summary>
    /// The Reducer for the user profile load fail action.
    /// </summary>
    /// <param name="state">The user profile state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new user profile state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailUserProfileAction(AnalyticsState state, AnalyticsActions.LoadFailUserProfileAction action)
    {
        return state with
        {
            UserProfilesReport = state.UserProfilesReport with { Data = null, IsLoading = false, RequestError = action.Error },
        };
    }

    /// <summary>
    /// The Reducer for the reset user profile state action.
    /// </summary>
    /// <param name="state">The user profile state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetUserProfilesStateAction))]
    public static AnalyticsState ReduceResetUserProfileStateAction(AnalyticsState state)
    {
        return state with
        {
            UserProfilesReport = state.UserProfilesReport with { Data = null, IsLoading = false, RequestError = null },
        };
    }

    /// <summary>
    /// The Reducer for the comments load success action.
    /// </summary>
    /// <param name="state">The comments state.</param>
    /// <param name="action">The comments load success action.</param>
    /// <returns>The new comments state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessCommentsAction(AnalyticsState state, AnalyticsActions.LoadSuccessCommentsAction action)
    {
        return state with
        {
            CommentsReport = state.CommentsReport with { Data = action.State, IsLoading = false },
        };
    }

    /// <summary>
    /// The Reducer for the comments load fail action.
    /// </summary>
    /// <param name="state">The comments state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new comments state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailCommentsAction(AnalyticsState state, AnalyticsActions.LoadFailCommentsAction action)
    {
        return state with
        {
            CommentsReport = state.CommentsReport with { Data = null, IsLoading = false , RequestError = action.Error },
        };
    }

    /// <summary>
    /// The Reducer for the reset comments state action.
    /// </summary>
    /// <param name="state">The comments state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetCommentsStateAction))]
    public static AnalyticsState ReduceResetCommentsStateAction(AnalyticsState state)
    {
        return state with
        {
            CommentsReport = state.CommentsReport with { Data = null, IsLoading = false, RequestError = null },
        };
    }

    /// <summary>
    /// The Reducer for the notes load success action.
    /// </summary>
    /// <param name="state">The notes state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new notes state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessNotesAction(AnalyticsState state, AnalyticsActions.LoadSuccessNotesAction action)
    {
        return state with
        {
            NotesReport = state.NotesReport with { Data = action.State, IsLoading = false },
        };
    }

    /// <summary>
    /// The Reducer for the notes load fail action.
    /// </summary>
    /// <param name="state">The notes state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new notes state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailNotesAction(AnalyticsState state, AnalyticsActions.LoadFailNotesAction action)
    {
        return state with
        {
            NotesReport = state.NotesReport with { Data = null, IsLoading = false, RequestError = action.Error },
        };
    }

    /// <summary>
    /// The Reducer for the reset notes state action.
    /// </summary>
    /// <param name="state">The notes state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetNotesStateAction))]
    public static AnalyticsState ReduceResetNotesStateAction(AnalyticsState state)
    {
        return state with
        {
            NotesReport = state.NotesReport with { Data = null, IsLoading = false, RequestError = null },
        };
    }

    /// <summary>
    /// The Reducer for the rating load success action.
    /// </summary>
    /// <param name="state">The ratings state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new rating state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessRatingsAction(AnalyticsState state, AnalyticsActions.LoadSuccessRatingsAction action)
    {
        return state with
        {
            RatingsReport = state.RatingsReport with { Data = action.State, IsLoading = false },
        };
    }

    /// <summary>
    /// The Reducer for the rating load fail action.
    /// </summary>
    /// <param name="state">The rating state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new rating state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailRatingsAction(AnalyticsState state, AnalyticsActions.LoadFailRatingsAction action)
    {
        return state with
        {
            RatingsReport = state.RatingsReport with { Data = null, IsLoading = false, RequestError = action.Error},
        };
    }

    /// <summary>
    /// The Reducer for the reset ratings state action.
    /// </summary>
    /// <param name="state">The ratings state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetRatingsStateAction))]
    public static AnalyticsState ReduceResetRatingsStateAction(AnalyticsState state)
    {
        return state with
        {
            RatingsReport = state.RatingsReport with { Data = null, IsLoading = false, RequestError = null },
        };
    }

    /// <summary>
    /// The Reducer for the inactive users load success action.
    /// </summary>
    /// <param name="state">The inactive users state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new inactive users state.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessInactiveUsersAction(AnalyticsState state, AnalyticsActions.LoadSuccessInactiveUsersAction action)
    {
        return state with
        {
            InactiveUsersReport = state.InactiveUsersReport with { Data = action.State, IsLoading = false, RequestError = null },
        };
    }

    /// <summary>
    /// The Reducer for the inactive users load fail action.
    /// </summary>
    /// <param name="state">The inactive users state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new inactive usersstate.</returns>
    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailInactiveUsersAction(AnalyticsState state, AnalyticsActions.LoadFailInactiveUsersAction action)
    {
        return state with
        {
            InactiveUsersReport = state.InactiveUsersReport with { Data = null, IsLoading = false, RequestError = action.Error },
        };
    }

    /// <summary>
    /// The Reducer for the reset inactive users state action.
    /// </summary>
    /// <param name="state">The inactive users state.</param>
    /// <returns>The empty state.</returns>
    [ReducerMethod(typeof(AnalyticsActions.ResetInactiveUsersStateAction))]
    public static AnalyticsState ReduceResetInactiveUsersStateAction(AnalyticsState state)
    {
        return state with
        {
            InactiveUsersReport = state.InactiveUsersReport with { Data = null, IsLoading = false, RequestError = null },
        };
    }
}
