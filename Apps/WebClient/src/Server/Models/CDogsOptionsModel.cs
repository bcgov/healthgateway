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
namespace HealthGateway.WebClient.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Model that defines the cdogs options.
    /// </summary>
    public class CDogsOptionsModel
    {
        /// <summary>
        /// Gets or sets the file type to convert to.
        /// </summary>
        [JsonPropertyName("convertTo")]
        public string ConvertTo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the file type.
        /// </summary>
        [JsonPropertyName("overwrite")]
        public bool Overwrite { get; set; } = true;

        /// <summary>
        /// Gets or sets the report name.
        /// </summary>
        [JsonPropertyName("reportName")]
        public string ReportName { get; set; } = string.Empty;
    }
}
