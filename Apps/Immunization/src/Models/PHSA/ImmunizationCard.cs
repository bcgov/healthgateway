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
namespace HealthGateway.Immunization.Models.PHSA
{
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Models.PHSA;

    /// <summary>
    /// Represents an Immunization Card.
    /// </summary>
    public class ImmunizationCard
    {
        /// <summary>
        /// Gets or sets the wallet card media.
        /// </summary>
        [JsonPropertyName("walletCard")]
        public EncodedMedia WalletCard { get; set; } = new ();

        /// <summary>
        /// Gets or sets the paper media.
        /// </summary>
        [JsonPropertyName("paperRecord")]
        public EncodedMedia PaperRecord { get; set; } = new ();

        /// <summary>
        /// Gets or sets the QR code associated.
        /// </summary>
        [JsonPropertyName("qrCode")]
        public EncodedMedia QRCode { get; set; } = new ();
    }
}
