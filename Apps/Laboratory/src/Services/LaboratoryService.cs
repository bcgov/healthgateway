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
namespace HealthGateway.Laboratory.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILaboratoryDelegate laboratoryDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        public LaboratoryService(
            ILogger<LaboratoryService> logger,
            IHttpContextAccessor httpAccessor,
            ILaboratoryDelegateFactory laboratoryDelegateFactory)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryOrder>>> GetLaboratoryOrders(string bearerToken, int pageIndex = 0)
        {
            return await this.laboratoryDelegate.GetLaboratoryOrders(bearerToken, pageIndex).ConfigureAwait(true);
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string bearerToken)
        {
            return await this.laboratoryDelegate.GetLabReport(id, bearerToken).ConfigureAwait(true);
        }
    }
}