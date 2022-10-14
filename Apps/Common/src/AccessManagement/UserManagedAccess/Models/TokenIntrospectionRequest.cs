//-------------------------------------------------------------------------
// Copyright © 2020 Province of British Columbia
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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.AccessManagement.UserManagedAccess.Constants;

    /// <summary>
    /// UMA 2.0 RPT Introspection Requst Payload (to be url encoded).
    /// </summary>
    public class TokenIntrospectionRequest
    {
        /// <summary>Gets or sets the resource id.</summary>
        [JsonPropertyName("token_type_hint")]
        public string? TokenTypeHint { get; set; } = OAuth2Constants.TokenTypeHintRpt;

        /// <summary>Requesting Party Token.</summary>
        [JsonPropertyName("token")]
        public string? Rpt { get; set; }

    }
}