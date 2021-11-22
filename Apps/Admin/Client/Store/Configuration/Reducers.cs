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
    /// The set of Reducers for the Configuration.
    /// </summary>
    public static class Reducers
    {
        /// <summary>
        /// The Reducer for loading the configuration.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <param name="action">The load action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static State ReduceLoadConfigurationAction(State state, LoadAction action) => new(state.Configuration, true);

        /// <summary>
        /// The Reducer for the load success action.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <param name="action">The load success action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static State ReduceLoadConfigurationSuccessAction(State state, LoadSuccessAction action) => new(action.Configuration);

        /// <summary>
        /// The Reducer for the fail action.
        /// </summary>
        /// <param name="state">The configuration state.</param>
        /// <param name="action">The load fail action.</param>
        /// <returns>The new state.</returns>
        [ReducerMethod]
        public static State ReduceLoadConfigurationFailAction(State state, LoadFailAction action) => new(state.Configuration, false, action.ErrorMessage);
    }
}
