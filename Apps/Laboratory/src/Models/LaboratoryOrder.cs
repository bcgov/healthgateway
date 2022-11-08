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
namespace HealthGateway.Laboratory.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

/// <summary>
/// An instance of a Laboratory Order.
/// </summary>
public class LaboratoryOrder
{
    /// <summary>
    /// Gets or sets the id for the laboratory report.
    /// </summary>
    [JsonPropertyName("labPdfId")]
    public string? LabPdfId { get; set; }

    /// <summary>
    /// Gets or sets a value for reporting source.
    /// </summary>
    [JsonPropertyName("reportingSource")]
    public string ReportingSource { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id for the report.
    /// </summary>
    [JsonPropertyName("reportId")]
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date time when the lab collection took place.
    /// </summary>
    [JsonPropertyName("collectionDateTime")]
    public DateTime? CollectionDateTime { get; set; }

    /// <summary>
    /// Gets or sets the date time for the most relevant date when displaying on a timeline.
    /// </summary>
    [JsonPropertyName("timelineDateTime")]
    public DateTime TimelineDateTime { get; set; }

    /// <summary>
    /// Gets or sets a value for common name.
    /// </summary>
    [JsonPropertyName("commonName")]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for ordering provider.
    /// </summary>
    [JsonPropertyName("orderingProvider")]
    public string OrderingProvider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for test status.
    /// </summary>
    [JsonPropertyName("testStatus")]
    public string TestStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for the order status.
    /// </summary>
    [JsonPropertyName("orderStatus")]
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether report is available.
    /// </summary>
    [JsonPropertyName("reportAvailable")]
    public bool ReportAvailable { get; set; }

    /// <summary>
    /// Gets or sets the collection of laboratory tests.
    /// </summary>
    [JsonPropertyName("laboratoryTests")]
    public IEnumerable<LaboratoryTest> LaboratoryTests { get; set; } = Enumerable.Empty<LaboratoryTest>();
}
