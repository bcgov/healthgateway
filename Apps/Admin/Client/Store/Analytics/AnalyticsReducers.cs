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

#pragma warning disable CS1591, SA1600
public static class AnalyticsReducers
{
    [ReducerMethod(typeof(AnalyticsActions.LoadUserProfilesAction))]
    public static AnalyticsState ReduceLoadUserProfilesAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadCommentsAction))]
    public static AnalyticsState ReduceLoadCommentsAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadNotesAction))]
    public static AnalyticsState ReduceLoadNotesAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadRatingsAction))]
    public static AnalyticsState ReduceLoadRatingsAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadInactiveUsersAction))]
    public static AnalyticsState ReduceLoadInactiveUsersAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadUserFeedbackAction))]
    public static AnalyticsState ReduceLoadUserFeedbackAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.LoadYearOfBirthCountsAction))]
    public static AnalyticsState ReduceYearOfBirthCountsAction(AnalyticsState state)
    {
        return state with
        {
            IsLoading = true,
        };
    }

    [ReducerMethod]
    public static AnalyticsState ReduceLoadSuccessAction(AnalyticsState state, AnalyticsActions.LoadSuccessAction action)
    {
        return state with
        {
            Result = action.Data,
            IsLoading = false,
            Error = null,
        };
    }

    [ReducerMethod]
    public static AnalyticsState ReduceLoadFailAction(AnalyticsState state, AnalyticsActions.LoadFailAction action)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = action.Error,
        };
    }

    [ReducerMethod(typeof(AnalyticsActions.ResetStateAction))]
    public static AnalyticsState ReduceResetAction(AnalyticsState state)
    {
        return state with
        {
            Result = null,
            IsLoading = false,
            Error = null,
        };
    }
}
