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
namespace HealthGateway.Admin.Common.Models.AdminReports
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a protected dependent report.
    /// </summary>
    /// <param name="Records">The list of records to display.</param>
    /// <param name="Metadata">Metadata describing the dataset metrics.</param>
    public record ProtectedDependentReport(IReadOnlyList<ProtectedDependentRecord> Records, ReportMetadata Metadata);

    /// <summary>
    /// Represents a protected dependent.
    /// </summary>
    /// <param name="Hdid">The dependent's HDID.</param>
    /// <param name="Phn">The dependent's personal health number.</param>
    public record ProtectedDependentRecord(string Hdid, string? Phn);
}
