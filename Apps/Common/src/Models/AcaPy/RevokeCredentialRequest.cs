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
namespace HealthGateway.Common.Models.AcaPy
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents a Revoke Credential Request to the aries agent.
    /// </summary>
    public class RevokeCredentialRequest
    {
        /// <summary>
        /// Gets or sets credential revocation id.
        /// </summary>
        [JsonPropertyName("cred_rev_id")]
        public string CredentialRevocationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets revocation registry id.
        /// </summary>
        [JsonPropertyName("rev_reg_id")]
        public string RevocationRegistryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the publish flag is set.
        /// </summary>
        [JsonPropertyName("publish")]
        public bool Publish { get; } = true;
    }
}
