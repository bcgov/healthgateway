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
namespace HealthGateway.Patient.Models
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Diagnostic image exam statuses.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    [JsonStringEnumMemberConverterOptions(deserializationFailureFallbackValue: Unknown)]
    public enum DiagnosticImagingStatus
    {
        /// <summary>
        /// Unknown status.
        /// </summary>
        Unknown,

        /// <summary>
        /// Exam is in progress.
        /// </summary>
        [EnumMember(Value = "In Progress")]
        InProgress,

        /// <summary>
        /// Exam result is pending.
        /// </summary>
        Pending,

        /// <summary>
        /// Exam is completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Exam is amended.
        /// </summary>
        Amended,

        /// <summary>
        /// Exam is final.
        /// </summary>
        Final,
    }
}
