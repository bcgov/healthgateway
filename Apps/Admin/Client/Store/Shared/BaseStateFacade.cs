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

namespace HealthGateway.Admin.Client.Store.Shared
{
    using Fluxor;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The base state facade.
    /// </summary>
    /// <typeparam name="T">generic state facade class.</typeparam>
    public abstract class BaseStateFacade<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStateFacade{T}"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dispatcher">The dispatcher to use.</param>
        protected BaseStateFacade(ILogger<T> logger, IDispatcher dispatcher)
        {
            this.Logger = logger;
            this.Dispatcher = dispatcher;
        }

        /// <summary>
        /// Gets or sets dispatcher.
        /// </summary>
        [Inject]
        protected IDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Gets or sets logger.
        /// </summary>
        [Inject]
        protected ILogger<T> Logger { get; set; }
    }
}
