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
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Laboratory.Delegates;
    using HealthGateway.Laboratory.Factories;
    using HealthGateway.Laboratory.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <inheritdoc/>
    public class LaboratoryService : ILaboratoryService
    {
        private readonly ILaboratoryDelegate laboratoryDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaboratoryService"/> class.
        /// </summary>
        /// <param name="laboratoryDelegateFactory">The laboratory delegate factory.</param>
        public LaboratoryService(
            ILaboratoryDelegateFactory laboratoryDelegateFactory)
        {
            this.laboratoryDelegate = laboratoryDelegateFactory.CreateInstance();
        }

        /// <inheritdoc/>
        public async Task<RequestResult<IEnumerable<LaboratoryModel>>> GetLaboratoryOrders(string bearerToken, string hdid, int pageIndex = 0)
        {
            RequestResult<IEnumerable<LaboratoryOrder>> delegateResult = await this.laboratoryDelegate.GetLaboratoryOrders(bearerToken, hdid, pageIndex).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<IEnumerable<LaboratoryModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = LaboratoryModel.FromPHSAModelList(delegateResult.ResourcePayload),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<IEnumerable<LaboratoryModel>>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<LaboratoryReport>> GetLabReport(Guid id, string hdid, string bearerToken)
        {
            return await this.laboratoryDelegate.GetLabReport(id, hdid, bearerToken).ConfigureAwait(true);
        }
    }
}