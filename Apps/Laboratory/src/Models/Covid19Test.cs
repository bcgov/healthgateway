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
using System.Text.Json.Serialization;
using HealthGateway.Laboratory.Models.PHSA;

/// <summary>
/// An instance of a COVID-19 Test.
/// </summary>
public class Covid19Test
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Covid19Test"/> class.
    /// </summary>
    public Covid19Test()
    {
        this.ResultDescription = new List<string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Covid19Test"/> class.
    /// </summary>
    /// <param name="resultDescription">The paragraphs comprising the result description.</param>
    [JsonConstructor]
    public Covid19Test(IList<string> resultDescription)
    {
        this.ResultDescription = resultDescription;
    }

    /// <summary>
    /// Gets or sets the id for the COVID-19 result.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the test type.
    /// </summary>
    [JsonPropertyName("testType")]
    public string TestType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the result is out of range.
    /// </summary>
    [JsonPropertyName("outOfRange")]
    public bool OutOfRange { get; set; }

    /// <summary>
    /// Gets or sets the datetime the lab collection took place.
    /// </summary>
    [JsonPropertyName("collectedDateTime")]
    public DateTime CollectedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the status of the test.
    /// </summary>
    [JsonPropertyName("testStatus")]
    public string? TestStatus { get; set; }

    /// <summary>
    /// Gets or sets the lab result outcome.
    /// </summary>
    [JsonPropertyName("labResultOutcome")]
    public string LabResultOutcome { get; set; } = string.Empty;

    /// <summary>
    /// Gets the result description.
    /// </summary>
    [JsonPropertyName("resultDescription")]
    public IList<string> ResultDescription { get; }

    /// <summary>
    /// Gets or sets the result link.
    /// </summary>
    [JsonPropertyName("resultLink")]
    public string ResultLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the received datetime.
    /// </summary>
    [JsonPropertyName("receivedDateTime")]
    public DateTime ReceivedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the result datetime.
    /// </summary>
    [JsonPropertyName("resultDateTime")]
    public DateTime ResultDateTime { get; set; }

    /// <summary>
    /// Gets or sets the LOINC code.
    /// </summary>
    [JsonPropertyName("loinc")]
    public string? Loinc { get; set; }

    /// <summary>
    /// Gets or sets the LOINC Name/Description.
    /// </summary>
    [JsonPropertyName("loincName")]
    public string? LoincName { get; set; }

    /// <summary>
    /// Creates a Covid19Test object from a PHSA model.
    /// </summary>
    /// <param name="model">The PHSA model to convert.</param>
    /// <returns>The newly created COVID-19 test object.</returns>
    public static Covid19Test FromPhsaModel(PhsaCovid19Test model)
    {
        return new Covid19Test(model.ResultDescription)
        {
            Id = model.Id,
            TestType = model.TestType,
            OutOfRange = model.OutOfRange,
            CollectedDateTime = model.CollectedDateTime,
            TestStatus = model.TestStatus,
            LabResultOutcome = model.LabResultOutcome,
            ResultLink = model.ResultLink,
            ReceivedDateTime = model.ReceivedDateTime,
            ResultDateTime = model.ResultDateTime,
            Loinc = model.Loinc,
            LoincName = model.LoincName,
        };
    }
}
