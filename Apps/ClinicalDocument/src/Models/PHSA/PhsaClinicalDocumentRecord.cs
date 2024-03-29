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
namespace HealthGateway.ClinicalDocument.Models.PHSA
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the PHSA model for a clinical document record.
    /// </summary>
    public class PhsaClinicalDocumentRecord
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [JsonPropertyName("healthDataId")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        [JsonPropertyName("healthDataType")]
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        [JsonPropertyName("healthDataFileId")]
        public string FileId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the facility name.
        /// </summary>
        [JsonPropertyName("facilityName")]
        public string FacilityName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the discipline.
        /// </summary>
        [JsonPropertyName("discipline")]
        public string Discipline { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service date.
        /// </summary>
        [JsonPropertyName("serviceDate")]
        public DateTime ServiceDate { get; set; }

        /// <summary>
        /// Gets or sets the source system id.
        /// </summary>
        [JsonPropertyName("sourceSystemId")]
        public string SourceSystemId { get; set; } = string.Empty;
    }
}
