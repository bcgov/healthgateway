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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A credential proposal.
    /// </summary>
    public class CredentialProposal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialProposal"/> class.
        /// </summary>
        public CredentialProposal()
        {
            this.Attributes = new Collection<CredentialAttribute>();
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [JsonPropertyName("@type")]
        public string Type { get; } = "issue-credential/1.0/credential-preview";

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [JsonPropertyName("attributes")]
        public ICollection<CredentialAttribute> Attributes { get; }
    }
}
