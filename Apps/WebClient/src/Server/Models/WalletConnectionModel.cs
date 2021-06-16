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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.Json.Serialization;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models;

    /// <summary>
    /// Model that provides a user representation of an verifiable credential connection model.
    /// </summary>
    public class WalletConnectionModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WalletConnectionModel"/> class.
        /// </summary>
        public WalletConnectionModel()
        {
            this.Credentials = new List<WalletCredentialModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletConnectionModel"/> class.
        /// </summary>
        /// <param name="credentials">The list of credentials.</param>
        [JsonConstructor]
        public WalletConnectionModel(IList<WalletCredentialModel> credentials)
        {
            this.Credentials = credentials ?? new List<WalletCredentialModel>();
        }

        /// <summary>
        /// Gets or sets the wallet connection id.
        /// </summary>
        public Guid WalletConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the user hdid.
        /// </summary>
        public string Hdid { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the issuer connection id.
        /// </summary>
        public Guid? IssuerConnectionId { get; set; }

        /// <summary>
        /// Gets or sets the connection date.
        /// </summary>
        public DateTime? ConnectedDate { get; set; }

        /// <summary>
        /// Gets or sets the disconnection date.
        /// </summary>
        public DateTime? DisconnectedDate { get; set; }

        /// <summary>
        /// Gets or sets the QR code data.
        /// </summary>
        public string? InvitationEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public WalletConnectionStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the db record version.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets the collection of credentials for this connection.
        /// </summary>
        public IList<WalletCredentialModel> Credentials { get; }

        /// <summary>
        /// Constructs a WalletConnectionModel from a database model.
        /// </summary>
        /// <param name="dbWalletConnection">The wallet connection database model.</param>
        /// <returns>The verifiable credential connection model.</returns>
        public static WalletConnectionModel CreateFromDbModel(WalletConnection dbWalletConnection)
        {
            return new WalletConnectionModel(
                dbWalletConnection.Credentials?
                    .Select(walletCredential => WalletCredentialModel.CreateFromDbModel(walletCredential))
                    .ToList())
            {
                WalletConnectionId = dbWalletConnection.Id,
                IssuerConnectionId = dbWalletConnection.AgentId,
                Hdid = dbWalletConnection.UserProfileId,
                ConnectedDate = dbWalletConnection.ConnectedDateTime,
                DisconnectedDate = dbWalletConnection.DisconnectedDateTime,
                Status = dbWalletConnection.Status,
                Version = dbWalletConnection.Version,
                InvitationEndpoint = dbWalletConnection.InvitationEndpoint,
            };
        }
    }
}
