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

namespace HealthGateway.Admin.Client.Store.PatientSupport
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class PatientSupportEffects
    {
        public PatientSupportEffects(ILogger<PatientSupportEffects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<PatientSupportEffects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        [EffectMethod]
        public async Task HandleLoadAction(PatientSupportActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading patients!");

            try
            {
                IList<PatientSupportResult> response = await this.SupportApi.GetPatientsAsync(action.QueryType, action.QueryString).ConfigureAwait(true);
                this.Logger.LogInformation("Patients loaded successfully!");
                dispatcher.Dispatch(new PatientSupportActions.LoadSuccessAction { Data = response, ShouldNavigateToPatientDetails = action.ShouldNavigateToPatientDetails });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading patients, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientSupportActions.LoadFailureAction { Error = error });
            }
        }
    }
}
