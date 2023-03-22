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
// Disables documentation for internal class.
#pragma warning disable SA1600

// Disables documentation for internal class.
#pragma warning disable SA1602
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using HealthGateway.Common.Utils;
    using Refit;

    internal enum DonorStatus
    {
        Registered,
        NotRegistered,
        Error,
        Pending,
    }

    internal interface IPatientApi
    {
        [Get("/patient/{pid}/file/{fileId}")]
        Task<FileResult?> GetFile(Guid pid, string fileId, CancellationToken ct);

        [Get("/patient/{pid}/health-options")]
        Task<HealthOptionsResult?> GetHealthOptionsAsync(Guid pid, [Query] string[] categories, CancellationToken ct);
    }

    internal record FileResult(string? MediaType, string? Data, string? Encoding);

    internal record HealthOptionsResult(HealthOptionMetadata Metadata, IEnumerable<HealthOptionData> Data);

    internal record HealthOptionMetadata;

    [JsonConverter(typeof(HealthOptionDataJsonConverter))]
    internal abstract record HealthOptionData;

    internal record OrganDonor : HealthOptionData
    {
        public string? HealthOptionsId { get; set; }

        public DonorStatus DonorStatus { get; set; }

        public string? StatusMessage { get; set; }

        public string? HealthOptionsFileId { get; set; }
    }

    internal class HealthOptionDataJsonConverter : PolymorphicJsonConverter<HealthOptionData>
    {
        protected override string Discriminator => "healthOptionsType";

        protected override Type? ResolveType(string discriminatorValue) =>
            discriminatorValue switch
            {
                "BcTransplantOrganDonor" => typeof(OrganDonor),
                _ => null,
            };
    }
}
#pragma warning restore SA1600
#pragma warning restore SA1602
