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
namespace HealthGateway.Common.Models.ODR
{
    /// <summary>
    /// The configuration for ODR certificate usage.
    /// </summary>
    public class OdrCertificateConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether certificate authentication should be used.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the path to the certificate file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the certificate file.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
