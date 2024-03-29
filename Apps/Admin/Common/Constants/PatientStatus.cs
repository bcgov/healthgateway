﻿// -------------------------------------------------------------------------
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
namespace HealthGateway.Admin.Common.Constants;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the status of a user returned from a support query.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PatientStatus
{
    /// <summary>
    /// Indicates that the patient has no special status.
    /// </summary>
    Default,

    /// <summary>
    /// Indicates that the patient was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// Indicates that the patient is deceased.
    /// </summary>
    Deceased,

    /// <summary>
    /// Indicates that the patient is not a user of Health Gateway.
    /// </summary>
    NotUser,
}
