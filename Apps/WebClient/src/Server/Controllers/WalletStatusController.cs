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
namespace HealthGateway.WebClient.Controllers
{
    using HealthGateway.Common.Filters;
    using HealthGateway.WebClient.Services;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Provides a web hook for Aca-Py to receive status updates on Wallnet Connections and Credentials.
    /// </summary>
    [IgnoreAudit]
    [TypeFilter(typeof(AvailabilityFilter))]
    public class WalletStatusController : Controller
    {
        private readonly IConfigurationService configservice;
        private readonly IWalletStatusService walletStatusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletStatusController"/> class.
        /// </summary>
        /// <param name="configService">The injected configuration service provider.</param>
        /// <param name="walletStatusService">The injected wallet status service provider.</param>
        public WalletStatusController(IConfigurationService configService, IWalletStatusService walletStatusService)
        {
            this.configservice = configService;
            this.walletStatusService = walletStatusService;
        }
    }
}
