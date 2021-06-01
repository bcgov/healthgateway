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
    /// Represents a Credential Definition Id from the aries agent.
    /// </summary>
    public class CredentialDefinitionIdResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialDefinitionIdResponse"/> class.
        /// </summary>
        public CredentialDefinitionIdResponse()
        {
            this.CredentialDefinitionIds = new Collection<string>();
        }

        /// <summary>
        /// Gets the credential definition ids.
        /// </summary>
        [JsonPropertyName("credential_definition_ids")]
        public ICollection<string> CredentialDefinitionIds { get; }
    }
}
