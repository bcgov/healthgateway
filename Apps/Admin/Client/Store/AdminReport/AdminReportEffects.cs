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

namespace HealthGateway.Admin.Client.Store.AdminReport
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Fluxor;
    using HealthGateway.Admin.Client.Api;
    using HealthGateway.Admin.Client.Utils;
    using HealthGateway.Admin.Common.Models.AdminReports;
    using Microsoft.Extensions.Logging;
    using Refit;

    public class AdminReportEffects(ILogger<AdminReportEffects> logger, IAdminReportApi adminReportApi)
    {
        [EffectMethod(typeof(AdminReportActions.GetBlockedAccessAction))]
        public async Task HandleGetBlockedAccessAction(IDispatcher dispatcher)
        {
            logger.LogInformation("Retrieving users with blocked data sources");
            try
            {
                IEnumerable<BlockedAccessRecord> report = await adminReportApi.GetBlockedAccessReportAsync();
                dispatcher.Dispatch(new AdminReportActions.GetBlockedAccessSuccessAction { Data = report });
            }
            catch (Exception e) when (e is ApiException or HttpRequestException)
            {
                logger.LogError(e, "Error retrieving users with blocked data sources: {Message}", e.Message);
                RequestError error = StoreUtility.FormatRequestError(e);
                dispatcher.Dispatch(new AdminReportActions.GetProtectedDependentsFailureAction { Error = error });
            }
        }
    }
}
