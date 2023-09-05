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
    using AutoMapper;
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
        private readonly IMapper mapper;

        public PatientDataService(IPatientDataRepository patientDataRepository, IPatientRepository patientRepository, IPersonalAccountsService personalAccountsService, IMapper mapper)
        {
            this.patientDataRepository = patientDataRepository;
            this.patientRepository = patientRepository;
            this.personalAccountsService = personalAccountsService;
            this.mapper = mapper;
        }

        public async Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct)
        {
            IList<PatientDataType> unblockedPatientDataTypes = await this.GetUnblockedPatientDataTypesAsync(query.Hdid, query.PatientDataTypes).ConfigureAwait(true);
            if (!unblockedPatientDataTypes.Any())
            {
                return new PatientDataResponse(Enumerable.Empty<PatientData>());
            }

            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            HealthQuery healthQuery = new(pid, unblockedPatientDataTypes.Select(t => this.mapper.Map<HealthCategory>(t)));
            PatientDataQueryResult result = await this.patientDataRepository.Query(healthQuery, ct)
                .ConfigureAwait(true);
            return new PatientDataResponse(result.Items.Where(Filter).Select(i => this.mapper.Map<PatientData>(i)));
        }

        public async Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientFile? file = (await this.patientDataRepository.Query(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct).ConfigureAwait(true)).Items.OfType<PatientFile>()
                .FirstOrDefault();

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        private static bool Filter(HealthData healthData)
        {
            if (healthData is PatientDataAccess.BcCancerScreening cse)
            {
                return cse.EventType == BcCancerScreeningType.Result;
            }

            return true;
        }

        private async Task<IList<PatientDataType>> GetUnblockedPatientDataTypesAsync(string hdid, IEnumerable<PatientDataType> patientDataTypes)
        {
            List<PatientDataType> unblockedPatientDataTypes = new();
            foreach (PatientDataType patientDataType in patientDataTypes)
            {
                DataSource dataSource = this.mapper.Map<DataSource>(patientDataType);
                if (await this.patientRepository.CanAccessDataSourceAsync(hdid, dataSource).ConfigureAwait(true))
                {
                    unblockedPatientDataTypes.Add(patientDataType);
                }
            }

            return unblockedPatientDataTypes;
        }

        private async Task<Guid> ResolvePidFromHdid(string patientHdid)
        {
            return (await this.personalAccountsService.GetPatientAccountAsync(patientHdid).ConfigureAwait(true)).PatientIdentity.Pid;
        }
    }
#pragma warning restore SA1600
}
