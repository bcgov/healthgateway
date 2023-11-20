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

#pragma warning disable CS1591, SA1600
public static class DashboardReducers
{
    [ReducerMethod(typeof(DashboardActions.GetDailyUserRegistrationCountsAction))]
    public static DashboardState ReduceGetRegisteredUsersAction(DashboardState state)
    {
        return state with
        {
            GetDailyUserRegistrationCounts = state.GetDailyUserRegistrationCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDailyUserRegistrationCountsSuccessAction(DashboardState state, DashboardActions.GetDailyUserRegistrationCountsSuccessAction action)
    {
        return state with
        {
            GetDailyUserRegistrationCounts = state.GetDailyUserRegistrationCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDailyUserRegistrationCountsFailureAction(DashboardState state, DashboardActions.GetDailyUserRegistrationCountsFailureAction action)
    {
        return state with
        {
            GetDailyUserRegistrationCounts = state.GetDailyUserRegistrationCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetDailyUniqueLoginCountsAction))]
    public static DashboardState ReduceGetDailyUniqueLoginCountsAction(DashboardState state)
    {
        return state with
        {
            GetDailyUniqueLoginCounts = state.GetDailyUniqueLoginCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDailyUniqueLoginCountsSuccessAction(DashboardState state, DashboardActions.GetDailyUniqueLoginCountsSuccessAction action)
    {
        return state with
        {
            GetDailyUniqueLoginCounts = state.GetDailyUniqueLoginCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState GetDailyUniqueLoginCountsFailureAction(DashboardState state, DashboardActions.GetDailyUniqueLoginCountsFailureAction action)
    {
        return state with
        {
            GetDailyUniqueLoginCounts = state.GetDailyUniqueLoginCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetDailyDependentRegistrationCountsAction))]
    public static DashboardState ReduceGetDailyDependentRegistrationCountsAction(DashboardState state)
    {
        return state with
        {
            GetDailyDependentRegistrationCounts = state.GetDailyDependentRegistrationCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDailyDependentRegistrationCountsSuccessAction(DashboardState state, DashboardActions.GetDailyDependentRegistrationCountsSuccessAction action)
    {
        return state with
        {
            GetDailyDependentRegistrationCounts = state.GetDailyDependentRegistrationCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetDailyDependentRegistrationCountsFailureAction(DashboardState state, DashboardActions.GetDailyDependentRegistrationCountsFailureAction action)
    {
        return state with
        {
            GetDailyDependentRegistrationCounts = state.GetDailyDependentRegistrationCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetRecurringUserCountAction))]
    public static DashboardState ReduceGetRecurringUserCountAction(DashboardState state)
    {
        return state with
        {
            GetRecurringUserCount = state.GetRecurringUserCount with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRecurringUserCountSuccessAction(DashboardState state, DashboardActions.GetRecurringUserCountSuccessAction action)
    {
        return state with
        {
            GetRecurringUserCount = state.GetRecurringUserCount with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRecurringUserCountFailureAction(DashboardState state, DashboardActions.GetRecurringUserCountFailureAction action)
    {
        return state with
        {
            GetRecurringUserCount = state.GetRecurringUserCount with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetAppLoginCountsAction))]
    public static DashboardState ReduceGetAppLoginCountsAction(DashboardState state)
    {
        return state with
        {
            GetAppLoginCounts = state.GetAppLoginCounts with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetAppLoginCountsSuccessAction(DashboardState state, DashboardActions.GetAppLoginCountsSuccessAction action)
    {
        return state with
        {
            GetAppLoginCounts = state.GetAppLoginCounts with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetAppLoginCountsFailureAction(DashboardState state, DashboardActions.GetAppLoginCountsFailureAction action)
    {
        return state with
        {
            GetAppLoginCounts = state.GetAppLoginCounts with
            {
                Result = null,
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(DashboardActions.GetRatingsSummaryAction))]
    public static DashboardState ReduceGetRatingsSummaryAction(DashboardState state)
    {
        return state with
        {
            GetRatingsSummary = state.GetRatingsSummary with { IsLoading = true },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRatingsSummarySuccessAction(DashboardState state, DashboardActions.GetRatingsSummarySuccessAction action)
    {
        return state with
        {
            GetRatingsSummary = state.GetRatingsSummary with
            {
                Result = action.Data,
                IsLoading = false,
                Error = null,
            },
        };
    }

    [ReducerMethod]
    public static DashboardState ReduceGetRatingsSummaryFailureAction(DashboardState state, DashboardActions.GetRatingsSummaryFailureAction action)
    {
        return state with
        {
            GetRatingsSummary = state.GetRatingsSummary with
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
        IDictionary<string, int> yearOfBirthCounts = new Dictionary<string, int>(action.Data);

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
            GetDailyUserRegistrationCounts = new(),
            GetDailyUniqueLoginCounts = new(),
            GetDailyDependentRegistrationCounts = new(),
            GetRecurringUserCount = new(),
            GetAppLoginCounts = new(),
            GetRatingsSummary = new(),
            GetYearOfBirthCounts = new(),
            YearOfBirthCounts = ImmutableDictionary<string, int>.Empty,
        };
    }
}
