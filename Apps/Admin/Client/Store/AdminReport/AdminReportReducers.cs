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
namespace HealthGateway.Admin.Client.Store.AdminReport
{
    using Fluxor;

#pragma warning disable CS1591, SA1600
    public static class AdminReportReducers
    {
        [ReducerMethod(typeof(AdminReportActions.ResetStateAction))]
        public static AdminReportState ReduceResetStateAction(AdminReportState state)
        {
            return state with
            {
                BlockedAccess = new(),
            };
        }

        [ReducerMethod(typeof(AdminReportActions.GetBlockedAccessAction))]
        public static AdminReportState ReduceGetBlockedAccessAction(AdminReportState state)
        {
            return state with
            {
                BlockedAccess = state.BlockedAccess with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static AdminReportState ReduceGetBlockedAccessSuccessAction(AdminReportState state, AdminReportActions.GetBlockedAccessSuccessAction action)
        {
            return state with
            {
                BlockedAccess = state.BlockedAccess with
                {
                    IsLoading = false,
                    Error = null,
                    Result = action.Data,
                },
            };
        }

        [ReducerMethod]
        public static AdminReportState ReduceGetBlockedAccessFailureAction(AdminReportState state, AdminReportActions.GetBlockedAccessFailureAction action)
        {
            return state with
            {
                BlockedAccess = state.BlockedAccess with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
