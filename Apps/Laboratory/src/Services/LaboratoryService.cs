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
    using System.Collections.Generic;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Delegates;
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
        /// <param name="laboratoryDelegate">The laboratory delegates.</param>
        public LaboratoryService(
            ILogger<LaboratoryService> logger,
            IHttpContextAccessor httpAccessor,
            ILaboratoryDelegate laboratoryDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.laboratoryDelegate = laboratoryDelegate;
        }

        /// <inheritdoc/>
        public RequestResult<IEnumerable<LaboratoryResult>> GetLaboratory(string hdid)
        {
            IEnumerable<LaboratoryResult> result = this.laboratoryDelegate.GetLaboratoryData();
            return new RequestResult<IEnumerable<LaboratoryResult>>()
            {
                ResourcePayload = result,
                ResultStatus = Common.Constants.ResultType.Success,
            };
        }
    }
}