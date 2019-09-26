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
    /// The HNClient message configuration.
    /// </summary>
    public class HNClientConfiguration
    {
#pragma warning disable SA1310 // Field names should not contain underscore
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// The claims standard message header segment.
        /// </summary>
        public const string SEGMENT_ZCA = "ZCA";

        /// <summary>
        /// The provider information segment.
        /// </summary>
        public const string SEGMENT_ZCB = "ZCB";

        /// <summary>
        /// The beneficiary information segment.
        /// </summary>
        public const string SEGMENT_ZCC = "ZCC";

        /// <summary>
        /// The transaction control segment.
        /// </summary>
        public const string SEGMENT_ZZZ = "ZZZ";

        /// <summary>
        /// The patient profile transaction id.
        /// </summary>
        public const string PATIENT_PROFILE_TRANSACTION_ID = "TRP";

        /// <summary>
        /// The patient profile message type.
        /// </summary>
        public const string PATIENT_PROFILE_MESSAGE_TYPE = "ZPN";
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore CA1707 // Identifiers should not contain underscores

        /// <summary>
        /// Gets or sets the provider information configuration.
        /// </summary>
        public ZcbConfiguration ZCB { get; set; }

        /// <summary>
        /// Gets or sets the claims standard message header configuration.
        /// </summary>
        public ZcaConfiguration ZCA { get; set; }

        /// <summary>
        /// Gets or sets the transaction control configuration.
        /// </summary>
        public ZzzConfiguration ZZZ { get; set; }

        /// <summary>
        /// Gets or sets the message version.
        /// </summary>
        public string MessageVersion { get; set; }

        /// <summary>
        /// Gets or sets the processing id.
        /// </summary>
        public string ProcessingID { get; set; }

        /// <summary>
        /// Gets or sets the sending application.
        /// </summary>
        public string SendingApplication { get; set; }

        /// <summary>
        /// Gets or sets the sending facility.
        /// </summary>
        public string SendingFacility { get; set; }

        /// <summary>
        /// Gets or sets the receiving application.
        /// </summary>
        public string ReceivingApplication { get; set; }

        /// <summary>
        /// Gets or sets the receiving facility.
        /// </summary>
        public string ReceivingFacility { get; set; }
    }
}
