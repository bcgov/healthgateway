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
using AutoMapper;

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

        public async Task<QueryResult> Query(PatientDataQuery query)
        {
            using var cts = new CancellationTokenSource();

            return await (query switch
            {
                HealthServicesQuery q => Handle(q, cts.Token),
                _ => throw new NotImplementedException()
            });
        }

        private async Task<QueryResult> Handle(HealthServicesQuery query, CancellationToken ct)
        {
            var categories = query.Categories.Select(c => Map(c)).ToArray();
            var results = await patientApi.GetHealthOptionsAsync(query.Pid, categories, ct);

            return new QueryResult(results.Data.Select(d => Map(d)));
        }

        private HealthData Map(HealthOptionData healthOptionData) => mapper.Map<HealthData>(healthOptionData);

        private static string Map(HealthServiceCategory category) =>
            category switch
            {
                HealthServiceCategory.OrganDonor => "BcTransplantOrganDonor",

                _ => throw new NotImplementedException($"{category}"),
            };
    }
}
