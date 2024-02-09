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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Utils;
    using HealthGateway.Common.Data.ViewModels;
    using HealthGateway.Common.Models.Immunization;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.MapUtils;
    using HealthGateway.Immunization.Models;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly IImmunizationDelegate immunizationDelegate;
        private readonly IPatientRepository patientRepository;
        private readonly IMapper autoMapper;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="immunizationDelegate">The injected immunization delegate.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="autoMapper">The inject automapper.</param>
        /// <param name="configuration">The injected configuration.</param>
        public ImmunizationService(IImmunizationDelegate immunizationDelegate, IPatientRepository patientRepository, IMapper autoMapper, IConfiguration configuration)
        {
            this.immunizationDelegate = immunizationDelegate;
            this.patientRepository = patientRepository;
            this.autoMapper = autoMapper;
            this.configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationEvent>> GetImmunizationAsync(string immunizationId, CancellationToken ct = default)
        {
            RequestResult<PhsaResult<ImmunizationViewResponse>> delegateResult = await this.immunizationDelegate.GetImmunizationAsync(immunizationId, ct);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationEvent>
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = ImmunizationEventMapUtils.ToUiModel(delegateResult.ResourcePayload!.Result, this.autoMapper, DateFormatter.GetLocalTimeZone(this.configuration)),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }

            return new RequestResult<ImmunizationEvent>
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };
        }

        /// <inheritdoc/>
        public async Task<RequestResult<ImmunizationResult>> GetImmunizationsAsync(string hdid, CancellationToken ct = default)
        {
            if (!await this.patientRepository.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
            {
                return new RequestResult<ImmunizationResult>
                {
                    ResultStatus = ResultType.Success,
                    ResourcePayload = new ImmunizationResult(),
                    TotalResultCount = 0,
                };
            }

            RequestResult<PhsaResult<ImmunizationResponse>> delegateResult = await this.immunizationDelegate.GetImmunizationsAsync(hdid, ct);
            if (delegateResult.ResultStatus == ResultType.Success)
            {
                return new RequestResult<ImmunizationResult>
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = new ImmunizationResult(
                        this.autoMapper.Map<LoadStateModel>(delegateResult.ResourcePayload!.LoadState),
                        ImmunizationEventMapUtils.ToUiModels(delegateResult.ResourcePayload!.Result!.ImmunizationViews, this.autoMapper, DateFormatter.GetLocalTimeZone(this.configuration)),
                        ImmunizationRecommendationMapUtils.FromPhsaModelList(delegateResult.ResourcePayload.Result.Recommendations, this.autoMapper)),
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                };
            }

            return new RequestResult<ImmunizationResult>
            {
                ResultStatus = delegateResult.ResultStatus,
                ResultError = delegateResult.ResultError,
            };
        }
    }
}
