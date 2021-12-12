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
    using HealthGateway.Admin.Client.Store.Common;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// StateFacade.
    /// </summary>
    public class StateFacade : BaseStateFacade<LoadEffect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateFacade"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="dispatcher">The dispatcher to use.</param>
        public StateFacade(ILogger<LoadEffect> logger, IDispatcher dispatcher)
            : base(logger, dispatcher)
        {
        }

        /// <summary>
        /// Requests that the load message verification action is dispatched.
        /// </summary>
        /// <param name="queryType">Represents the type of query being performed.</param>
        /// <param name="queryString">Represents the query string being performed.</param>
        public void LoadMessagingVerification(int queryType, string queryString)
        {
            this.Logger.LogInformation("Issuing action to load message verification");
            this.Dispatcher.Dispatch(new LoadAction(queryType, queryString.Trim()));
        }
    }
}
