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
namespace HealthGateway.Admin.Models.Support
{
    /// <summary>
    /// Represents the retrieved immunization information.
    /// </summary>
    public class RetrieveImmunizationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the provided information matches.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the name on the retrieved address.
        /// </summary>
        public string NameOnAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the retrieved address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the immunization document encoded (TODO: verify that is comming from PHSA).
        /// </summary>
        public string EncodedDocument { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the immunization document encoded (TODO: verify that is comming from PHSA).
        /// </summary>
        public string EncodingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document name (TODO: verify that is needed).
        /// </summary>
        public string DocumentName { get; set; } = string.Empty;
    }
}
