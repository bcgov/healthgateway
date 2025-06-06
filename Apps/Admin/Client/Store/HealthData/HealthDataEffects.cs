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
