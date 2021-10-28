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
namespace HealthGateway.Common.Models.BCMailPlus
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Additional Job properties.
    /// </summary>
    public class BcmpProperties
    {
        /// <summary>
        /// Gets or sets the unique Job ID associated with the Vaccine Proof.
        /// </summary>
        [JsonPropertyName("JOB_ID")]
        public long JobId { get; set; }

        /// <summary>
        /// Gets or sets the asset uri.
        /// </summary>
        [JsonPropertyName("PDF_URL")]
        public Uri? AssetUri { get; set; }
    }
}
