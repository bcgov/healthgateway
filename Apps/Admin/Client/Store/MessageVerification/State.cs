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
    using Fluxor;
    using HealthGateway.Admin.Client.Store;
    using HealthGateway.Common.Data.ViewModels;

    /// <summary>
    /// MessageVerificationState.
    /// State should be decorated with [FeatureState] for automatic discovery when services. AddFluxor is called.
    /// </summary>
    [FeatureState]
    public record State : BaseState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        /// <param name="requestResult">Class that returns from service.</param>
        public State(RequestResult<IEnumerable<MessagingVerificationModel>> requestResult)
        {
            this.RequestResult = requestResult;
        }

        /// A parameterless constructor is required on state for determining the initial state, and can be private or public.
        /// <summary>
        /// Initializes a new instance of the <see cref="State"/> class.
        /// </summary>
        public State()
        {
        }

        /// <summary>
        /// Gets messagingVerification.
        /// </summary>
        public RequestResult<IEnumerable<MessagingVerificationModel>>? RequestResult { get; init; }
    }
}
