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
namespace HealthGateway.MedicationService.Models
{
    /// <summary>
    /// The HNClient claims standard message header configuration.
    /// </summary>
    public class ZCAConfiguration
    {
        /// <summary>
        /// Gets or sets the software id.
        /// </summary>
        public string SoftwareId { get; set; }

        /// <summary>
        /// Gets or sets the software version.
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// Gets or sets the BIN.
        /// </summary>
        public string BIN { get; set; }

        /// <summary>
        /// Gets or sets the CPHA version number.
        /// </summary>
        public string CPHAVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }
    }
}
