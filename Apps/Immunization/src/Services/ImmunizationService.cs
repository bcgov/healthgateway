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
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.MapUtils;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly IImmunizationDelegate immunizationDelegate;
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="immunizationDelegate">The factory to create immunization delegates.</param>
        /// <param name="autoMapper">The inject automapper provider.</param>
        public ImmunizationService(IImmunizationDelegate immunizationDelegate, IMapper autoMapper)
        {
            this.immunizationDelegate = immunizationDelegate;
            this.autoMapper = autoMapper;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunization(string immunizationId)
        {
            RequestResult<PhsaResult<ImmunizationViewResponse>> delegateResult = await this.immunizationDelegate.GetImmunization(immunizationId).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = this.autoMapper.Map<ImmunizationEvent>(delegateResult.ResourcePayload!.Result),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationEvent>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationResult>> GetImmunizations(string hdid)
        {
            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = await this.immunizationDelegate.GetImmunizations(hdid).ConfigureAwait(true);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = new ImmunizationResult(
                        this.autoMapper.Map<LoadStateModel>(delegateResult.ResourcePayload!.LoadState),
                        this.autoMapper.Map<IList<ImmunizationEvent>>(delegateResult.ResourcePayload!.Result!.ImmunizationViews),
                        ImmunizationRecommendationMapUtils.FromPHSAModelList(delegateResult.ResourcePayload.Result.Recommendations, this.autoMapper)),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }
            else
            {
                return new RequestResult<ImmunizationResult>()
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
            }
        }
    }
}
