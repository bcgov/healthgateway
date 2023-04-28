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
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using HealthGateway.Admin.Client.Models;

#pragma warning disable CS1591, SA1600
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
                Result = action.Data,
                Error = null,
                WarningMessage = action.Data.ResultError?.ResultMessage,
                Data = action.Data.ResourcePayload?.Select(u => new ExtendedPatientSupportDetails(u)).ToList(),
            };
        }

        [ReducerMethod]
        public static PatientSupportState ReduceLoadFailAction(PatientSupportState state, PatientSupportActions.LoadFailAction action)
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
                Data = null,
                WarningMessage = null,
            };
        }

        [ReducerMethod]
        public static PatientSupportState ReduceToggleIsExpandedAction(PatientSupportState state, PatientSupportActions.ToggleIsExpandedAction action)
        {
            IEnumerable<ExtendedPatientSupportDetails> data = state.Data ?? Enumerable.Empty<ExtendedPatientSupportDetails>();

            ExtendedPatientSupportDetails? user = data.SingleOrDefault(c => c.Hdid == action.Hdid);
            if (user != null)
            {
                user.IsExpanded = !user.IsExpanded;
            }

            return state;
        }
    }
}
