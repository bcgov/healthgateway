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
namespace HealthGateway.Immunization.Models.PHSA
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// An object representing a file received from PHSA.
    /// </summary>
    public class Media
    {
        /// <summary>
        /// Gets or sets the media type of the data attribute.
        /// </summary>
        [JsonPropertyName("mediaType")]
        public string? MediaType { get; set; }

        /// <summary>
        /// Gets or sets the encoding of data attribute.
        /// </summary>
        [JsonPropertyName("encoding")]
        public string? Encoding { get; set; }

        /// <summary>
        /// Gets or sets the raw data encoded and having a media type as specified.
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}
