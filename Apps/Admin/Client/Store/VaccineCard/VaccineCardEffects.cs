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

namespace HealthGateway.Admin.Client.Store.VaccineCard
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models.CovidSupport;
    using HealthGateway.Common.Data.Models;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class VaccineCardEffects(ILogger<VaccineCardEffects> logger, ISupportApi supportApi)
    {
        [EffectMethod]
        public async Task HandleMailVaccineCardAction(VaccineCardActions.MailVaccineCardAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Request to mail vaccine card");
            try
            {
                MailDocumentRequest request = new()
                {
                    PersonalHealthNumber = action.Phn,
                    MailAddress = action.MailAddress,
                };

                await supportApi.MailVaccineCard(request).ConfigureAwait(true);
                dispatcher.Dispatch(new VaccineCardActions.MailVaccineCardSuccessAction());
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError("Error requesting to mail vaccine card: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new VaccineCardActions.MailVaccineCardFailureAction { Error = error });
            }
        }

        [EffectMethod]
        public async Task HandlePrintVaccineCardAction(VaccineCardActions.PrintVaccineCardAction action, IDispatcher dispatcher)
        {
            logger.LogInformation("Retrieving vaccine card");
            try
            {
                ReportModel report = await supportApi.RetrieveVaccineRecord(action.Phn).ConfigureAwait(true);
                dispatcher.Dispatch(new VaccineCardActions.PrintVaccineCardSuccessAction { Data = report });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError("Error retrieving vaccine card record: {Exception}", e.ToString());
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new VaccineCardActions.PrintVaccineCardFailureAction { Error = error });
            }
        }
    }
}
