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
namespace HealthGateway.Common.Models.CDogs
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Object that defines the cdogs request for creating a report.
    /// </summary>
    public class CDogsRequestModel
    {
        /// <summary>
        /// Gets or sets the Json data.
        /// </summary>
        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        [JsonPropertyName("options")]
        public CDogsOptionsModel Options { get; set; } = new();

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        [JsonPropertyName("template")]
        public CDogsTemplateModel Template { get; set; } = new();
    }
}
