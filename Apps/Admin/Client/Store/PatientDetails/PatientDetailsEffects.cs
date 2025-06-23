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

namespace HealthGateway.Admin.Client.Store.PatientDetails
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Store.Configuration;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class PatientDetailsEffects(ILogger<PatientDetailsEffects> logger, ISupportApi supportApi, IState<ConfigurationState> configurationState)
    {
        [EffectMethod]
        public async Task HandleLoadAction(PatientDetailsActions.LoadAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Loading patient details");

            try
            {
                Dictionary<string, bool>? features = configurationState.Value.Result?.Features;
                bool showApiRegistration = features != null &&
                                           features.TryGetValue("ShowApiRegistration", out bool enabled) &&
                                           enabled;

                PatientSupportDetails response = await supportApi.GetPatientSupportDetailsAsync(action.QueryType, action.QueryString, action.RefreshVaccineDetails, showApiRegistration);
                logger.LogInformation("Patient details loaded successfully!");
                dispatcher.Dispatch(new PatientDetailsActions.LoadSuccessAction { Data = response });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                logger.LogError(e, "Error loading patient details, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientDetailsActions.LoadFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandleBlockAccessAction(PatientDetailsActions.BlockAccessAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Blocking access");
            try
            {
                BlockAccessRequest request = new(action.DataSources, action.Reason);
                await supportApi.BlockAccessAsync(action.Hdid, request);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessSuccessAction { Hdid = action.Hdid });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError(e, "Error blocking access: {Message}", e.Message);
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public Task HandleBlockSuccessAction(PatientDetailsActions.BlockAccessSuccessAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Reload the patient's data for details page");
            dispatcher.Dispatch(
                new PatientDetailsActions.LoadAction
                {
                    QueryType = ClientRegistryType.Hdid,
                    QueryString = action.Hdid,
                    RefreshVaccineDetails = false,
                });
            return Task.CompletedTask;
        }
    }
}
