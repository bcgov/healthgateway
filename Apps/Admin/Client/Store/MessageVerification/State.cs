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
    using HealthGateway.Admin.Client.Store.Shared;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// MessageVerificationState.
    /// State should be decorated with [FeatureState] for automatic discovery when services.AddFluxor is called.
    /// </summary>
    [FeatureState]
    public class State : BaseState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="messagingVerification">messagingVerification.</param>
        /// <param name="isLoading">True if the data is being loaded.</param>
        /// <param name="errorMessage">An error message if the state was not loaded.</param>
        public State(MessagingVerification messagingVerification, bool isLoading = false, string? errorMessage = null)
            : base(isLoading, errorMessage)
        {
            this.MessagingVerification = messagingVerification;
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
        /// Gets or sets messagingVerification.
        /// </summary>
        public MessagingVerification? MessagingVerification { get; set; }
    }
}
