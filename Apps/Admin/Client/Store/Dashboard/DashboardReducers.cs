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
    public static DashboardState ReduceRegisteredUsersAction(DashboardState state)
    {
        return state with
        {
            RegisteredUsers = state.RegisteredUsers with { IsLoading = true },
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The registered user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new registered user state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceLoadRegisteredUsersSuccessAction(DashboardState state, DashboardActions.RegisteredUsersSuccessAction action)
    {
        return state with
        {
            RegisteredUsers = state.RegisteredUsers with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The registered user state.</param>
    /// <param name="action">The load fail action.</param>
    /// <returns>The new registered user state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceRegisteredUserFailAction(DashboardState state, DashboardActions.RegisteredUsersFailAction action)
    {
        return state with
        {
            RegisteredUsers = state.RegisteredUsers with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load logged in users action.
    /// </summary>
    /// <param name="state">The users state.</param>
    /// <returns>The new users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadLoggedInUsersAction))]
    public static DashboardState ReduceLoggedInUsersAction(DashboardState state)
    {
        return state with
        {
            LoggedInUsers = state.LoggedInUsers with { IsLoading = true },
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The logged in user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new logged in user state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceLoggedInUsersSuccessAction(DashboardState state, DashboardActions.LoggedInUsersSuccessAction action)
    {
        return state with
        {
            LoggedInUsers = state.LoggedInUsers with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The logged in user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new logged in user state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceLoggedInUserFailAction(DashboardState state, DashboardActions.LoggedInUsersFailAction action)
    {
        return state with
        {
            LoggedInUsers = state.LoggedInUsers with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load dependents action.
    /// </summary>
    /// <param name="state">The dependents state.</param>
    /// <returns>The new dependents  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadDependentsAction))]
    public static DashboardState ReduceDependentsAction(DashboardState state)
    {
        return state with
        {
            Dependents = state.Dependents with { IsLoading = true },
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The logged in user state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new logged in user state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceDependentsSuccessAction(DashboardState state, DashboardActions.DependentsSuccessAction action)
    {
        return state with
        {
            Dependents = state.Dependents with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The logged in dependents state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new dependents state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceDependentsFailAction(DashboardState state, DashboardActions.DependentsFailAction action)
    {
        return state with
        {
            Dependents = state.Dependents with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load recurring users action.
    /// </summary>
    /// <param name="state">The recurring users state.</param>
    /// <returns>The new recurring users  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadRecurringUsersAction))]
    public static DashboardState ReduceRecurringUsersAction(DashboardState state)
    {
        return state with
        {
            UserCounts = state.UserCounts with { IsLoading = true },
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The recurring users state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new recurring users state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceRecurringUsersSuccessAction(DashboardState state, DashboardActions.RecurringUsersSuccessAction action)
    {
        return state with
        {
            UserCounts = state.UserCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The recurring users state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new recurring users state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceRecurringUsersFailAction(DashboardState state, DashboardActions.RecurringUsersFailAction action)
    {
        return state with
        {
            UserCounts = state.UserCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load rating users action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <returns>The new rating summary  state.</returns>
    [ReducerMethod(typeof(DashboardActions.LoadRatingSummaryAction))]
    public static DashboardState ReduceRatingSummaryAction(DashboardState state)
    {
        return state with
        {
            RatingSummary = state.RatingSummary with { IsLoading = true },
        };
    }

    /// <summary>
    /// The Reducer for the load success action.
    /// </summary>
    /// <param name="state">The rating summary state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new rating summary state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceRatingSummarySuccessAction(DashboardState state, DashboardActions.RatingSummarySuccessAction action)
    {
        return state with
        {
            RatingSummary = state.RatingSummary with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    /// <summary>
    /// The Reducer for the load fail action.
    /// </summary>
    /// <param name="state">The logged in rating summary state.</param>
    /// <param name="action">The load success action.</param>
    /// <returns>The new ratings summary state.</returns>
    [ReducerMethod]
    public static DashboardState ReduceRatingSummaryFailAction(DashboardState state, DashboardActions.RatingSummaryFailAction action)
    {
        return state with
        {
            RatingSummary = state.RatingSummary with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    /// <summary>
    /// The reducer for the reset state action.
    /// </summary>
    /// <param name="state">The dashboard state.</param>
    /// <returns>The default state.</returns>
    [ReducerMethod(typeof(DashboardActions.ResetStateAction))]
    public static DashboardState ReduceResetStateAction(DashboardState state)
    {
        return state with
        {
            RegisteredUsers = new(),
            LoggedInUsers = new(),
            Dependents = new(),
            UserCounts = new(),
            RatingSummary = new(),
        };
    }
}
