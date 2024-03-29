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
namespace HealthGateway.Laboratory.Models.PHSA;

using System.Text.Json.Serialization;

/// <summary>
/// An instance of a PHSA Laboratory Test.
/// </summary>
public class PhsaLaboratoryTest
{
    /// <summary>
    /// Gets or sets a value for battery type.
    /// </summary>
    [JsonPropertyName("batteryType")]
    public string BatteryType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the id for the obx.
    /// </summary>
    [JsonPropertyName("obxId")]
    public string ObxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether laboratory test is out of range.
    /// </summary>
    [JsonPropertyName("outOfRange")]
    public bool OutOfRange { get; set; }

    /// <summary>
    /// Gets or sets a value for loinc.
    /// </summary>
    [JsonPropertyName("loinc")]
    public string Loinc { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value for test status.
    /// </summary>
    [JsonPropertyName("plisTestStatus")]
    public string PlisTestStatus { get; set; } = string.Empty;
}
