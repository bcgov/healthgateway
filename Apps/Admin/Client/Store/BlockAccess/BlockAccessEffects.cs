// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Admin.Client.Store.BlockAccess
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Store.PatientDetails;
    using HealthGateway.Admin.Client.Store.PatientSupport;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

#pragma warning disable CS1591, SA1600
    public class BlockAccessEffects
    {
        public BlockAccessEffects(ISupportApi api, ILogger<BlockAccessEffects> logger, IState<PatientSupportState> patientSupportState)
        {
            this.Api = api;
            this.Logger = logger;
            this.PatientSupportState = patientSupportState;
        }

        private ISupportApi Api { get; }

        private ILogger<BlockAccessEffects> Logger { get; }

        private IState<PatientSupportState> PatientSupportState { get; }

        [EffectMethod]
        public async Task HandleSetBlockAccessAction(BlockAccessActions.SetBlockAccessAction action, IDispatcher dispatcher)
        {
            this.Logger.LogInformation("Setting block access");
            try
            {
                string? patientHdid = this.PatientSupportState.Value.Result?[0].Hdid;
                if (patientHdid == null)
                {
                    RequestError error = new() { Message = "No patient selected" };
                    dispatcher.Dispatch(new BlockAccessActions.SetBlockAccessFailureAction(error));
                    return;
                }

                BlockAccessRequest request = new(action.DataSources, action.Reason);
                await this.Api.BlockAccessAsync(patientHdid, request).ConfigureAwait(true);
                dispatcher.Dispatch(new BlockAccessActions.SetBlockAccessSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                this.Logger.LogError("Error setting block access: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new BlockAccessActions.SetBlockAccessFailureAction(error));
            }
        }

        [EffectMethod(typeof(BlockAccessActions.SetBlockAccessSuccessAction))]
        public Task HandleSetBlockSuccessAction(IDispatcher dispatcher)
        {
            string? patientHdid = this.PatientSupportState.Value.Result?[0].Hdid;
            if (patientHdid == null)
            {
                RequestError error = new() { Message = "No patient selected" };
                dispatcher.Dispatch(new BlockAccessActions.SetBlockAccessFailureAction(error));
                return Task.CompletedTask;
            }

            // Reload the patient's data for details page.
            dispatcher.Dispatch(new PatientSupportDetailsActions.LoadAction(patientHdid));
            return Task.CompletedTask;
        }
    }
}
