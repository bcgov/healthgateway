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
                RefreshDiagnosticImagingCache = new(),
                RefreshLaboratoryCache = new(),
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshDiagnosticImagingCacheAction))]
        public static HealthDataState ReduceRefreshDiagnosticImagingCacheAction(HealthDataState state)
        {
            return state with
            {
                RefreshDiagnosticImagingCache = state.RefreshDiagnosticImagingCache with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshDiagnosticImagingCacheSuccessAction))]
        public static HealthDataState ReduceRefreshDiagnosticImagingSuccessAction(HealthDataState state)
        {
            return state with
            {
                RefreshDiagnosticImagingCache = state.RefreshDiagnosticImagingCache with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static HealthDataState ReduceRefreshDiagnosticImagingCacheFailureAction(HealthDataState state, HealthDataActions.RefreshDiagnosticImagingCacheFailureAction action)
        {
            return state with
            {
                RefreshDiagnosticImagingCache = state.RefreshDiagnosticImagingCache with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(HealthDataActions.RefreshLaboratoryCacheAction))]
        public static HealthDataState ReduceRefreshLaboratoryCacheAction(HealthDataState state)
        {
            return state with
            {
                RefreshLaboratoryCache = state.RefreshLaboratoryCache with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static HealthDataState ReduceRefreshLaboratoryCacheSuccessAction(HealthDataState state, HealthDataActions.RefreshLaboratoryCacheSuccessAction action)
        {
            return state with
            {
                RefreshLaboratoryCache = state.RefreshLaboratoryCache with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static HealthDataState ReduceRefreshLaboratoryCacheFailureAction(HealthDataState state, HealthDataActions.RefreshLaboratoryCacheFailureAction action)
        {
            return state with
            {
                RefreshLaboratoryCache = state.RefreshLaboratoryCache with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
