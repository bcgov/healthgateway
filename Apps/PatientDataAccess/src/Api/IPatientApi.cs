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
namespace HealthGateway.PatientDataAccess.Api
{
#pragma warning disable SA1600 // Disables documentation for internal class.
#pragma warning disable SA1602 // Disables documentation for internal class.
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Refit;

    internal enum OrganDonorRegistrationStatus
    {
        Registered,
        NotRegistered,
        Error,
        Pending,
    }

    internal enum HealthDataCategory
    {
        Laboratory,
        Covid19Laboratory,
        ClinicalDocument,
        DiagnosticImaging,
        BcCancerScreening,
    }

    internal enum DiagnosticImagingStatus
    {
        Scheduled,
        InProgress,
        Finalized,
        Pending,
        Completed,
        Amended,
    }

    internal enum CancerScreeningType
    {
        Recall,
        Result,
    }

    internal interface IPatientApi
    {
        [Get("/patient/{pid}/file/{fileId}")]
        Task<FileResult?> GetFileAsync(Guid pid, string fileId, CancellationToken ct);

        [Get("/patient/{pid}/health-options")]
        Task<HealthOptionsResult?> GetHealthOptionsAsync(Guid pid, [Query(CollectionFormat.Multi)] string[] categories, CancellationToken ct);

        [Get("/patient/{pid}/health-data")]
        Task<HealthDataResult?> GetHealthDataAsync(Guid pid, [Query(CollectionFormat.Multi)] string[] categories, CancellationToken ct);
    }

    internal record FileResult(string? MediaType, string? Data, string? Encoding);

    internal record HealthOptionsResult(HealthOptionsMetadata Metadata, IEnumerable<HealthOptionsData> Data);

    internal record HealthDataResult(HealthDataMetadata Metadata, IEnumerable<HealthDataEntry> Data);

    internal record HealthOptionsMetadata;

    internal record HealthDataMetadata;
}
#pragma warning restore SA1600
#pragma warning restore SA1602
