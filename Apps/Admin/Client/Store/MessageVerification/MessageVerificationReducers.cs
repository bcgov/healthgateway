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
    using Fluxor;

    /// <summary>
    /// The set of reducers for the feature.
    /// </summary>
    public static class MessageVerificationReducers
    {
        /// <summary>
        /// The Reducer for loading the message verification.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod(typeof(MessageVerificationActions.LoadAction))]
        public static MessageVerificationState ReduceLoadAction(MessageVerificationState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        /// <summary>
        /// The Reducer for the load success action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <param name="action">The load success action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static MessageVerificationState ReduceLoadSuccessAction(MessageVerificationState state, MessageVerificationActions.LoadSuccessAction action)
        {
            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
                WarningMessage = action.Data.ResultError?.ResultMessage,
            };
        }

        /// <summary>
        /// The Reducer for the fail action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <param name="action">The load fail action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static MessageVerificationState ReduceLoadFailAction(MessageVerificationState state, MessageVerificationActions.LoadFailAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }

        /// <summary>
        /// The Reducer for the reset state action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <returns>The empty state.</returns>
        [ReducerMethod(typeof(MessageVerificationActions.ResetStateAction))]
        public static MessageVerificationState ReduceResetStateAction(MessageVerificationState state)
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
