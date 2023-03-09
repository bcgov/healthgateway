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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HealthGateway.PatientDataAccess.Api;

namespace HealthGateway.PatientDataAccess
{
    internal class PatientDataRepository : IPatientDataRepository
    {
        private readonly IPatientApi patientApi;
        private readonly IMapper mapper;

        public PatientDataRepository(IPatientApi patientApi, IMapper mapper)
        {
            this.patientApi = patientApi;
            this.mapper = mapper;
        }

        public async Task<PatientDataQueryResult> Query(PatientDataQuery query, CancellationToken ct)
        {
            return query switch
            {
                HealthServicesQuery q => await Handle(q, ct),
                PatientFileQuery q => await Handle(q, ct),

                _ => throw new NotImplementedException($"{query.GetType().Name} doesn't have a handler")
            };
        }

        private async Task<PatientDataQueryResult> Handle(HealthServicesQuery query, CancellationToken ct)
        {
            var categories = query.Categories.Select(c => Map(c)).ToArray();
            var results = await patientApi.GetHealthOptionsAsync(query.Pid, categories, ct) ??
                          new HealthOptionsResult(new HealthOptionMetadata(), Array.Empty<HealthOptionData>());

            return new PatientDataQueryResult(results.Data.Select(Map));
        }

        private async Task<PatientDataQueryResult> Handle(PatientFileQuery query, CancellationToken ct)
        {
            var fileResult = await patientApi.GetFile(query.Pid, query.FileId, ct);
            var mappedFiles = new[] { fileResult }
                .Where(f => f?.Data != null)
                .Select(f => Map(query.FileId, f!));
            return new PatientDataQueryResult(mappedFiles);
        }

        private HealthData Map(HealthOptionData healthOptionData) => mapper.Map<HealthData>(healthOptionData);

        private static string Map(HealthServiceCategory category) =>
            category switch
            {
                HealthServiceCategory.OrganDonor => "BcTransplantOrganDonor",

                _ => throw new NotImplementedException($"No mapping implemented for {category}")
            };

        private static PatientFile Map(string fileId, FileResult file) =>
            new(fileId, Encoding.Default.GetBytes(file.Data!), file.MediaType ?? string.Empty);
    }
}
