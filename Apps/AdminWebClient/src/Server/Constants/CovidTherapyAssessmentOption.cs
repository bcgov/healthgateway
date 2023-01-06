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
namespace HealthGateway.Admin.Constants;

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the options that can be selected from yes/no radio button options.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CovidTherapyAssessmentOption
{
    /// <summary>
    /// Indicates that Unspecified was selected.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Indicates that Yes was selected.
    /// </summary>
    Yes,

    /// <summary>
    /// Indicates that No was selected.
    /// </summary>
    No,

    /// <summary>
    /// Indicates that Not Sure was selected.
    /// </summary>
    NotSure,
}
