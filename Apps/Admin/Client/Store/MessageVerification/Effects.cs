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
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// The effects for the feature.
    /// </summary>
    public class Effects
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Effects"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="supportApi">the injected api to query the support. </param>
        public Effects(ILogger<Effects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<Effects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        /// <summary>
        /// Handler that calls the service and dispatch the actions.
        /// </summary>
        /// <param name="action">Load the initial action.</param>
        /// <param name="dispatcher">Dispatch the actions.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [EffectMethod]
        public async Task HandleLoadAction(Actions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading Messaging Verification");
            ApiResponse<RequestResult<IEnumerable<MessagingVerificationModel>>> response = await this.SupportApi.GetMedicationVerifications(action.QueryType, action.QueryString).ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                this.Logger.LogInformation("Messaging Verification loaded successfully!");
                dispatcher.Dispatch(new Actions.LoadSuccessAction(response.Content));
            }
            else
            {
                this.Logger.LogError($"Error loading Messaging Verification, reason: {response.Error?.Message}");
                dispatcher.Dispatch(new Actions.LoadFailAction(response.Error?.Message));
            }
        }
    }
}
