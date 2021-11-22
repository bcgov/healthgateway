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
    using System;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Admin.Common.Models;
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
        /// <param name="configApi">the injected api to query the configuration. </param>
        public LoadEffect(ILogger<LoadEffect> logger, IConfigurationApi configApi)
        {
            this.Logger = logger;
            this.ConfigApi = configApi;
        }

        [Inject]
        private ILogger<LoadEffect> Logger { get; set; }

        [Inject]
        private IConfigurationApi ConfigApi { get; set; }

        /// <inheritdoc/>
        public override async Task HandleAsync(LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading External Configuration");
            ApiResponse<ExternalConfiguration> response = await this.ConfigApi.GetConfiguration().ConfigureAwait(true);
            if (response.IsSuccessStatusCode)
            {
                this.Logger.LogInformation("External Configuration loaded successfully!");
                await Task.Delay(TimeSpan.FromMilliseconds(2000)).ConfigureAwait(true);
                dispatcher.Dispatch(new LoadSuccessAction(response.Content));
            }
            else
            {
                this.Logger.LogError($"Error loading External Configuration, reason: {response.Error?.Message}");
                dispatcher.Dispatch(new LoadFailAction(response.Error?.Message));
            }
        }
    }
}
