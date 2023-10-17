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
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Constants;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class PatientDetailsEffects
    {
        public PatientDetailsEffects(ILogger<PatientDetailsEffects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        private ILogger<PatientDetailsEffects> Logger { get; }

        private ISupportApi SupportApi { get; }

        [EffectMethod]
        public async Task HandleLoadAction(PatientDetailsActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading patient details");

            try
            {
                PatientSupportDetails response = await this.SupportApi.GetPatientSupportDetailsAsync(action.QueryType, action.QueryString, action.RefreshVaccineDetails).ConfigureAwait(true);
                this.Logger.LogInformation("Patient details loaded successfully!");
                dispatcher.Dispatch(new PatientDetailsActions.LoadSuccessAction { Data = response });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading patient details...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading patient details, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientDetailsActions.LoadFailureAction { Error = error });
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
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessSuccessAction { Hdid = action.Hdid });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error blocking access: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.BlockAccessFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public Task HandleBlockSuccessAction(PatientDetailsActions.BlockAccessSuccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Reload the patient's data for details page");
            dispatcher.Dispatch(
                new PatientDetailsActions.LoadAction
                {
                    QueryType = ClientRegistryType.Hdid,
                    QueryString = action.Hdid,
                    RefreshVaccineDetails = false,
                });
            return Task.CompletedTask;
        }

        [EffectMethod]
        public async Task HandleSubmitCovid19TreatmentAssessmentAction(PatientDetailsActions.SubmitCovid19TreatmentAssessmentAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Submitting COVID-19 treatment assessment");
            try
            {
                await this.SupportApi.SubmitCovidAssessment(action.Request).ConfigureAwait(true);
                dispatcher.Dispatch(new PatientDetailsActions.SubmitCovid19TreatmentAssessmentSuccessAction { Phn = action.Phn });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error submitting COVID-19 treatment assessment: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new PatientDetailsActions.SubmitCovid19TreatmentAssessmentFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public Task HandleSubmitCovid19TreatmentAssessmentSuccessAction(PatientDetailsActions.SubmitCovid19TreatmentAssessmentSuccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Reload the patient's data for details page");
            dispatcher.Dispatch(
                new PatientDetailsActions.LoadAction
                {
                    QueryType = ClientRegistryType.Phn,
                    QueryString = action.Phn,
                    RefreshVaccineDetails = false,
                });
            return Task.CompletedTask;
        }
    }
}
