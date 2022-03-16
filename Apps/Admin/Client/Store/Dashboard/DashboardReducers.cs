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
namespace HealthGateway.Admin.Client.Store.Dashboard;

using Fluxor;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
public static class DashboardReducers
{
    /// <summary>
    /// The Reducer for the load registered users action.
    /// </summary>
    /// <param name="state">The users state.</param>
    /// <returns>The new users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadRegisteredUsersAction))]
    public static DashboardUserState ReduceLoadRegisteredUsersAction(DashboardUserState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load logged in users action.
    /// </summary>
    /// <param name="state">The users state.</param>
    /// <returns>The new users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadLoggedInUsersAction))]
    public static DashboardUserState ReduceLoadLoggedInUsersAction(DashboardUserState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load dependents action.
    /// </summary>
    /// <param name="state">The users state.</param>
    /// <returns>The new users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadDependentsAction))]
    public static DashboardUserState ReduceLoadDependentsAction(DashboardUserState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load recurring users action.
    /// </summary>
    /// <param name="state">The recurring users state.</param>
    /// <returns>The new recurring users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadRecurringUsersAction))]
    public static DashboardRecurringUserState ReduceLoadRecurringUsersAction(DashboardRecurringUserState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load rating users action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <returns>The new rating summary  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadRatingSummaryAction))]
    public static DashboardRatingSummaryState ReduceLoadRatingSummaryAction(DashboardRatingSummaryState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new user state.</returns>
    [ReducerMethod]
    public static DashboardUserState ReduceLoadUserSuccessAction(DashboardUserState state, DashboardActions.LoadSuccessUserAction action)
    {
        return state with
        {
            Result = action.Data,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The user state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new user state.</returns>
    [ReducerMethod]
    public static DashboardUserState ReduceLoadUserFailAction(DashboardUserState state, DashboardActions.LoadFailUserAction action)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = action.Error,
        };
    }

    /// <summary>
    /// The Reducer for the load reset action.
    /// </summary>
    /// <param name="state">The user state.</param>
    /// <returns>The new user state.</returns>
    [ReducerMethod(typeof(DashboardActions.ResetUserStateAction))]
    public static DashboardUserState ReduceLoadUserResetAction(DashboardUserState state)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The recurring user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new recurring user state.</returns>
    [ReducerMethod]
    public static DashboardRecurringUserState ReduceLoadRecurringUserSuccessAction(DashboardRecurringUserState state, DashboardActions.LoadSuccessRecurringUserAction action)
    {
        return state with
        {
            Result = action.Data,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The recurring user state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new recurring user state.</returns>
    [ReducerMethod]
    public static DashboardRecurringUserState ReduceLoadRecurringUserFailAction(DashboardRecurringUserState state, DashboardActions.LoadFailRecurringUserAction action)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = action.Error,
        };
    }

    /// <summary>
    /// The Reducer for the load reset action.
    /// </summary>
    /// <param name="state">The recurring user state.</param>
    /// <returns>The new recurring user state.</returns>
    [ReducerMethod(typeof(DashboardActions.ResetRecurringUserStateAction))]
    public static DashboardRecurringUserState ReduceLoadRecurringUserResetAction(DashboardRecurringUserState state)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new rating summary state.</returns>
    [ReducerMethod]
    public static DashboardRatingSummaryState ReduceLoadRatingSummarySuccessAction(DashboardRatingSummaryState state, DashboardActions.LoadSuccessRatingSummaryAction action)
    {
        return state with
        {
            Result = action.Data,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new rating summary state.</returns>
    [ReducerMethod]
    public static DashboardRatingSummaryState ReduceLoadRatingSummaryFailAction(DashboardRatingSummaryState state, DashboardActions.LoadFailRatingSummaryAction action)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = action.Error,
        };
    }

    /// <summary>
    /// The Reducer for the load reset action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <returns>The new rating summary state.</returns>
    [ReducerMethod(typeof(DashboardActions.ResetRatingSummaryStateAction))]
    public static DashboardRatingSummaryState ReduceLoadRatingSummaryResetAction(DashboardRatingSummaryState state)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = null,
        };
    }

    /// <summary>
    /// The Reducer for the dashboard state reset action.
    /// </summary>
    /// <param name="userState">The user state.</param>
    /// <param name="recurringUserState">The recurring user state.</param>
    /// <param name="ratingSummaryState">The rating summary state.</param>
    [ReducerMethod(typeof(DashboardActions.ResetDashboardStateAction))]
    public static void ReduceLoadDashboardResetAction(DashboardUserState userState, DashboardRecurringUserState recurringUserState, DashboardRatingSummaryState ratingSummaryState)
    {
        _ = userState with { Result = null, IsLoading = false, Error = null };
        _ = recurringUserState with { Result = null, IsLoading = false, Error = null };
        _ = ratingSummaryState with { Result = null, IsLoading = false, Error = null };
    }
}
