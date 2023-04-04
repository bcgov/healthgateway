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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Services;
    using HealthGateway.PatientDataAccess;

    internal class PatientDataService : IPatientDataService
    {
        private readonly IPatientDataRepository patientDataRepository;
        private readonly IPersonalAccountsService personalAccountsService;
        private readonly IMapper mapper;

        public PatientDataService(IPatientDataRepository patientDataRepository, IPersonalAccountsService personalAccountsService, IMapper mapper)
        {
            this.patientDataRepository = patientDataRepository;
            this.personalAccountsService = personalAccountsService;
            this.mapper = mapper;
        }

        public async Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            HealthQuery healthQuery = new(pid, query.PatientDataTypes.Select(t => this.mapper.Map<HealthCategory>(t)));
            PatientDataQueryResult result = await this.patientDataRepository.Query(healthQuery, ct)
                .ConfigureAwait(true);
            return new PatientDataResponse(result.Items.Select(i => this.mapper.Map<PatientData>(i)));
        }

        public async Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientFile? file = (await this.patientDataRepository.Query(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct).ConfigureAwait(true)).Items.FirstOrDefault() as PatientFile;

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        private async Task<Guid> ResolvePidFromHdid(string patientHdid)
        {
            return (await this.personalAccountsService.GetPatientAccountAsync(patientHdid).ConfigureAwait(true)).PatientIdentity.Pid;
        }
    }
#pragma warning restore SA1600
}
