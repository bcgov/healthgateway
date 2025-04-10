// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------
namespace HealthGateway.Patient.Services
{
#pragma warning disable SA1600 // Disable documentation for internal class.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.AccountDataAccess.Patient;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Constants;
    using HealthGateway.PatientDataAccess;

    internal class PatientDataService : IPatientDataService
    {
        private readonly IPatientDataRepository patientDataRepository;
        private readonly IPatientRepository patientRepository;
        private readonly IPersonalAccountsService personalAccountsService;
        private readonly IPatientMappingService mappingService;

        public PatientDataService(
            IPatientDataRepository patientDataRepository,
            IPatientRepository patientRepository,
            IPersonalAccountsService personalAccountsService,
            IPatientMappingService mappingService)
        {
            this.patientDataRepository = patientDataRepository;
            this.patientRepository = patientRepository;
            this.personalAccountsService = personalAccountsService;
            this.mappingService = mappingService;
        }

        public async Task<PatientDataResponse> QueryAsync(PatientDataQuery query, CancellationToken ct = default)
        {
            IList<PatientDataType> unblockedPatientDataTypes = await this.GetUnblockedPatientDataTypesAsync(query.Hdid, query.PatientDataTypes, ct);
            if (!unblockedPatientDataTypes.Any())
            {
                return new PatientDataResponse([]);
            }

            Guid pid = await this.ResolvePidFromHdidAsync(query.Hdid, ct);
            HealthQuery healthQuery = new(pid, unblockedPatientDataTypes.Select(this.mappingService.MapToHealthCategory));
            PatientDataQueryResult result = await this.patientDataRepository.QueryAsync(healthQuery, ct);
            return new PatientDataResponse(result.Items.Select(this.mappingService.MapToPatientData));
        }

        public async Task<PatientFileResponse?> QueryAsync(PatientFileQuery query, CancellationToken ct = default)
        {
            Guid pid = await this.ResolvePidFromHdidAsync(query.Hdid, ct);
            PatientFile? file = (await this.patientDataRepository.QueryAsync(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct)).Items.OfType<PatientFile>()
                .FirstOrDefault();

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        private async Task<IList<PatientDataType>> GetUnblockedPatientDataTypesAsync(string hdid, IEnumerable<PatientDataType> patientDataTypes, CancellationToken ct)
        {
            List<PatientDataType> unblockedPatientDataTypes = [];
            foreach (PatientDataType patientDataType in patientDataTypes)
            {
                DataSource dataSource = this.mappingService.MapToDataSource(patientDataType);
                if (await this.patientRepository.CanAccessDataSourceAsync(hdid, dataSource, ct))
                {
                    unblockedPatientDataTypes.Add(patientDataType);
                }
            }

            return unblockedPatientDataTypes;
        }

        private async Task<Guid> ResolvePidFromHdidAsync(string patientHdid, CancellationToken ct)
        {
            return (await this.personalAccountsService.GetPersonalAccountAsync(patientHdid, ct)).PatientIdentity.Pid;
        }
    }
#pragma warning restore SA1600
}
