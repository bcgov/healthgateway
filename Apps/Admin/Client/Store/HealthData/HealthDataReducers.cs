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
namespace HealthGateway.Admin.Client.Store.HealthData
{
    using Fluxor;

    public static class HealthDataReducers
    {
        [ReducerMethod(typeof(HealthDataActions.ResetStateAction))]
        public static HealthDataState ReduceResetStateAction(HealthDataState state)
        {
            return state with
            {
                RefreshImagingCache = new(),
                RefreshLabsCache = new(),
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshImagingCacheAction))]
        public static HealthDataState ReduceRefreshImagingCacheAction(HealthDataState state)
        {
            return state with
            {
                RefreshImagingCache = state.RefreshImagingCache with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshImagingCacheSuccessAction))]
        public static HealthDataState ReduceRefreshImagingSuccessAction(HealthDataState state)
        {
            return state with
            {
                RefreshImagingCache = state.RefreshImagingCache with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static HealthDataState ReduceRefreshImagingCacheFailureAction(HealthDataState state, HealthDataActions.RefreshImagingCacheFailureAction action)
        {
            return state with
            {
                RefreshImagingCache = state.RefreshImagingCache with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshLabsCacheAction))]
        public static HealthDataState ReduceRefreshLabsCacheAction(HealthDataState state)
        {
            return state with
            {
                RefreshLabsCache = state.RefreshLabsCache with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshLabsCacheSuccessAction))]
        public static HealthDataState ReduceRefreshLabsCacheSuccessAction(HealthDataState state)
        {
            return state with
            {
                RefreshLabsCache = state.RefreshLabsCache with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static HealthDataState ReduceRefreshLabsCacheFailureAction(HealthDataState state, HealthDataActions.RefreshLabsCacheFailureAction action)
        {
            return state with
            {
                RefreshLabsCache = state.RefreshLabsCache with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
