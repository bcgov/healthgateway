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
namespace HealthGateway.WebClient.Models
{
    using System;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of an WalletCredentialModel.
    /// </summary>
    public class WalletCredentialModel
    {
        /// <summary>
        /// Gets or sets the verifiable credential id.
        /// </summary>
        public Guid CredentialId { get; set; }

        /// <summary>
        /// Gets or sets the wallet connection id.
        /// </summary>
        public Guid WalletConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the source id.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer connection id.
        /// </summary>
        public Guid? IssuerConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the connection date.
        /// </summary>
        public DateTime? AddedDate { get; set; }

        /// <summary>
        /// Gets or sets the disconnection date.
        /// </summary>
        public DateTime? RevokedDate { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public WalletCredentialStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the db record version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Constructs a WalletCredentialModel from a database model.
        /// </summary>
        /// <param name="walletCredential">The wallet credential database model.</param>
        /// <returns>The verifiable credential model.</returns>
        public static WalletCredentialModel CreateFromDbModel(WalletCredential walletCredential)
        {
            return new WalletCredentialModel()
            {
                CredentialId = walletCredential.Id,
                WalletConnectionId = walletCredential.WalletConnectionId,
                IssuerConnectionId = walletCredential.WalletConnection.AgentId,
                Status = walletCredential.Status,
                SourceId = walletCredential.ResourceId,
                SourceType = walletCredential.ResourceType,
                AddedDate = walletCredential.AddedDateTime,
                RevokedDate = walletCredential.RevokedDateTime,
                Version = walletCredential.Version,
            };
        }
    }
}
