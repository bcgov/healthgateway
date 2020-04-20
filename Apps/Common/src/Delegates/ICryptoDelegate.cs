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

namespace HealthGateway.Common.Delegates
{
    using HealthGateway.Database.Models.Cacheable;

    /// <summary>
    /// A delegate to encrypted and decrypt text.
    /// </summary>
    public interface ICryptoDelegate
    {
        /// <summary>
        /// Encrypts the plaintext using the key supplied.
        /// </summary>
        /// <param name="key">The base64 encoded encryption key.</param>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>The encrypted text.</returns>
        public string Encrypt(string key, string plainText);

        /// <summary>
        /// Decrypts the encrypted text using the key supplied.
        /// </summary>
        /// <param name="key">The base64 encoded encryption key.</param>
        /// <param name="encryptedText">The text to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        public string Decrypt(string key, string encryptedText);

        /// <summary>
        /// Generates a suitable key for use in the implementation Encrypt/Decrypt methods.
        /// </summary>
        /// <returns>A Base64 encoded key.</returns>
        public string GenerateKey();
    }
}
