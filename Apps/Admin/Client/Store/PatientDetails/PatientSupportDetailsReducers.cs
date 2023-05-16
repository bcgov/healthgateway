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
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Fluxor;
    using HealthGateway.Common.Data.ViewModels;

#pragma warning disable CS1591, SA1600
    public static class PatientSupportDetailsReducers
    {
        [ReducerMethod(typeof(PatientSupportDetailsActions.LoadAction))]
        public static PatientSupportDetailsState ReduceLoadAction(PatientSupportDetailsState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        [ReducerMethod]
        public static PatientSupportDetailsState ReduceLoadSuccessAction(PatientSupportDetailsState state, PatientSupportDetailsActions.LoadSuccessAction action)
        {
            ImmutableList<MessagingVerificationModel> messageVerifications = state.MessagingVerifications ?? new List<MessagingVerificationModel>().ToImmutableList();

            IEnumerable<MessagingVerificationModel>? verifications = action.Data?.MessagingVerifications;
            if (verifications != null)
            {
                messageVerifications = messageVerifications.RemoveAll(x => x.UserProfileId == action.Hdid).AddRange(verifications);
            }

            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
                MessagingVerifications = messageVerifications,
                BlockedDataSources = action.Data?.BlockedDataSources,
                AgentActions = action.Data?.AgentActions,
            };
        }

        [ReducerMethod]
        public static PatientSupportDetailsState ReduceLoadFailAction(PatientSupportDetailsState state, PatientSupportDetailsActions.LoadFailAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }

        [ReducerMethod(typeof(PatientSupportDetailsActions.ResetStateAction))]
        public static PatientSupportDetailsState ReduceResetStateAction(PatientSupportDetailsState state)
        {
            return state with
            {
                IsLoading = false,
                Result = null,
                Error = null,
                MessagingVerifications = null,
                BlockedDataSources = null,
                AgentActions = null,
            };
        }
    }
}
