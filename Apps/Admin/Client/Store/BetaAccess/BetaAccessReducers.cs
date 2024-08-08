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
namespace HealthGateway.Admin.Client.Store.BetaAccess;

using System.Collections.Immutable;
using Fluxor;
using HealthGateway.Admin.Common.Models;

/// <summary>
/// The set of reducers for the feature.
/// </summary>
public static class BetaAccessReducers
{
    [ReducerMethod(typeof(BetaAccessActions.SetUserAccessAction))]
    public static BetaAccessState ReduceSetUserAccessAction(BetaAccessState state)
    {
        return state with
        {
            SetUserAccess = state.SetUserAccess with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceSetUserAccessSuccessAction(BetaAccessState state, BetaAccessActions.SetUserAccessSuccessAction action)
    {
        return state with
        {
            SetUserAccess = state.SetUserAccess with
            {
                IsLoading = false,
                Error = null,
            },
            SearchResult = state.SearchResult?.Email == action.Request.Email ? action.Request : state.SearchResult,
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceSetUserAccessFailureAction(BetaAccessState state, BetaAccessActions.SetUserAccessFailureAction action)
    {
        return state with
        {
            SetUserAccess = state.SetUserAccess with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(BetaAccessActions.GetUserAccessAction))]
    public static BetaAccessState ReduceGetUserAccessAction(BetaAccessState state)
    {
        return state with
        {
            GetUserAccess = state.GetUserAccess with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceGetUserAccessSuccessAction(BetaAccessState state, BetaAccessActions.GetUserAccessSuccessAction action)
    {
        return state with
        {
            GetUserAccess = state.GetUserAccess with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            SearchResult = action.Data,
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceGetUserAccessFailureAction(BetaAccessState state, BetaAccessActions.GetUserAccessFailureAction action)
    {
        return state with
        {
            GetUserAccess = state.GetUserAccess with
            {
                IsLoading = false,
                Error = action.Error,
            },
            SearchResult = null,
        };
    }

    [ReducerMethod(typeof(BetaAccessActions.GetAllUserAccessAction))]
    public static BetaAccessState ReduceGetAllUserAccessAction(BetaAccessState state)
    {
        return state with
        {
            GetAllUserAccess = state.GetAllUserAccess with
            {
                IsLoading = true,
            },
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceGetAllUserAccessSuccessAction(BetaAccessState state, BetaAccessActions.GetAllUserAccessSuccessAction action)
    {
        IImmutableDictionary<string, UserBetaAccess> allUserAccess = action.Data.ToImmutableDictionary(a => a.Email, a => a);

        return state with
        {
            GetAllUserAccess = state.GetAllUserAccess with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            },
            AllUserAccess = allUserAccess,
        };
    }

    [ReducerMethod]
    public static BetaAccessState ReduceGetAllUserAccessFailureAction(BetaAccessState state, BetaAccessActions.GetAllUserAccessFailureAction action)
    {
        return state with
        {
            GetAllUserAccess = state.GetAllUserAccess with
            {
                IsLoading = false,
                Error = action.Error,
            },
        };
    }

    [ReducerMethod(typeof(BetaAccessActions.ResetStateAction))]
    public static BetaAccessState ReduceResetStateAction(BetaAccessState state)
    {
        return state with
        {
            SetUserAccess = new(),
            GetUserAccess = new(),
            GetAllUserAccess = new(),
            AllUserAccess = null,
            SearchResult = null,
        };
    }
}
