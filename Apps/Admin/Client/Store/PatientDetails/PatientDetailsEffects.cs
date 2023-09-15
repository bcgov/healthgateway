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
            this.Logger.LogInformation("Loading patient details");

            try
            {
                PatientSupportDetails response = await this.SupportApi.GetPatientSupportDetailsAsync(action.Hdid).ConfigureAwait(true);
                this.Logger.LogInformation("Patient details loaded successfully!");
                dispatcher.Dispatch(new PatientDetailsActions.LoadSuccessAction(response, action.Hdid));
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading patient details...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading patient details, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientDetailsActions.LoadFailAction(error));
            }
        }

        [EffectMethod]
        public async Task HandleBlockAccessAction(PatientDetailsActions.BlockAccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Blocking access");
            try
            {
                BlockAccessRequest request = new(action.DataSources, action.Reason);
                await this.SupportApi.BlockAccessAsync(action.Hdid, request).ConfigureAwait(true);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessSuccessAction(action.Hdid));
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error blocking access: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction(error));
            }
        }

        [EffectMethod]
        public Task HandleBlockSuccessAction(PatientDetailsActions.BlockAccessSuccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Reload the patient's data for details page");
            dispatcher.Dispatch(new PatientDetailsActions.LoadAction(action.Hdid));
            return Task.CompletedTask;
        }

        [EffectMethod]
        public async Task HandleSubmitCovid19TreatmentAssessmentAction(PatientDetailsActions.SubmitCovid19TreatmentAssessmentAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Submitting COVID-19 treatment assessment");
            try
            {
                await this.SupportApi.SubmitCovidAssessment(action.Request).ConfigureAwait(true);
                dispatcher.Dispatch(new PatientDetailsActions.SubmitCovid19TreatmentAssessmentSuccessAction { Hdid = action.Hdid });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error submitting COVID-19 treatment assessment: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.SubmitCovid19TreatmentAssessmentFailAction(error));
            }
        }

        [EffectMethod]
        public Task HandleSubmitCovid19TreatmentAssessmentSuccessAction(PatientDetailsActions.SubmitCovid19TreatmentAssessmentSuccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Reload the patient's data for details page");
            dispatcher.Dispatch(new PatientDetailsActions.LoadAction(action.Hdid));
            return Task.CompletedTask;
        }
    }
}
