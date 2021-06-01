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

    /// <summary>
    /// A verifiable credential for an Immunization.
    /// </summary>
    public abstract class CredentialPayload
    {
        /// <summary>
        /// Gets or sets the schema name to be used for the Credential payload.
        /// </summary>
        public string? SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the schema version to be used for the Credential payload.
        /// </summary>
        public string? SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the full legal name of the recipient.
        /// </summary>
        public string? RecipientName { get; set; }

        /// <summary>
        /// Gets or sets the birthdate of the recipient.
        /// </summary>
        public DateTime? RecipientBirthDate { get; set; }

        /// <summary>
        /// Gets or sets the Personal Health Number of the recipient.
        /// </summary>
        public string? RecipientPHN { get; set; }
    }
}
