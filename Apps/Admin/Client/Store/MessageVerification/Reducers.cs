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
    /// The set of Reducers for the Configuration.
    /// </summary>
    public static class Reducers
    {
        /// <summary>
        /// The Reducer for loading the message verification.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod(typeof(Actions.LoadAction))]
        public static State ReduceLoadAction(State state)
        {
            return state with
            {
                RequestResult = state.RequestResult,
                IsLoading = true,
                ErrorMessage = state.ErrorMessage,
            };
        }

        /// <summary>
        /// The Reducer for the load success action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <param name="action">The load success action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static State ReduceLoadSuccessAction(State state, Actions.LoadSuccessAction action)
        {
            return state with
            {
                RequestResult = action.State,
                IsLoading = false,
            };
        }

        /// <summary>
        /// The Reducer for the fail action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <param name="action">The load fail action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static State ReduceLoadFailAction(State state, Actions.LoadFailAction action)
        {
            return state with
            {
                RequestResult = state.RequestResult,
                IsLoading = false,
                ErrorMessage = action.ErrorMessage,
            };
        }
    }
}
