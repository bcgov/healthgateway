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
namespace HealthGateway.Common.AccessManagement.UserManagedAccess.Models
{
    using System.Text.Json.Serialization;

    /// <summary>Metadata, a component of <see cref="AuthorizationRequest"/>.</summary>
    public class RequestMetadata
    {
        /// <summary>Gets or sets a value indicating whether to include Resource Name.</summary>
        [JsonPropertyName("includeResourceName")]
        public bool IncludeResourceName { get; set; }

        /// <summary>Gets or sets the limit.</summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>Gets or sets the response mode.</summary>
        [JsonPropertyName("responseMode")]
        public string? ResponseMode { get; set; }
    }
}