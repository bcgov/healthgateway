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

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Model object representing covid therapy assessment details.
/// </summary>
public class CovidAssessmentDetailsResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether there has been known positive covid 19 in past 70 days.
    /// </summary>
    [JsonPropertyName("hasKnownPositiveC19Past7Days")]
    public bool HasKnownPositiveC19Past7Days { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether citizen is considered immuno compromised.
    /// </summary>
    [JsonPropertyName("citizenIsConsideredImmunoCompromised")]
    public bool CitizenIsConsideredImmunoCompromised { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there has been 3 doses in more than 14 days.
    /// </summary>
    [JsonPropertyName("has3DoseMoreThan14Days")]
    public bool Has3DoseMoreThan14Days { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether there has been a documented chronic condition.
    /// </summary>
    [JsonPropertyName("hasDocumentedChronicCondition")]
    public bool HasDocumentedChronicCondition { get; set; }

    /// <summary>
    /// Gets or sets the list of previous assessment details.
    /// </summary>
    [JsonPropertyName("previousAssessmentDetailsList")]
    public IEnumerable<PreviousAssessmentDetails>? PreviousAssessmentDetailsList { get; set; }
}
