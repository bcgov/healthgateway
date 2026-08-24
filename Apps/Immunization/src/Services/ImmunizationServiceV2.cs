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
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Models.PHSA.Recommendation;
    using HealthGateway.Common.Services;
    using HealthGateway.Immunization.Models;
    using HealthGateway.PatientDataAccess;
    using PatientDataImmunizationRecommendation = HealthGateway.PatientDataAccess.ImmunizationRecommendation;
    using PatientDataImmunizationRecord = HealthGateway.PatientDataAccess.Immunization;

    /// <inheritdoc/>
    public class ImmunizationServiceV2(
        IPersonalAccountsService personalAccountsService,
        IPatientDataRepository patientDataRepository,
        IPatientRepository patientRepository,
        IImmunizationMappingService mappingService,
        IMapper mapper) : IImmunizationServiceV2
    {
        /// <inheritdoc/>
        public async Task<ImmunizationResultV2> GetImmunizationsAsync(string hdid, CancellationToken ct = default)
        {
            if (!await patientRepository.CanAccessDataSourceAsync(hdid, DataSource.Immunization, ct))
            {
                return new ImmunizationResultV2();
            }

            PersonalAccount account = await personalAccountsService.GetPersonalAccountAsync(hdid, ct);
            HealthQuery healthQuery = new(
                account.PatientIdentity.Pid,
                [HealthCategory.Immunization, HealthCategory.ImmunizationRecommendation]);
            PatientDataQueryResult result = await patientDataRepository.QueryAsync(healthQuery, ct);

            return new ImmunizationResultV2
            {
                Immunizations = result.Items
                    .OfType<PatientDataImmunizationRecord>()
                    .Select(mappingService.MapToImmunizationEvent)
                    .ToList(),
                Recommendations = mappingService.MapToImmunizationRecommendations(
                    mapper.Map<IEnumerable<ImmunizationRecommendationResponse>>(
                        result.Items
                            .OfType<PatientDataImmunizationRecommendation>())),
            };
        }
    }
}
