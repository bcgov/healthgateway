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
namespace HealthGateway.Admin.Client.Store.VaccineCard
{
    using Fluxor;

#pragma warning disable CS1591, SA1600
    public static class VaccineCardReducers
    {
        [ReducerMethod(typeof(VaccineCardActions.ResetStateAction))]
        public static VaccineCardState ReduceResetStateAction(VaccineCardState state)
        {
            return state with
            {
                MailVaccineCard = new(),
                PrintVaccineCard = new(),
            };
        }

        [ReducerMethod(typeof(VaccineCardActions.MailVaccineCardAction))]
        public static VaccineCardState ReduceMailVaccineCardAction(VaccineCardState state)
        {
            return state with
            {
                MailVaccineCard = state.MailVaccineCard with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod(typeof(VaccineCardActions.MailVaccineCardSuccessAction))]
        public static VaccineCardState ReduceMailVaccineCardSuccessAction(VaccineCardState state)
        {
            return state with
            {
                MailVaccineCard = state.MailVaccineCard with
                {
                    IsLoading = false,
                    Error = null,
                },
            };
        }

        [ReducerMethod]
        public static VaccineCardState ReduceMailVaccineCardFailureAction(VaccineCardState state, VaccineCardActions.MailVaccineCardFailureAction action)
        {
            return state with
            {
                MailVaccineCard = state.MailVaccineCard with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }

        [ReducerMethod(typeof(VaccineCardActions.PrintVaccineCardAction))]
        public static VaccineCardState ReducePrintVaccineCardAction(VaccineCardState state)
        {
            return state with
            {
                PrintVaccineCard = state.PrintVaccineCard with
                {
                    IsLoading = true,
                },
            };
        }

        [ReducerMethod]
        public static VaccineCardState ReducePrintVaccineCardSuccessAction(VaccineCardState state, VaccineCardActions.PrintVaccineCardSuccessAction action)
        {
            return state with
            {
                PrintVaccineCard = state.PrintVaccineCard with
                {
                    IsLoading = false,
                    Error = null,
                    Result = action.Data,
                },
            };
        }

        [ReducerMethod]
        public static VaccineCardState ReducePrintVaccineCardFailureAction(VaccineCardState state, VaccineCardActions.PrintVaccineCardFailureAction action)
        {
            return state with
            {
                PrintVaccineCard = state.PrintVaccineCard with
                {
                    IsLoading = false,
                    Error = action.Error,
                },
            };
        }
    }
}
