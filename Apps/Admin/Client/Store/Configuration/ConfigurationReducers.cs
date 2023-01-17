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

#pragma warning disable CS1591, SA1600
    public static class ConfigurationReducers
    {
        [ReducerMethod(typeof(ConfigurationActions.LoadAction))]
        public static ConfigurationState ReduceLoadAction(ConfigurationState state)
        {
            return state with
            {
                IsLoading = true,
            };
        }

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
