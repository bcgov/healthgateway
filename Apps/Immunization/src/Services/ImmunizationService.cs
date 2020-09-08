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
namespace HealthGateway.Immunization.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImmunizationDelegate immunizationDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="httpAccessor">The injected http context accessor provider.</param>
        /// <param name="immunizationDelegate">The factory to create immunization delegates.</param>
        public ImmunizationService(
            ILogger<ImmunizationService> logger,
            IHttpContextAccessor httpAccessor,
            IImmunizationDelegate immunizationDelegate)
        {
            this.logger = logger;
            this.httpContextAccessor = httpAccessor;
            this.immunizationDelegate = immunizationDelegate;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<ImmunizationModel>>> GetImmunizations(string bearerToken, int pageIndex = 0)
        {
            RequestResult<IEnumerable<ImmunizationResponse>> delegateResult = await this.immunizationDelegate.GetImmunizations(bearerToken, pageIndex).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<IEnumerable<ImmunizationModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = ImmunizationModel.FromPHSAModelList(delegateResult.ResourcePayload),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<IEnumerable<ImmunizationModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }
    }
}
