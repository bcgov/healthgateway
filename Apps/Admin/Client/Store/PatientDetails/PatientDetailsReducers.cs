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
namespace HealthGateway.Admin.Client.Store.PatientDetails
{
    using System.Collections.Immutable;
    using Fluxor;

    public static class PatientDetailsReducers
    {
        [ReducerMethod(typeof(PatientDetailsActions.LoadAction))]
        public static PatientDetailsState ReduceLoadAction(PatientDetailsState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        [ReducerMethod]
        public static PatientDetailsState ReduceLoadSuccessAction(PatientDetailsState state, PatientDetailsActions.LoadSuccessAction action)
        {
            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
                MessagingVerifications = action.Data.MessagingVerifications?.ToImmutableList(),
                BlockedDataSources = action.Data.BlockedDataSources?.ToImmutableList(),
                AgentActions = action.Data.AgentActions?.ToImmutableList(),
                Dependents = action.Data.Dependents?.ToImmutableList(),
                VaccineDetails = action.Data.VaccineDetails,
                IsAccountRegistered = action.Data.IsAccountRegistered,
                LastDiagnosticImagingRefreshDate = action.Data.LastDiagnosticImagingRefreshDate,
                LastLaboratoryRefreshDate = action.Data.LastLaboratoryRefreshDate,
            };
        }

        [ReducerMethod]
        public static PatientDetailsState ReduceLoadFailureAction(PatientDetailsState state, PatientDetailsActions.LoadFailureAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }

        [ReducerMethod(typeof(PatientDetailsActions.ResetStateAction))]
        public static PatientDetailsState ReduceResetStateAction(PatientDetailsState state)
        {
            return state with
            {
                IsLoading = false,
                Result = null,
                Error = null,
                MessagingVerifications = null,
                BlockedDataSources = null,
                AgentActions = null,
                Dependents = null,
                VaccineDetails = null,
                IsAccountRegistered = null,
                LastDiagnosticImagingRefreshDate = null,
                LastLaboratoryRefreshDate = null,
            };
        }

        [ReducerMethod(typeof(PatientDetailsActions.BlockAccessAction))]
        public static PatientDetailsState ReduceBlockAccessAction(PatientDetailsState state)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(PatientDetailsActions.BlockAccessSuccessAction))]
        public static PatientDetailsState ReduceBlockAccessSuccessAction(PatientDetailsState state)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static PatientDetailsState ReduceBlockAccessFailureAction(PatientDetailsState state, PatientDetailsActions.BlockAccessFailureAction action)
        {
            return state with
            {
                BlockAccess = state.BlockAccess with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
