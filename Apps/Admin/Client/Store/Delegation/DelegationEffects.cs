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
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Services;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class DelegationEffects(ILogger<DelegationEffects> logger, IDelegationApi api, IAdminClientMappingService mappingService, IState<DelegationState> delegationState)
    {
        [EffectMethod]
        public async Task HandleSearchAction(DelegationActions.SearchAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Retrieving delegation info");
            try
            {
                DelegationInfo response = await api.GetDelegationInformationAsync(action.Phn);
                logger.LogInformation("Delegation info retrieved successfully");
                dispatcher.Dispatch(
                    new DelegationActions.SearchSuccessAction
                    {
                        Dependent = response.Dependent,
                        AgentActions = response.AgentActions,
                        Delegates = response.Delegates.Select(mappingService.MapToExtendedDelegateInfo).ToList(),
                    });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = e switch
                {
                    ApiException { StatusCode: HttpStatusCode.BadRequest } => new RequestError { Message = "This feature is only available for users 11 and under." },
                    _ => StoreUtility.FormatRequestError(e),
                };

                logger.LogError(e, "Error retrieving delegation info, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.SearchFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandleDelegateSearchAction(DelegationActions.DelegateSearchAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Retrieving delegate info");
            try
            {
                DelegateInfo response = await api.GetDelegateInformationAsync(action.Phn);
                logger.LogInformation("Delegate info retrieved successfully");
                dispatcher.Dispatch(new DelegationActions.DelegateSearchSuccessAction { Data = mappingService.MapToExtendedDelegateInfo(response) });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = e switch
                {
                    ApiException { StatusCode: HttpStatusCode.BadRequest or HttpStatusCode.NotFound } => new RequestError { Message = "Unable to find user." },
                    _ => StoreUtility.FormatRequestError(e),
                };

                logger.LogError(e, "Error retrieving delegate info, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.DelegateSearchFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandleProtectDependentAction(DelegationActions.ProtectDependentAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Protect dependent");
            try
            {
                string? dependentHdid = delegationState.Value.Dependent?.Hdid;
                if (dependentHdid == null)
                {
                    RequestError error = new() { Message = "Dependent HDID is null" };
                    dispatcher.Dispatch(new DelegationActions.ProtectDependentFailureAction { Error = error });
                    return;
                }

                IEnumerable<string> delegateHdids = delegationState.Value.Delegates
                    .Where(d => d.StagedDelegationStatus is DelegationStatus.Added or DelegationStatus.Allowed)
                    .Select(x => x.Hdid);

                ProtectDependentRequest protectDependentRequest = new(delegateHdids, action.Reason);

                AgentAction change = await api.ProtectDependentAsync(dependentHdid, protectDependentRequest);
                logger.LogInformation("Dependent protected successfully");
                dispatcher.Dispatch(new DelegationActions.ProtectDependentSuccessAction { AgentAction = change });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                logger.LogError("Error protecting dependent, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.ProtectDependentFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandleUnprotectDependentAction(DelegationActions.UnprotectDependentAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Unprotecting dependent");
            try
            {
                string? dependentHdid = delegationState.Value.Dependent?.Hdid;
                if (dependentHdid == null)
                {
                    RequestError error = new() { Message = "Dependent HDID is null" };
                    dispatcher.Dispatch(new DelegationActions.UnprotectDependentFailureAction { Error = error });
                    return;
                }

                UnprotectDependentRequest unprotectDependentRequest = new(action.Reason);

                AgentAction change = await api.UnprotectDependentAsync(dependentHdid, unprotectDependentRequest);
                logger.LogInformation("Dependent unprotected successfully");
                dispatcher.Dispatch(new DelegationActions.UnprotectDependentSuccessAction { AgentAction = change });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                logger.LogError("Error unprotecting dependent, reason: {Exception}", e.ToString());
                dispatcher.Dispatch(new DelegationActions.UnprotectDependentFailureAction { Error = error });
            }
        }
    }
}
