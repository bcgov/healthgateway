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
namespace HealthGateway.WebClient.Models.AcaPy
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Connection to the Ares Agent.
    /// </summary>
    public class CreateConnectionResponse
    {
        /// <summary>
        /// Gets or sets the connection id.
        /// </summary>
        [JsonPropertyName("connection_id")]
        public Guid AgentId { get; set; }

        /// <summary>
        /// Gets or sets the invitation url.
        /// </summary>
        [JsonPropertyName("invitation_url")]
        public System.Uri? InvitationUrl { get; set; }
    }
}
