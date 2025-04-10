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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Immunization.Delegates;
    using HealthGateway.Immunization.Models;

    /// <summary>
    /// The Immunization data service.
    /// </summary>
    public class ImmunizationService : IImmunizationService
    {
        private readonly IImmunizationDelegate immunizationDelegate;
        private readonly IPatientRepository patientRepository;
        private readonly IImmunizationMappingService mappingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmunizationService"/> class.
        /// </summary>
        /// <param name="immunizationDelegate">The injected immunization delegate.</param>
        /// <param name="patientRepository">The injected patient repository.</param>
        /// <param name="mappingService">The injected mapping service.</param>
        public ImmunizationService(IImmunizationDelegate immunizationDelegate, IPatientRepository patientRepository, IImmunizationMappingService mappingService)
        {
            this.immunizationDelegate = immunizationDelegate;
            this.patientRepository = patientRepository;
            this.mappingService = mappingService;
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

            return delegateResult.ResultStatus == ResultType.Success
                ? new RequestResult<ImmunizationResult>
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResourcePayload = new ImmunizationResult
                    {
                        LoadState = this.mappingService.MapToLoadStateModel(delegateResult.ResourcePayload!.LoadState),
                        Immunizations = delegateResult.ResourcePayload!.Result!.ImmunizationViews.Select(this.mappingService.MapToImmunizationEvent).ToList(),
                        Recommendations = this.mappingService.MapToImmunizationRecommendations(delegateResult.ResourcePayload.Result.Recommendations),
                    },
                    PageIndex = delegateResult.PageIndex,
                    PageSize = delegateResult.PageSize,
                    TotalResultCount = delegateResult.TotalResultCount,
                }
                : new RequestResult<ImmunizationResult>
                {
                    ResultStatus = delegateResult.ResultStatus,
                    ResultError = delegateResult.ResultError,
                };
        }
    }
}
