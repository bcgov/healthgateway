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
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The state facade.
    /// </summary>
    public class StateFacade
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateFacade"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dispatcher">The dispatcher to use.</param>
        public StateFacade(ILogger<LoadEffect> logger, IDispatcher dispatcher)
        {
            this.Logger = logger;
            this.Dispatcher = dispatcher;
        }

        [Inject]
        private ILogger<LoadEffect> Logger { get; set; }

        [Inject]
        private IDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Requests that the load action is dispatched.
        /// </summary>
        public void LoadConfiguration()
        {
            this.Logger.LogInformation("Issuing action to load Configuration");
            this.Dispatcher.Dispatch(new LoadAction());
        }
    }
}
