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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Fluxor;

public static class DashboardReducers
{
    [ReducerMethod(typeof(DashboardActions.GetRegisteredUsersAction))]
    public static DashboardState ReduceGetRegisteredUsersAction(DashboardState state)
    {
        return state with
        {
            GetRegisteredUsers = state.GetRegisteredUsers with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRegisteredUsersSuccessAction(DashboardState state, DashboardActions.GetRegisteredUsersSuccessAction action)
    {
        return state with
        {
            GetRegisteredUsers = state.GetRegisteredUsers with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRegisteredUsersFailureAction(DashboardState state, DashboardActions.GetRegisteredUsersFailureAction action)
    {
        return state with
        {
            GetRegisteredUsers = state.GetRegisteredUsers with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetLoggedInUsersAction))]
    public static DashboardState ReduceGetLoggedInUsersAction(DashboardState state)
    {
        return state with
        {
            GetLoggedInUsers = state.GetLoggedInUsers with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetLoggedInUsersSuccessAction(DashboardState state, DashboardActions.GetLoggedInUsersSuccessAction action)
    {
        return state with
        {
            GetLoggedInUsers = state.GetLoggedInUsers with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetLoggedInUserFailureAction(DashboardState state, DashboardActions.GetLoggedInUsersFailureAction action)
    {
        return state with
        {
            GetLoggedInUsers = state.GetLoggedInUsers with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetDependentsAction))]
    public static DashboardState ReduceGetDependentsAction(DashboardState state)
    {
        return state with
        {
            GetDependents = state.GetDependents with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReducGeteDependentsSuccessAction(DashboardState state, DashboardActions.GetDependentsSuccessAction action)
    {
        return state with
        {
            GetDependents = state.GetDependents with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDependentsFailureAction(DashboardState state, DashboardActions.GetDependentsFailureAction action)
    {
        return state with
        {
            GetDependents = state.GetDependents with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetUserCountsAction))]
    public static DashboardState ReduceGetUserCountsAction(DashboardState state)
    {
        return state with
        {
            GetUserCounts = state.GetUserCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetUserCountsSuccessAction(DashboardState state, DashboardActions.GetUserCountsSuccessAction action)
    {
        return state with
        {
            GetUserCounts = state.GetUserCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetUserCountsFailureAction(DashboardState state, DashboardActions.GetUserCountsFailureAction action)
    {
        return state with
        {
            GetUserCounts = state.GetUserCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetRatingSummaryAction))]
    public static DashboardState ReduceGetRatingSummaryAction(DashboardState state)
    {
        return state with
        {
            GetRatingSummary = state.GetRatingSummary with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRatingSummarySuccessAction(DashboardState state, DashboardActions.GetRatingSummarySuccessAction action)
    {
        return state with
        {
            GetRatingSummary = state.GetRatingSummary with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRatingSummaryFailureAction(DashboardState state, DashboardActions.GetRatingSummaryFailureAction action)
    {
        return state with
        {
            GetRatingSummary = state.GetRatingSummary with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetYearOfBirthCountsAction))]
    public static DashboardState ReduceGetYearOfBirthCountsAction(DashboardState state)
    {
        return state with
        {
            GetYearOfBirthCounts = state.GetYearOfBirthCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetYearOfBirthCountsSuccessAction(DashboardState state, DashboardActions.GetYearOfBirthCountsSuccessAction action)
    {
        List<string> years = action.Data.Keys.Order().ToList();
        Dictionary<string, int> yearOfBirthCounts = new Dictionary<string, int>(action.Data);

        // add empty entries for years that are not populated
        if (years.Count > 0 && int.TryParse(years[0], out int firstYear) && int.TryParse(years[^1], out int lastYear))
        {
            for (int year = firstYear; year <= lastYear; year++)
            {
                string yearString = year.ToString(CultureInfo.InvariantCulture);
                if (!yearOfBirthCounts.ContainsKey(yearString))
                {
                    yearOfBirthCounts[yearString] = 0;
                }
            }
        }

        return state with
        {
            GetYearOfBirthCounts = state.GetYearOfBirthCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
            YearOfBirthCounts = yearOfBirthCounts.ToImmutableSortedDictionary(),
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetYearOfBirthCountsFailureAction(DashboardState state, DashboardActions.GetYearOfBirthCountsFailureAction action)
    {
        return state with
        {
            GetYearOfBirthCounts = state.GetYearOfBirthCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
            YearOfBirthCounts = ImmutableDictionary<string, int>.Empty,
        };
    }

    [ReducerMethod(typeof(DashboardActions.ResetStateAction))]
    public static DashboardState ReduceResetStateAction(DashboardState state)
    {
        return state with
        {
            GetRegisteredUsers = new(),
            GetLoggedInUsers = new(),
            GetDependents = new(),
            GetUserCounts = new(),
            GetRatingSummary = new(),
            GetYearOfBirthCounts = new(),
            YearOfBirthCounts = ImmutableDictionary<string, int>.Empty,
        };
    }
}
