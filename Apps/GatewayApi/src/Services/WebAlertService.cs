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
namespace HealthGateway.GatewayApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Services;
    using HealthGateway.GatewayApi.Api;
    using HealthGateway.GatewayApi.Models;
    using HealthGateway.GatewayApi.Models.Phsa;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service to interact with the PHSA web alert API.
    /// </summary>
    public class WebAlertService : IWebAlertService
    {
        private readonly ILogger<WebAlertService> logger;
        private readonly IPersonalAccountsService personalAccountsService;
        private readonly IWebAlertApi webAlertApi;
        private readonly IGatewayApiMappingService mappingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAlertService"/> class.
        /// </summary>
        /// <param name="logger">The injected logger.</param>
        /// <param name="personalAccountsService">The injected personal accounts service.</param>
        /// <param name="webAlertApi">The injected web alert Refit API.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public WebAlertService(
            ILogger<WebAlertService> logger,
            IPersonalAccountsService personalAccountsService,
            IWebAlertApi webAlertApi,
            IGatewayApiMappingService mappingService)
        {
            this.logger = logger;
            this.personalAccountsService = personalAccountsService;
            this.webAlertApi = webAlertApi;
            this.mappingService = mappingService;
        }

        /// <inheritdoc/>
        public async Task<IList<WebAlert>> GetWebAlertsAsync(string hdid, CancellationToken ct = default)
        {
            Guid pid = await this.GetPersonalAccountPidByHdidAsync(hdid, ct) ?? throw new InvalidOperationException($"No pid found for hdid {hdid}");

            this.logger.LogDebug("Retrieving web alerts from PHSA");
            IList<PhsaWebAlert> phsaWebAlerts = await this.webAlertApi.GetWebAlertsAsync(pid, ct);
            IList<WebAlert> webAlerts = phsaWebAlerts
                .Where(a => a.ExpirationDateTimeUtc > DateTime.UtcNow && a.ScheduledDateTimeUtc < DateTime.UtcNow)
                .OrderByDescending(a => a.ScheduledDateTimeUtc)
                .Select(this.mappingService.MapToWebAlert)
                .ToList();

            return webAlerts;
        }

        /// <inheritdoc/>
        public async Task DismissWebAlertsAsync(string hdid, CancellationToken ct = default)
        {
            Guid pid = await this.GetPersonalAccountPidByHdidAsync(hdid, ct) ?? throw new InvalidOperationException($"No pid found for hdid {hdid}");

            this.logger.LogDebug("Sending request to PHSA to dismiss web alerts");
            await this.webAlertApi.DeleteWebAlertsAsync(pid, ct);
        }

        /// <inheritdoc/>
        public async Task DismissWebAlertAsync(string hdid, Guid webAlertId, CancellationToken ct = default)
        {
            Guid pid = await this.GetPersonalAccountPidByHdidAsync(hdid, ct) ?? throw new InvalidOperationException($"No pid found for hdid {hdid}");

            this.logger.LogDebug("Sending request to PHSA to dismiss web alert {WebAlertId}", webAlertId);
            await this.webAlertApi.DeleteWebAlertAsync(pid, webAlertId, ct);
        }

        private async Task<Guid?> GetPersonalAccountPidByHdidAsync(string hdid, CancellationToken ct)
        {
            return (await this.personalAccountsService.GetPersonalAccountAsync(hdid, ct)).PatientIdentity.Pid;
        }
    }
}
