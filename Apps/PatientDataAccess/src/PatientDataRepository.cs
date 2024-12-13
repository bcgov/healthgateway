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
    using System.Diagnostics.CodeAnalysis;
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
    /// <param name="patientApi">The patient API to use.</param>
    /// <param name="mapper">The injected mapper.</param>
    [SuppressMessage("Style", "IDE0072:Switch expression should be exhaustive", Justification = "Team decision")]
    internal class PatientDataRepository(IPatientApi patientApi, IMapper mapper) : IPatientDataRepository
    {
        /// <summary>
        /// Performs a query against the data repository.
        /// </summary>
        /// <param name="query">The query to perform.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The query result.</returns>
        /// <exception cref="NotImplementedException">Thrown if query is not implemented.</exception>
        public async Task<PatientDataQueryResult> QueryAsync(PatientDataQuery query, CancellationToken ct = default)
        {
            return query switch
            {
                HealthQuery q => await this.HandleAsync(q, ct),
                PatientFileQuery q => await this.HandleAsync(q, ct),
                _ => throw new NotImplementedException($"{query.GetType().Name} doesn't have a handler"),
            };
        }

        private static string? MapHealthOptionsCategories(HealthCategory category)
        {
            return category switch
            {
                HealthCategory.OrganDonorRegistrationStatus => "BcTransplantOrganDonor",
                _ => null,
            };
        }

        private static string? MapHealthDataCategories(HealthCategory category)
        {
            return category switch
            {
                HealthCategory.DiagnosticImaging => "DiagnosticImaging",
                HealthCategory.BcCancerScreening => "BcCancerScreening",
                _ => null,
            };
        }

        private static PatientFile Map(string fileId, FileResult file)
        {
            return new(fileId, Convert.FromBase64String(file.Data!), file.MediaType ?? string.Empty);
        }

        private async Task<PatientDataQueryResult> HandleAsync(HealthQuery query, CancellationToken ct)
        {
            List<Task<IEnumerable<HealthData>>> tasks =
            [
                this.HandleServiceQueryAsync(query, ct),
                this.HandleDataQueryAsync(query, ct),
            ];

            IEnumerable<IEnumerable<HealthData>> results = await Task.WhenAll(tasks);
            return new PatientDataQueryResult(results.SelectMany(r => r));
        }

        private async Task<PatientDataQueryResult> HandleAsync(PatientFileQuery query, CancellationToken ct)
        {
            try
            {
                FileResult? fileResult = await patientApi.GetFileAsync(query.Pid, query.FileId, ct);
                IEnumerable<PatientFile> mappedFiles = new[] { fileResult }
                    .Where(f => f?.Data != null)
                    .Select(f => Map(query.FileId, f!));
                return new PatientDataQueryResult(mappedFiles);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                // file not found
                return new PatientDataQueryResult([]);
            }
        }

        private async Task<IEnumerable<HealthData>> HandleServiceQueryAsync(HealthQuery query, CancellationToken ct)
        {
            string[] categories = query.Categories
                .Select(MapHealthOptionsCategories)
                .Where(c => c != null)
                .OfType<string>()
                .ToArray();

            if (categories.Length != 0)
            {
                HealthOptionsResult results = await patientApi.GetHealthOptionsAsync(query.Pid, categories, ct) ??
                                              new(new HealthOptionsMetadata(), []);

                return results.Data.Select(this.Map);
            }

            return [];
        }

        private async Task<IEnumerable<HealthData>> HandleDataQueryAsync(HealthQuery query, CancellationToken ct)
        {
            string[] categories = query.Categories
                .Select(MapHealthDataCategories)
                .Where(c => c != null)
                .OfType<string>()
                .ToArray();

            if (categories.Length != 0)
            {
                HealthDataResult results = await patientApi.GetHealthDataAsync(query.Pid, categories, ct) ??
                                           new(new HealthDataMetadata(), []);

                return results.Data.Select(this.Map);
            }

            return [];
        }

        private HealthData Map(HealthOptionsData healthOptionsData)
        {
            return mapper.Map<HealthData>(healthOptionsData);
        }

        private HealthData Map(HealthDataEntry healthDataEntry)
        {
            return mapper.Map<HealthData>(healthDataEntry);
        }
    }
}
