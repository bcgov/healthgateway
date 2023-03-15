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

namespace HealthGateway.Admin.Client.Store.Delegation
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class DelegationEffects
    {
        public DelegationEffects(ILogger<DelegationEffects> logger, IDelegationApi api)
        {
            this.Logger = logger;
            this.Api = api;
        }

        [Inject]
        private ILogger<DelegationEffects> Logger { get; set; }

        [Inject]
        private IDelegationApi Api { get; set; }

        [EffectMethod]
        public async Task HandleSearchAction(DelegationActions.SearchAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Retrieving delegation info");
            try
            {
                DelegationInfo response = await this.Api.GetDelegationInformationAsync(action.Phn).ConfigureAwait(true);
                this.Logger.LogInformation("Delegation info retrieved successfully");
                dispatcher.Dispatch(new DelegationActions.SearchSuccessAction(response));
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError(e, "Error retrieving delegation info, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.SearchFailAction(error));
            }
        }
    }
}
