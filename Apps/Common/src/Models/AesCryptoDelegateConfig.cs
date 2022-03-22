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
    /// <summary>
    /// The configuration for the AESCrypto delegate.
    /// </summary>
    public class AesCryptoDelegateConfig
    {
        /// <summary>
        /// The default key size used for key AES crypto functions.
        /// </summary>
        public const int DefaultKeySize = 256;

        /// <summary>
        /// Gets or sets the key size for AES crypto functions.
        /// </summary>
        public int KeySize { get; set; } = DefaultKeySize;

        /// <summary>
        /// Gets or sets the Initialization Vector.
        /// </summary>
        public string? Iv { get; set; }
    }
}
