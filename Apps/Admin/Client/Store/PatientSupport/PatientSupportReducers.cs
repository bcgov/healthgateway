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
namespace HealthGateway.Admin.Client.Store.PatientSupport
{
    using System.Collections.Immutable;
    using Fluxor;

    public static class PatientSupportReducers
    {
        [ReducerMethod(typeof(PatientSupportActions.LoadAction))]
        public static PatientSupportState ReduceLoadAction(PatientSupportState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        [ReducerMethod]
        public static PatientSupportState ReduceLoadSuccessAction(PatientSupportState state, PatientSupportActions.LoadSuccessAction action)
        {
            return state with
            {
                IsLoading = false,
                Result = action.Data.ToImmutableList(),
                Error = null,
            };
        }

        [ReducerMethod]
        public static PatientSupportState ReduceLoadFailureAction(PatientSupportState state, PatientSupportActions.LoadFailureAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }

        [ReducerMethod(typeof(PatientSupportActions.ResetStateAction))]
        public static PatientSupportState ReduceResetStateAction(PatientSupportState state)
        {
            return state with
            {
                IsLoading = false,
                Result = null,
                Error = null,
            };
        }
    }
}
