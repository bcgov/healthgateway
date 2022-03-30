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
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HealthGateway.Laboratory.Models.PHSA;

/// <summary>
/// An instance of a Laboratory Order.
/// </summary>
public class LaboratoryOrder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LaboratoryOrder"/> class.
    /// </summary>
    public LaboratoryOrder()
    {
        this.LaboratoryTests = new List<LaboratoryTest>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LaboratoryOrder"/> class.
    /// </summary>
    /// <param name="laboratoryTests">The list of Laboratory Test records.</param>
    [JsonConstructor]
    public LaboratoryOrder(IList<LaboratoryTest> laboratoryTests)
    {
        this.LaboratoryTests = laboratoryTests;
    }

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
    /// Gets or sets a value indicating whether report is available.
    /// </summary>
    [JsonPropertyName("reportAvailable")]
    public bool ReportAvailable { get; set; }

    /// <summary>
    /// Gets a list of PHSA Laboratory Tests.
    /// </summary>
    [JsonPropertyName("laboratoryTests")]
    public IList<LaboratoryTest> LaboratoryTests { get; }

    /// <summary>
    /// Creates a <see cref="LaboratoryOrder"/> object from a PHSA model.
    /// </summary>
    /// <param name="model">The laboratory order result to convert.</param>
    /// <returns>The newly created laboratory order object.</returns>
    public static LaboratoryOrder FromPhsaModel(PhsaLaboratoryOrder model)
    {
        IList<LaboratoryTest> laboratoryTests =
            model.LabBatteries != null ? model.LabBatteries.Select(LaboratoryTest.FromPhsaModel).ToList() : new List<LaboratoryTest>();

        return new LaboratoryOrder(laboratoryTests)
        {
            LabPdfId = model.LabPdfId,
            ReportingSource = model.ReportingSource,
            ReportId = model.ReportId,
            CollectionDateTime = model.CollectionDateTime,
            CommonName = model.CommonName,
            OrderingProvider = model.OrderingProvider,
            TestStatus = model.PlisTestStatus,
            ReportAvailable = model.PdfReportAvailable,
        };
    }

    /// <summary>
    /// Creates a collection of <see cref="LaboratoryOrder"/> models from a collection of PHSA models.
    /// </summary>
    /// <param name="phsaOrders">The list of PHSA models to convert.</param>
    /// <returns>A collection of <see cref="LaboratoryOrder"/> models.</returns>
    public static IEnumerable<LaboratoryOrder> FromPhsaModelCollection(IEnumerable<PhsaLaboratoryOrder>? phsaOrders)
    {
        return phsaOrders != null ? phsaOrders.Select(FromPhsaModel) : Enumerable.Empty<LaboratoryOrder>();
    }
}
