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
    using HealthGateway.Admin.Client.Store.Shared;
    using HealthGateway.Admin.Common.Models;

    /// <summary>
    /// State for Configuration.
    /// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
    /// </summary>
    [FeatureState]
    public class State : BaseState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to store.</param>
        /// <param name="isLoading">True if the data is being loaded.</param>
        /// <param name="errorMessage">An error message if the state was not loaded.</param>
        public State(ExternalConfiguration configuration, bool isLoading = false, string? errorMessage = null)
            : base(isLoading, errorMessage)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// A parameterless constructor is required on state for determining the initial state, and can be private or public.
        /// </summary>
        public State()
       : base(false, null)
        {
        }

        /// <summary>
        /// Gets the loaded configuration.
        /// </summary>
        public ExternalConfiguration? Configuration { get; init; }
    }
}
