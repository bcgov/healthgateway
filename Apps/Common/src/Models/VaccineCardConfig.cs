// -------------------------------------------------------------------------
//  Copyright © 2019 Province of British Columbia
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// -------------------------------------------------------------------------

namespace HealthGateway.Common.Models
{
    using HealthGateway.Common.Constants;

    /// <summary>
    /// Defines the Vaccine Card template Configuration model.
    /// </summary>
    public class VaccineCardConfig
    {
        /// <summary>
        /// Gets or sets the value for vaccine card config section key.
        /// </summary>
        public const string ConfigSectionKey = "VaccineCard";

        /// <summary>
        /// Gets or sets the name of the print template.
        /// </summary>
        public VaccineProofTemplate PrintTemplate { get; set; }

        /// <summary>
        /// Gets or sets the name of the mail template.
        /// </summary>
        public VaccineProofTemplate MailTemplate { get; set; }
    }
}
