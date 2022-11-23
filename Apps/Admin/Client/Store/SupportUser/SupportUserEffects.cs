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

namespace HealthGateway.Admin.Client.Store.SupportUser
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

    /// <summary>
    /// The effects for the feature.
    /// </summary>
    public class SupportUserEffects
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportUserEffects"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="supportApi">the injected api to query the support. </param>
        public SupportUserEffects(ILogger<SupportUserEffects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<SupportUserEffects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        /// <summary>
        /// Handler that calls the service and dispatch the actions.
        /// </summary>
        /// <param name="action">Load the initial action.</param>
        /// <param name="dispatcher">Dispatch the actions.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [EffectMethod]
        public async Task HandleLoadAction(SupportUserActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading users!");

            try
            {
                RequestResult<IEnumerable<SupportUser>> response = await this.SupportApi.GetSupportUsersAsync(action.QueryType, action.QueryString).ConfigureAwait(true);
                if (response.ResultStatus == ResultType.Success)
                {
                    this.Logger.LogInformation("Users loaded successfully!");
                    dispatcher.Dispatch(new SupportUserActions.LoadSuccessAction(response));
                }
                else if (response.ResultStatus == ResultType.ActionRequired)
                {
                    this.Logger.LogInformation("Users loaded with warning message: {WarningMessage}", response.ResultError?.ResultMessage);
                    dispatcher.Dispatch(new SupportUserActions.LoadSuccessAction(response));
                }
                else
                {
                    RequestError error = StoreUtility.FormatRequestError(response.ResultError);
                    this.Logger.LogError("Error loading users, reason: {ErrorMessage}", error.Message);
                    dispatcher.Dispatch(new SupportUserActions.LoadFailAction(error));
                }
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading users...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading users, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new SupportUserActions.LoadFailAction(error));
            }
        }
    }
}
