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
namespace HealthGateway.Admin.Client.Store.HealthData
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

    public class HealthDataEffects(ILogger<HealthDataEffects> logger, ISupportApi supportApi)
    {
        [EffectMethod]
        public async Task HandleRefreshDiagnosticImagingCacheAction(HealthDataActions.RefreshDiagnosticImagingCacheAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Request to refresh diagnostic imaging cache");
            try
            {
                HealthDataStatusRequest request = new()
                {
                    Phn = action.Phn,
                    System = SystemSource.DiagnosticImaging,
                };

                await supportApi.RequestHealthDataRefreshAsync(request);
                dispatcher.Dispatch(new HealthDataActions.RefreshDiagnosticImagingCacheSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError(e, "Error requesting to refresh diagnostic imaging cache: {Message}", e.Message);
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new HealthDataActions.RefreshDiagnosticImagingCacheFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandleRefreshLaboratoryCacheAction(HealthDataActions.RefreshLaboratoryCacheAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Request to refresh laboratory cache");
            try
            {
                HealthDataStatusRequest request = new()
                {
                    Phn = action.Phn,
                    System = SystemSource.Laboratory,
                };

                await supportApi.RequestHealthDataRefreshAsync(request);
                dispatcher.Dispatch(new HealthDataActions.RefreshLaboratoryCacheSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError(e, "Error requesting to refresh laboratory cache: {Message}", e.Message);
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new HealthDataActions.RefreshLaboratoryCacheFailureAction { Error = error });
            }
        }
    }
}
