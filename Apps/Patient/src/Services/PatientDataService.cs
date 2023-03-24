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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.Common.Services;
    using HealthGateway.Patient.Constants;
    using HealthGateway.Patient.Models;
    using HealthGateway.PatientDataAccess;
    using PatientDataQuery = HealthGateway.Patient.Models.PatientDataQuery;
    using PatientFileQuery = HealthGateway.Patient.Models.PatientFileQuery;

    // Disable documentation for internal class.
#pragma warning disable SA1600
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

        public async Task<PatientDataResponse> Query(PatientDataOptionsQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientDataQueryResult results = await this.patientDataRepository.Query(
                    new HealthOptionsQuery(pid, query.HealthOptionsTypes.Select(this.MapToHealthServiceCategory)),
                    ct)
                .ConfigureAwait(true);

            return new PatientDataResponse(results.Items.SelectMany(this.MapToPatientData).ToArray());
        }

        public async Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientFile? file = (await this.patientDataRepository.Query(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct).ConfigureAwait(true)).Items.FirstOrDefault() as PatientFile;

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        public async Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientDataQueryResult results = await this.patientDataRepository.Query(
                    new HealthDataQuery(pid, query.HealthDataTypes.Select(this.MapToHealthDataCategory)),
                    ct)
                .ConfigureAwait(true);
            return new PatientDataResponse(results.Items.SelectMany(this.MapToPatientData).ToArray());
        }

        private async Task<Guid> ResolvePidFromHdid(string patientHdid)
        {
            return (await this.personalAccountsService.GetPatientAccountAsync(patientHdid).ConfigureAwait(true)).PatientIdentity.Pid;
        }

        private IEnumerable<PatientData> MapToPatientData(BasePatientData basePatientData)
        {
            return basePatientData switch
            {
                OrganDonorRegistration hd => new[] { new OrganDonorRegistrationData(hd.Status, hd.StatusMessage, hd.RegistrationFileId) },
                DiagnosticImagingSummary summary => this.mapper.Map<DiagnosticImagingData>(summary).Exams,
                _ => throw new NotImplementedException($"{basePatientData.GetType().Name} is not mapped to {nameof(PatientData)}"),
            };
        }

        private HealthOptionsCategory MapToHealthServiceCategory(HealthOptionsType healthOptionsType)
        {
            return healthOptionsType switch
            {
                HealthOptionsType.OrganDonorRegistrationStatus => HealthOptionsCategory.OrganDonor,
                _ => throw new NotImplementedException($"{healthOptionsType} is not mapped to {nameof(HealthOptionsCategory)}"),
            };
        }

        private HealthDataCategory MapToHealthDataCategory(HealthDataType healthDataType)
        {
            return healthDataType switch
            {
                HealthDataType.ClinicalDocument => HealthDataCategory.ClinicalDocument,
                HealthDataType.Laboratory => HealthDataCategory.Laboratory,
                HealthDataType.Covid19Laboratory => HealthDataCategory.Covid19Laboratory,
                HealthDataType.DiagnosticImaging => HealthDataCategory.DiagnosticImaging,
                _ => throw new NotImplementedException($"{healthDataType} is not mapped to {nameof(HealthDataCategory)}"),
            };
        }
    }

#pragma warning restore SA1600
}
