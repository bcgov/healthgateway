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
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Models;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class DelegationEffects
    {
        public DelegationEffects(IDelegationApi api, IMapper autoMapper, IState<DelegationState> delegationState, ILogger<DelegationEffects> logger)
        {
            this.Api = api;
            this.AutoMapper = autoMapper;
            this.DelegationState = delegationState;
            this.Logger = logger;
        }

        private IDelegationApi Api { get; }

        private IMapper AutoMapper { get; }

        private IState<DelegationState> DelegationState { get; }

        private ILogger<DelegationEffects> Logger { get; }

        [EffectMethod]
        public async Task HandleSearchAction(DelegationActions.SearchAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Retrieving delegation info");
            try
            {
                DelegationInfo response = await this.Api.GetDelegationInformationAsync(action.Phn).ConfigureAwait(true);
                this.Logger.LogInformation("Delegation info retrieved successfully");
                dispatcher.Dispatch(
                    new DelegationActions.SearchSuccessAction
                    {
                        Dependent = response.Dependent,
                        Delegates = this.AutoMapper.Map<IEnumerable<DelegateInfo>, IEnumerable<ExtendedDelegateInfo>>(response.Delegates),
                    });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = e switch
                {
                    ApiException { StatusCode: HttpStatusCode.BadRequest } => new RequestError { Message = "This feature is only available for users 11 and under." },
                    _ => StoreUtility.FormatRequestError(e),
                };

                this.Logger.LogError(e, "Error retrieving delegation info, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.SearchFailAction(error));
            }
        }

        [EffectMethod(typeof(DelegationActions.ProtectDependentAction))]
        public async Task HandleProtectDependentAction(IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Protect dependent");
            try
            {
                string? dependentHdid = this.DelegationState.Value.Dependent?.Hdid;
                if (dependentHdid == null)
                {
                    RequestError error = new() { Message = "Dependent HDID is null" };
                    dispatcher.Dispatch(new DelegationActions.ProtectDependentFailAction(error));
                    return;
                }

                IEnumerable<string> delegateHdids = this.DelegationState.Value.Delegates
                    .Where(d => d.StagedDelegationStatus is DelegationStatus.Added or DelegationStatus.Allowed)
                    .Select(x => x.Hdid);

                await this.Api.ProtectDependentAsync(dependentHdid, delegateHdids).ConfigureAwait(true);
                this.Logger.LogInformation("Dependent protected successfully");
                dispatcher.Dispatch(new DelegationActions.ProtectDependentSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error protecting dependent, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.ProtectDependentFailAction(error));
            }
        }

        [EffectMethod(typeof(DelegationActions.UnprotectDependentAction))]
        public async Task HandleUnprotectDependentAction(IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Unprotecting dependent");
            try
            {
                string? dependentHdid = this.DelegationState.Value.Dependent?.Hdid;
                if (dependentHdid == null)
                {
                    RequestError error = new() { Message = "Dependent HDID is null" };
                    dispatcher.Dispatch(new DelegationActions.UnprotectDependentFailAction(error));
                    return;
                }

                await this.Api.UnprotectDependentAsync(dependentHdid).ConfigureAwait(true);
                this.Logger.LogInformation("Dependent unprotected successfully");
                dispatcher.Dispatch(new DelegationActions.UnprotectDependentSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error unprotecting dependent, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.UnprotectDependentFailAction(error));
            }
        }
    }
}
