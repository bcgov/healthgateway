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
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Common.Data.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// The effect for the Load Action.
    /// </summary>
    public class LoadEffect : Effect<LoadAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadEffect"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="supportApi">the injected api to query the support. </param>
        public LoadEffect(ILogger<LoadEffect> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<LoadEffect> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        /// <inheritdoc/>
        public override async Task HandleAsync(LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading Messaging Verification");
            ApiResponse<MessagingVerification> response = await this.SupportApi.GetMedicationVerification(3, "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A").ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                this.Logger.LogInformation("Messaging Verification loaded successfully!");
                dispatcher.Dispatch(new LoadSuccessAction(response.Content));
            }
            else
            {
                this.Logger.LogError($"Error loading Messaging Verification, reason: {response.Error?.Message}");
                dispatcher.Dispatch(new LoadFailAction(response.Error?.Message));
            }
        }
    }
}
