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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthGateway.Common.Services;
using HealthGateway.PatientDataAccess;

namespace HealthGateway.Patient.Services
{
    internal class PatientDataService : IPatientDataService
    {
        private readonly IPatientDataRepository patientDataRepository;
        private readonly IPersonalAccountsService personalAccountsService;

        private async Task<Guid> ResolvePidFromHdid(string patientHdid) =>
            (await personalAccountsService.GetPatientAccountAsync(patientHdid)).PatientIdentity.Pid;

        public PatientDataService(IPatientDataRepository patientDataRepository, IPersonalAccountsService personalAccountsService)
        {
            this.patientDataRepository = patientDataRepository;
            this.personalAccountsService = personalAccountsService;
        }

        public async Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct)
        {
            var pid = await ResolvePidFromHdid(query.Hdid);
            var results = await patientDataRepository.Query(
                new HealthServicesQuery(pid, query.PatientDataTypes.Select(MapToHealthServiceCategory)),
                ct);

            return new PatientDataResponse(results.Items.Select(MapToPatientData).ToArray());
        }

        public async Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct)
        {
            var pid = await ResolvePidFromHdid(query.Hdid);
            var file = (await patientDataRepository.Query(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct)).Items.FirstOrDefault() as PatientFile;

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        private PatientData MapToPatientData(HealthData healthData) =>
            healthData switch
            {
                OrganDonorRegistration hd => new OrganDonorRegistrationData(hd.Status.ToString(), hd.StatusMessage, hd.RegistrationFileId),

                _ => throw new NotImplementedException($"{healthData.GetType().Name} is not mapped to {nameof(PatientData)}")
            };

        private HealthServiceCategory MapToHealthServiceCategory(PatientDataType patientDataType) =>
            patientDataType switch
            {
                PatientDataType.OrganDonorRegistrationStatus => HealthServiceCategory.OrganDonor,

                _ => throw new NotImplementedException($"{patientDataType} is not mapped to {nameof(HealthServiceCategory)}")
            };
    }
}
