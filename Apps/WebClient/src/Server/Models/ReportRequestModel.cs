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
    using System.Text.Json;

    /// <summary>
    /// The report type enum.
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Indicates a PDF report.
        /// </summary>
        PDF,

        /// <summary>
        /// Indicates a EXCEL report.
        /// </summary>
        EXCEL,
    }

    /// <summary>
    /// The report template type enum.
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// Indicates a Medication template type.
        /// </summary>
        Medication,

        /// <summary>
        /// Indicates a Immunization template type.
        /// </summary>
        Immunization,

        /// <summary>
        /// Indicates a Encounter template type.
        /// </summary>
        Encounter,

        /// <summary>
        /// Indicates a Covid Test Results template type.
        /// </summary>
        Covid,
    }

    /// <summary>
    /// Object that defines the request for creating a report.
    /// </summary>
    public class ReportRequestModel
    {
        /// <summary>
        /// Gets or sets the Json data.
        /// </summary>
        public JsonElement Data { get; set; }

        /// <summary>
        /// Gets or sets the report type.
        /// </summary>
        public TemplateType Template { get; set; }

        /// <summary>
        /// Gets or sets the report type.
        /// </summary>
        public ReportType Type { get; set; }
    }
}
