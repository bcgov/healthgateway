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
namespace HealthGateway.Laboratory.Models
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// An instance of a Laboratory PDF Report.
    /// </summary>
    public class LaboratoryPDFReport
    {
        /// <summary>
        /// Gets or sets the base64 encoded PDF Report.
        /// </summary>
        [JsonPropertyName("base64Pdf")]
        public string ReportPDF { get; set; } = string.Empty;
    }
}
