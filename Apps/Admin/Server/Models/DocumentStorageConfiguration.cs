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
namespace HealthGateway.Admin.Server.Models
{
    /// <summary>
    /// Configuration data for external document storage.
    /// </summary>
    public class DocumentStorageConfiguration
    {
        /// <summary>
        /// Gets or sets the hostname for the server where documents will be stored.
        /// </summary>
        public string SftpHostname { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the port for the server where documents will be stored.
        /// </summary>
        public int SftpPort { get; set; } = 22;

        /// <summary>
        /// Gets or sets the username for accessing the server where documents will be stored.
        /// </summary>
        public string SftpUsername { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to the folder where documents will be stored on the server.
        /// </summary>
        public string SftpFolderPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the local path to the private key.
        /// </summary>
        public string PrivateKeyPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the passphrase for accessing the private key.
        /// </summary>
        public string PrivateKeyPassphrase { get; set; } = string.Empty;
    }
}
