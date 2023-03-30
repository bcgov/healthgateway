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
namespace HealthGateway.PatientDataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using HealthGateway.PatientDataAccess.Api;
    using Refit;

    /// <summary>
    /// Provides internal data access for Patient.
    /// </summary>
    internal class PatientDataRepository : IPatientDataRepository
    {
        private readonly IPatientApi patientApi;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientDataRepository"/> class.
        /// </summary>
        /// <param name="patientApi">The patient API to use.</param>
        /// <param name="mapper">The injected mapper.</param>
        public PatientDataRepository(IPatientApi patientApi, IMapper mapper)
        {
            this.patientApi = patientApi;
            this.mapper = mapper;
        }

        /// <summary>
        /// Performs a query against the data repository.
        /// </summary>
        /// <param name="query">The query to perform.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The query result.</returns>
        /// <exception cref="NotImplementedException">Thrown if query is not implemented.</exception>
        public async Task<PatientDataQueryResult> Query(PatientDataQuery query, CancellationToken ct)
        {
            return query switch
            {
                HealthServicesQuery q => await this.Handle(q, ct).ConfigureAwait(true),
                HealthDataQuery q => await this.Handle(q, ct).ConfigureAwait(true),
                PatientFileQuery q => await this.Handle(q, ct).ConfigureAwait(true),
                _ => throw new NotImplementedException($"{query.GetType().Name} doesn't have a handler"),
            };
        }

        private static string MapHealthOptionsCategories(HealthServiceCategory category)
        {
            return category switch
            {
                HealthServiceCategory.OrganDonor => "BcTransplantOrganDonor",
                _ => throw new NotImplementedException($"{category} doesn't have a handler"),
            };
        }

        private static string MapHealthDataCategories(HealthDataCategory category)
        {
            return category switch
            {
                HealthDataCategory.DiagnosticImaging => "DiagnosticImaging",
                _ => throw new NotImplementedException($"{category} doesn't have a handler"),
            };
        }

        private static PatientFile Map(string fileId, FileResult file)
        {
            return new(fileId, Convert.FromBase64String(file.Data!), file.MediaType ?? string.Empty);
        }

        private async Task<PatientDataQueryResult> Handle(HealthServicesQuery query, CancellationToken ct)
        {
            string[] categories = query.Categories.Select(MapHealthOptionsCategories).ToArray();

            if (categories.Any())
            {
                HealthOptionsResult results = await this.patientApi.GetHealthOptionsAsync(query.Pid, categories, ct).ConfigureAwait(true) ??
                                              new(new HealthOptionMetadata(), Array.Empty<HealthOptionData>());
                return new PatientDataQueryResult(results.Data.Select(this.Map));
            }

            return new PatientDataQueryResult(Array.Empty<HealthData>());
        }

        private async Task<PatientDataQueryResult> Handle(HealthDataQuery query, CancellationToken ct)
        {
            string[] categories = query.Categories.Select(MapHealthDataCategories).ToArray();

            if (categories.Any())
            {
                HealthDataResult results = await this.patientApi.GetHealthDataAsync(query.Pid, categories, ct).ConfigureAwait(true) ??
                                           new(new HealthDataMetadata(), Array.Empty<HealthDataEntry>());

                return new PatientDataQueryResult(results.Data.Select(this.Map));
            }

            return new PatientDataQueryResult(Array.Empty<HealthData>());
        }

        private async Task<PatientDataQueryResult> Handle(PatientFileQuery query, CancellationToken ct)
        {
            try
            {
                FileResult? fileResult = await this.patientApi.GetFile(query.Pid, query.FileId, ct).ConfigureAwait(true);
                IEnumerable<PatientFile> mappedFiles = new[] { fileResult }
                    .Where(f => f?.Data != null)
                    .Select(f => Map(query.FileId, f!));
                return new PatientDataQueryResult(mappedFiles);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // file not found
                return new PatientDataQueryResult(Array.Empty<HealthData>());
            }
        }

        private HealthData Map(HealthOptionData healthOptionData)
        {
            return this.mapper.Map<HealthData>(healthOptionData);
        }

        private HealthData Map(HealthDataEntry healthOptionData)
        {
            return this.mapper.Map<HealthData>(healthOptionData);
        }
    }
}
