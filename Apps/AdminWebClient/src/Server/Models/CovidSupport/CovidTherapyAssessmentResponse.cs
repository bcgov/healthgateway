//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.Admin.Server.Models.CovidSupport;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Model object representing an anti viral screener support submission response.
/// </summary>
public class CovidTherapyAssessmentResponse
{
    /// <summary>
    /// Gets or sets the phn.
    /// </summary>
    [JsonPropertyName("phn")]
    public string? Phn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there has been a reent positive covid result.
    /// </summary>
    [JsonPropertyName("hasRecentPositiveCovidResult")]
    public bool HasRecentPositiveCovidResult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cev 10 or cev 2 has been found.
    /// </summary>
    [JsonPropertyName("foundCev1OrCev2")]
    public bool FoundCev1OrCev2 { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there have been any chronic conditions found.
    /// </summary>
    [JsonPropertyName("foundChronicConditions")]
    public bool FoundChronicConditions { get; set; }

    /// <summary>
    /// Gets or sets the chronic condition count.
    /// </summary>
    [JsonPropertyName("chronicConditionCount")]
    public int ChronicConditionCount { get; set; }

    /// <summary>
    /// Gets or sets the dose count.
    /// </summary>
    [JsonPropertyName("doseCount")]
    public int DoseCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there are three doses and 14 days.
    /// </summary>
    [JsonPropertyName("threeDoseAnd14Days")]
    public bool ThreeDoseAnd14Days { get; set; }

    /// <summary>
    /// Gets or sets the calculated age.
    /// </summary>
    [JsonPropertyName("calculatedAge")]
    public int CalculatedAge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether previous anti viral submissions have been found.
    /// </summary>
    [JsonPropertyName("previousAvSubmissionFound")]
    public bool PreviousAvSubmissionFound { get; set; }

    /// <summary>
    /// Gets or sets the list of most recent anti viral submission date times.
    /// </summary>
    [JsonPropertyName("mostRecentAvSubmissionDateTime")]
    public IEnumerable<DateTime>? MostRecentAvSubmissionDateTime { get; set; }
}
