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
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class PatientDetailsEffects
    {
        public PatientDetailsEffects(ILogger<PatientDetailsEffects> logger, ISupportApi supportApi, IState<PatientSupportState> patientSupportState)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
            this.PatientSupportState = patientSupportState;
        }

        [Inject]
        private ILogger<PatientDetailsEffects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        [Inject]
        private IState<PatientSupportState> PatientSupportState { get; set; }

        [EffectMethod]
        public async Task HandleLoadAction(PatientDetailsActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading messaging verifications");

            try
            {
                PatientSupportDetails response = await this.SupportApi.GetPatientSupportDetailsAsync(action.Hdid).ConfigureAwait(true);
                this.Logger.LogInformation("Messaging verifications loaded successfully!");
                dispatcher.Dispatch(new PatientDetailsActions.LoadSuccessAction(response, action.Hdid));
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading messaging verifications...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading messaging verifications, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientDetailsActions.LoadFailAction(error));
            }
        }

        [EffectMethod]
        public async Task HandleBlockAccessAction(PatientDetailsActions.BlockAccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Setting block access");
            try
            {
                string? patientHdid = this.PatientSupportState.Value.Result?[0].Hdid;
                if (patientHdid == null)
                {
                    RequestError error = new() { Message = "No patient selected" };
                    dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction(error));
                    return;
                }

                BlockAccessRequest request = new(action.DataSources, action.Reason);
                await this.SupportApi.BlockAccessAsync(patientHdid, request).ConfigureAwait(true);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error setting block access: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction(error));
            }
        }

        [EffectMethod(typeof(PatientDetailsActions.BlockAccessSuccessAction))]
        public Task HandleBlockSuccessAction(IDispatcher dispatcher)
        {
            string? patientHdid = this.PatientSupportState.Value.Result?[0].Hdid;
            if (patientHdid == null)
            {
                RequestError error = new() { Message = "No patient selected" };
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction(error));
                return Task.CompletedTask;
            }

            // Reload the patient's data for details page.
            dispatcher.Dispatch(new PatientDetailsActions.LoadAction(patientHdid));
            return Task.CompletedTask;
        }
    }
}
