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
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an api result.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    public class ApiResult<T>
        where T : class?
    {
        /// <summary>
        /// Gets or sets a warning.
        /// </summary>
        [JsonPropertyName("warning")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ApiWarning? Warning { get; set; }

        /// <summary>
        /// Gets or sets the result payload.
        /// </summary>
        [JsonPropertyName("resourcePayload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? ResourcePayload { get; set; }
    }
}
