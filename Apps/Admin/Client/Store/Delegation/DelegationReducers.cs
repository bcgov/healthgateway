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
namespace HealthGateway.Admin.Client.Store.Delegation
{
    using Fluxor;

#pragma warning disable CS1591, SA1600
    public static class DelegationReducers
    {
        [ReducerMethod(typeof(DelegationActions.SearchAction))]
        public static DelegationState ReduceSearchAction(DelegationState state)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSearchSuccessAction(DelegationState state, DelegationActions.SearchSuccessAction action)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = false,
                    Result = action.Data,
                    Error = null,
                },
                Data = action.Data,
            };
        }

        [ReducerMethod]
        public static DelegationState ReduceSearchFailAction(DelegationState state, DelegationActions.SearchFailAction action)
        {
            return state with
            {
                Search = state.Search with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(DelegationActions.ResetStateAction))]
        public static DelegationState ReduceResetStateAction(DelegationState state)
        {
            return state with
            {
                Search = new(),
                Data = null,
            };
        }
    }
}
