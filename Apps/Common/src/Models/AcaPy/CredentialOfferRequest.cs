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
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A credential offer to send to aries agent.
    /// </summary>
    public class CredentialOfferRequest
    {
        /// <summary>
        /// Gets or sets the connectionId.
        /// </summary>
        [JsonPropertyName("connection_id")]
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the IssuerDid.
        /// </summary>
        [JsonPropertyName("issuer_did")]
        public string IssuerDid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SchemaId.
        /// </summary>
        [JsonPropertyName("schema_id")]
        public string SchemaId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Schema Issuer Did.
        /// </summary>
        [JsonPropertyName("schema_issuer_did")]
        public string SchemaIssuerDid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Schema Name.
        /// </summary>
        [JsonPropertyName("schema_name")]
        public string SchemaName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Schema Version.
        /// </summary>
        [JsonPropertyName("schema_version")]
        public string SchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Credential Definition Id.
        /// </summary>
        [JsonPropertyName("cred_def_id")]
        public string CredentialDefinitionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether auto remove is set.
        /// </summary>
        [JsonPropertyName("auto_remove")]
        public bool AutoRemove { get; } = false;

        /// <summary>
        /// Gets a value indicating whether trace is set.
        /// </summary>
        [JsonPropertyName("trace")]
        public bool Trace { get; } = false;

        /// <summary>
        /// Gets or sets the credential proposal.
        /// </summary>
        [JsonPropertyName("credential_proposal")]
        public CredentialProposal CredentialProposal { get; set; } = new();
    }
}
