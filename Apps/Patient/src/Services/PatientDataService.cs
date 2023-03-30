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
    using HealthGateway.PatientDataAccess;

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

        public async Task<PatientDataResponse> Query(PatientDataQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            List<Task<PatientDataQueryResult>> tasks = new();
            tasks.Add(this.HandleServiceQuery(pid, query, ct));
            tasks.Add(this.HandleDataQuery(pid, query, ct));

            IEnumerable<PatientDataQueryResult> results = await Task.WhenAll(tasks).ConfigureAwait(true);
            return new PatientDataResponse(results.SelectMany(r => r.Items).Select(this.MapToPatientData).ToArray());
        }

        public async Task<PatientFileResponse?> Query(PatientFileQuery query, CancellationToken ct)
        {
            Guid pid = await this.ResolvePidFromHdid(query.Hdid).ConfigureAwait(true);
            PatientFile? file = (await this.patientDataRepository.Query(new PatientDataAccess.PatientFileQuery(pid, query.FileId), ct).ConfigureAwait(true)).Items.FirstOrDefault() as PatientFile;

            return file != null
                ? new PatientFileResponse(file.Content, file.ContentType)
                : null;
        }

        private static IEnumerable<HealthServiceCategory> ExtractToHealthServiceCategoryArray(IEnumerable<PatientDataType> patientDataType)
        {
            foreach (PatientDataType pdt in patientDataType)
            {
                HealthServiceCategory? hsc = pdt switch
                {
                    PatientDataType.OrganDonorRegistrationStatus => HealthServiceCategory.OrganDonor,
                    _ => null,
                };
                if (hsc != null)
                {
                    yield return hsc.Value;
                }
            }
        }

        private static IEnumerable<HealthDataCategory> ExtractToHealthDataCategoryArray(IEnumerable<PatientDataType> patientDataType)
        {
            foreach (PatientDataType pdt in patientDataType)
            {
                HealthDataCategory? hsc = pdt switch
                {
                    PatientDataType.DiagnosticImaging => HealthDataCategory.DiagnosticImaging,
                    _ => null,
                };
                if (hsc != null)
                {
                    yield return hsc.Value;
                }
            }
        }

        private async Task<Guid> ResolvePidFromHdid(string patientHdid)
        {
            return (await this.personalAccountsService.GetPatientAccountAsync(patientHdid).ConfigureAwait(true)).PatientIdentity.Pid;
        }

        private async Task<PatientDataQueryResult> HandleServiceQuery(Guid pid, PatientDataQuery query, CancellationToken ct)
        {
            IEnumerable<HealthServiceCategory> categories = ExtractToHealthServiceCategoryArray(query.PatientDataTypes).ToArray();
            if (categories.Any())
            {
                PatientDataQueryResult results = await this.patientDataRepository.Query(
                        new HealthServicesQuery(pid, categories),
                        ct)
                    .ConfigureAwait(true);

                return results;
            }

            return new PatientDataQueryResult(Array.Empty<HealthData>());
        }

        private async Task<PatientDataQueryResult> HandleDataQuery(Guid pid, PatientDataQuery query, CancellationToken ct)
        {
            IEnumerable<HealthDataCategory> categories = ExtractToHealthDataCategoryArray(query.PatientDataTypes).ToArray();
            if (categories.Any())
            {
                PatientDataQueryResult results = await this.patientDataRepository.Query(
                        new HealthDataQuery(pid, categories),
                        ct)
                    .ConfigureAwait(true);

                return results;
            }

            return new PatientDataQueryResult(Array.Empty<HealthData>());
        }

        private PatientData MapToPatientData(HealthData healthData)
        {
            return healthData switch
            {
                OrganDonorRegistration hd => this.mapper.Map<OrganDonorRegistrationData>(hd),
                DiagnosticImagingExam hd => this.mapper.Map<DiagnosticImagingExamData>(hd),
                _ => throw new NotImplementedException($"{healthData.GetType().Name} is not mapped to {nameof(PatientData)}"),
            };
        }
    }
#pragma warning restore SA1600
}
