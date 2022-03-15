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
namespace HealthGateway.Admin.Client.Store.Configuration
{
    using Fluxor;

    /// <summary>
    /// The set of reducers for the feature.
    /// </summary>
    public static class ConfigurationReducers
    {
        /// <summary>
        /// The Reducer for loading the configuration.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod(typeof(ConfigurationActions.LoadAction))]
        public static ConfigurationState ReduceLoadAction(ConfigurationState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

        /// <summary>
        /// The Reducer for the load success action.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <param name="action">The load success action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static ConfigurationState ReduceLoadSuccessAction(ConfigurationState state, ConfigurationActions.LoadSuccessAction action)
        {
            return state with
            {
                IsLoading = false,
                Result = action.Data,
                Error = null,
            };
        }

        /// <summary>
        /// The Reducer for the fail action.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <param name="action">The load fail action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static ConfigurationState ReduceLoadFailAction(ConfigurationState state, ConfigurationActions.LoadFailAction action)
        {
            return state with
            {
                IsLoading = false,
                Error = action.Error,
            };
        }
    }
}
