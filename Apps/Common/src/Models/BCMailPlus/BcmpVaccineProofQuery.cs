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
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Constants;

    /// <summary>
    /// The BC Mail Plus Vaccine Proof query model.
    /// </summary>
    public class BcmpVaccineProofQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpVaccineProofQuery"/> class.
        /// </summary>
        public BcmpVaccineProofQuery()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BcmpVaccineProofQuery"/> class.
        /// </summary>
        /// <param name="templates">The list of templates.</param>
        [JsonConstructor]
        public BcmpVaccineProofQuery(IList<VaccineProofTemplate> templates)
        {
            this.Templates = templates;
        }

        /// <summary>
        /// Gets or sets the schema version associated with the request.
        /// </summary>
        [JsonPropertyName("schemaVersion")]
        public string SchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operation that should be performed.
        /// </summary>
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vaccine status of the patient.
        /// </summary>
        [JsonPropertyName("vaccineStatus")]
        public VaccinationStatus VaccineStatus { get; set; }

        /// <summary>
        /// Gets or sets the Smart Health Card data for the patient.
        /// </summary>
        [JsonPropertyName("shc")]
        public BcmpSmartHealthCard SmartHealthCard { get; set; } = new();

        /// <summary>
        /// Gets the templates that will be used to generate the document.
        /// </summary>
        [JsonPropertyName("templates")]
        public IList<VaccineProofTemplate> Templates { get; } = new List<VaccineProofTemplate>();

        /// <summary>
        /// Gets or sets the address where the document should be mailed.
        /// </summary>
        [JsonPropertyName("address")]
        public BcmpAddress Address { get; set; } = new();
    }
}
