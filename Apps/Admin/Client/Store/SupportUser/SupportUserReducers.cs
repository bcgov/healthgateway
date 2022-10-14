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
namespace HealthGateway.Admin.Client.Store.SupportUser
{
    using System.Collections.Generic;
    using System.Linq;
    using Fluxor;
    using HealthGateway.Admin.Client.Models;

    /// <summary>
    /// The set of reducers for the feature.
    /// </summary>
    public static class SupportUserReducers
    {
        /// <summary>
        /// The Reducer for loading the support user.
        /// </summary>
        /// <param name="state">The support user state.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod(typeof(SupportUserActions.LoadAction))]
        public static SupportUserState ReduceLoadAction(SupportUserState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        /// <summary>
        /// The Reducer for the load success action.
        /// </summary>
        /// <param name="state">The support user state.</param>
        /// <param name="action">The load success action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static SupportUserState ReduceLoadSuccessAction(SupportUserState state, SupportUserActions.LoadSuccessAction action)
        {
            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
                WarningMessage = action.Data.ResultError?.ResultMessage,
                Data = action.Data.ResourcePayload?.Select(u => new ExtendedSupportUser(u)).ToList(),
            };
        }

        /// <summary>
        /// The Reducer for the fail action.
        /// </summary>
        /// <param name="state">The message verification state.</param>
        /// <param name="action">The load fail action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static SupportUserState ReduceLoadFailAction(SupportUserState state, SupportUserActions.LoadFailAction action)
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
        /// <param name="state">The support user state.</param>
        /// <returns>The empty state.</returns>
        [ReducerMethod(typeof(SupportUserActions.ResetStateAction))]
        public static SupportUserState ReduceResetStateAction(SupportUserState state)
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

        /// <summary>
        /// The reducer for the toggle IsExpanded action.
        /// </summary>
        /// <param name="state">The user state.</param>
        /// <param name="action">The toggle IsExpanded action.</param>
        /// <returns>The default state.</returns>
        [ReducerMethod]
        public static SupportUserState ReduceToggleIsExpandedAction(SupportUserState state, SupportUserActions.ToggleIsExpandedAction action)
        {
            IEnumerable<ExtendedSupportUser> data = state.Data ?? Enumerable.Empty<ExtendedSupportUser>();

            ExtendedSupportUser? user = data.SingleOrDefault(c => c.Hdid == action.Hdid);
            if (user != null)
            {
                user.IsExpanded = !user.IsExpanded;
            }

            return state;
        }
    }
}
