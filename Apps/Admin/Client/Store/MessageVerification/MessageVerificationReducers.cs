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
namespace HealthGateway.Admin.Client.Store.MessageVerification
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Fluxor;
    using HealthGateway.Common.Data.ViewModels;

#pragma warning disable CS1591, SA1600
    public static class MessageVerificationReducers
    {
        [ReducerMethod(typeof(MessageVerificationActions.LoadAction))]
        public static MessageVerificationState ReduceLoadAction(MessageVerificationState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        [ReducerMethod]
        public static MessageVerificationState ReduceLoadSuccessAction(MessageVerificationState state, MessageVerificationActions.LoadSuccessAction action)
        {
            ImmutableList<MessagingVerificationModel> data = state.Data ?? new List<MessagingVerificationModel>().ToImmutableList();

            IEnumerable<MessagingVerificationModel>? verifications = action.Data.ResourcePayload;
            if (verifications != null)
            {
                data = data.RemoveAll(x => x.UserProfileId == action.Hdid).AddRange(verifications);
            }

            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
                Data = data,
            };
        }

        [ReducerMethod]
        public static MessageVerificationState ReduceLoadFailAction(MessageVerificationState state, MessageVerificationActions.LoadFailAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }

        [ReducerMethod(typeof(MessageVerificationActions.ResetStateAction))]
        public static MessageVerificationState ReduceResetStateAction(MessageVerificationState state)
        {
            return state with
            {
                IsLoading = false,
                Result = null,
                Error = null,
                Data = null,
            };
        }
    }
}
