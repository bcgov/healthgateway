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
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Delegate to encrypt/decrypt using AES.
    /// </summary>
    public class AESCryptoDelegate : ICryptoDelegate
    {
        private const string ConfigKey = "AESCrypto";
        private readonly ILogger<AESCryptoDelegate> logger;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AESCryptoDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public AESCryptoDelegate(
            ILogger<AESCryptoDelegate> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.AesConfig = new AESCryptoDelegateConfig();
            this.configuration.Bind(ConfigKey, this.AesConfig);
        }

        /// <summary>
        /// Gets or sets the instance configuration.
        /// </summary>
        public AESCryptoDelegateConfig AesConfig { get; set; }

        /// <inheritdoc />
        public string Encrypt(string key, string plainText)
        {
            return this.Encrypt(key, this.AesConfig.IV, plainText);
        }

        /// <summary>
        /// Encrypts the plaintext using the key and initialization vector supplied.
        /// </summary>
        /// <param name="key">The base64 encoded encryption key.</param>
        /// <param name="iv">The base64 encoded initialization vector.</param>
        /// <param name="plainText">The text to encrypt.</param>
        /// <returns>The encrypted text.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5401:Do not use CreateEncryptor with non-default IV", Justification = "Team decision")]
        public string Encrypt(string key, string? iv, string plainText)
        {
            using Aes aes = Aes.Create();
            aes.KeySize = this.AesConfig.KeySize;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = iv != null ? Convert.FromBase64String(iv) : new byte[16];
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(plainText);
            swEncrypt.Close();
            byte[] encryptedBytes = msEncrypt.ToArray();
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <inheritdoc />
        public string Decrypt(string key, string encryptedText)
        {
            return this.Decrypt(key, this.AesConfig.IV, encryptedText);
        }

        /// <summary>
        /// Decrypts the encrypted text using the key and initialization vector supplied.
        /// </summary>
        /// <param name="key">The base64 encoded encryption key.</param>
        /// <param name="iv">The base64 encoded initialization vector.</param>
        /// <param name="encryptedText">The text to decrypt.</param>
        /// <returns>The decrypted text.</returns>
        public string Decrypt(string key, string? iv, string encryptedText)
        {
            string? plaintext = null;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            using Aes aes = Aes.Create();
            aes.KeySize = this.AesConfig.KeySize;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = iv != null ? Convert.FromBase64String(iv) : new byte[16];
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using MemoryStream msDecrypt = new MemoryStream(encryptedBytes);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();
            return plaintext;
        }

        /// <inheritdoc />
        public string GenerateKey()
        {
            using Aes aes = Aes.Create();
            aes.KeySize = this.AesConfig.KeySize;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
    }
}
