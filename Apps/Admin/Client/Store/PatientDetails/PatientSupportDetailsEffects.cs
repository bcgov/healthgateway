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
    using HealthGateway.Admin.Common.Models;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class PatientSupportDetailsEffects
    {
        public PatientSupportDetailsEffects(ILogger<PatientSupportDetailsEffects> logger, ISupportApi supportApi)
        {
            this.Logger = logger;
            this.SupportApi = supportApi;
        }

        [Inject]
        private ILogger<PatientSupportDetailsEffects> Logger { get; set; }

        [Inject]
        private ISupportApi SupportApi { get; set; }

        [EffectMethod]
        public async Task HandleLoadAction(PatientSupportDetailsActions.LoadAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Loading messaging verifications");

            try
            {
                PatientSupportDetails response = await this.SupportApi.GetPatientSupportDetailsAsync(action.Hdid).ConfigureAwait(true);
                this.Logger.LogInformation("Messaging verifications loaded successfully!");
                dispatcher.Dispatch(new PatientSupportDetailsActions.LoadSuccessAction(response, action.Hdid));
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error loading messaging verifications...{Error}", e);
                RequestError error = StoreUtility.FormatRequestError(e);
                this.Logger.LogError("Error loading messaging verifications, reason: {ErrorMessage}", error.Message);
                dispatcher.Dispatch(new PatientSupportDetailsActions.LoadFailAction(error));
            }
        }
    }
}
