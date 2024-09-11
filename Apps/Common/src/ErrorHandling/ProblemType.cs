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
namespace HealthGateway.Common.ErrorHandling;

using System.Runtime.Serialization;

/// <summary>
/// Problem types for Health Gateway, providing concisely coded failure reasons.
/// </summary>
public enum ProblemType
{
    /// <summary>
    /// Generic server error.
    /// </summary>
    [EnumMember(Value = "server-error")]
    ServerError,

    /// <summary>
    /// Dataset is in the process of being refreshed.
    /// </summary>
    [EnumMember(Value = "refresh-in-progress")]
    RefreshInProgress,

    /// <summary>
    /// Maximum number of retries has been reached.
    /// </summary>
    [EnumMember(Value = "max-retries-reached")]
    MaxRetriesReached,

    /// <summary>
    /// Record already exists.
    /// </summary>
    [EnumMember(Value = "record-already-exists")]
    RecordAlreadyExists,

    /// <summary>
    /// Record was not found.
    /// </summary>
    [EnumMember(Value = "record-not-found")]
    RecordNotFound,

    /// <summary>
    /// Service or database did not return valid data.
    /// </summary>
    [EnumMember(Value = "invalid-data")]
    InvalidData,

    /// <summary>
    /// Upstream service returned an error.
    /// </summary>
    [EnumMember(Value = "upstream-error")]
    UpstreamError,

    /// <summary>
    /// Invalid input provided.
    /// </summary>
    [EnumMember(Value = "invalid-input")]
    InvalidInput,

    /// <summary>
    /// Error when performing a database operation.
    /// </summary>
    [EnumMember(Value = "database-error")]
    DatabaseError,
}
