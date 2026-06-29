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
namespace HealthGateway.Encounter.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Models.PHSA;
    using HealthGateway.Common.Services;
    using HealthGateway.Encounter.Models;
    using HealthGateway.PatientDataAccess;
    using PatientDataHospitalVisit = HealthGateway.PatientDataAccess.HospitalVisit;

    /// <inheritdoc/>
    public class EncounterServiceV2(
        IPersonalAccountsService personalAccountsService,
        IPatientDataRepository patientDataRepository,
        IPatientRepository patientRepository,
        IEncounterMappingService mappingService) : IEncounterServiceV2
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<HospitalVisitModel>> GetHospitalVisitsAsync(string hdid, CancellationToken ct = default)
        {
            if (!await patientRepository.CanAccessDataSourceAsync(hdid, DataSource.HospitalVisit, ct))
            {
                return [];
            }

            PersonalAccount account = await personalAccountsService.GetPersonalAccountAsync(hdid, ct);
            HealthQuery healthQuery = new(account.PatientIdentity.Pid, [HealthCategory.HospitalVisits]);
            PatientDataQueryResult result = await patientDataRepository.QueryAsync(healthQuery, ct);
            return result.Items
                .OfType<PatientDataHospitalVisit>()
                .Select(mappingService.MapToHospitalVisitModel)
                .ToList();
        }
    }
}
