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
namespace HealthGateway.Common.Data.ViewModels
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Data.ErrorHandling;

    /// <summary>
    /// The RequestResultError model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RequestResultError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestResultError"/> class.
        /// </summary>
        public RequestResultError()
        {
            this.TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the message depending on the result type.
        /// Will always be set when ResultType is Error.
        /// </summary>
        [JsonPropertyName("resultMessage")]
        public string ResultMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error code.
        /// Will always be set when ResultType is Error.
        /// </summary>
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trace id associated with the request.
        /// </summary>
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action code.
        /// Will always be set when ResultType is ActionRequired.
        /// </summary>
        [JsonIgnore]
        public ActionType? ActionCode { get; set; }

        /// <summary>
        /// Gets action code.
        /// Will always be set when ResultType is ActionRequired.
        /// </summary>
        [JsonPropertyName("actionCode")]
        public string? ActionCodeValue => this.ActionCode?.Value;
    }
}
