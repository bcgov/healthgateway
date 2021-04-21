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
namespace HealthGateway.CommonTests.Delegates
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    /// <summary>
    /// AesCryptoDelegate's Unit Tests.
    /// </summary>
    public class AesCryptoDelegateTests
    {
        /// <summary>
        /// AesCryptoDelegateTests - Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyConfigurationBinding()
        {
            AesCryptoDelegateConfig expectedConfig = new AesCryptoDelegateConfig()
            {
                KeySize = 256,
                IV = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF")),
            };

            var myConfiguration = new Dictionary<string, string>
            {
                { "AESCrypto:KeySize", expectedConfig.KeySize.ToString(CultureInfo.CurrentCulture) },
                { "AESCrypto:IV", Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF")) },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            Assert.True(expectedConfig.IsDeepEqual(aesDelegate.AesConfig));
        }

        /// <summary>
        /// AesCryptoDelegateTests - Default Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            AesCryptoDelegateConfig expectedConfig = new AesCryptoDelegateConfig()
            {
                KeySize = AesCryptoDelegateConfig.DefaultKeySize,
            };

            var myConfiguration = new Dictionary<string, string>
            {
                // test empty configuration
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            Assert.True(expectedConfig.IsDeepEqual(aesDelegate.AesConfig));
        }

        /// <summary>
        /// GenerateKey - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyKeyGeneration()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "AESCrypto:KeySize", "128" },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = aesDelegate.GenerateKey();
            byte[] keyBytes = Convert.FromBase64String(key);

            Assert.True(keyBytes.Length == aesDelegate.AesConfig.KeySize / 8);
        }

        /// <summary>
        /// Encrypt - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyEncryption()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "Hello World";
            string expectedStr = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            Assert.True(expectedStr == encryptedStr);
        }

        /// <summary>
        /// Encrypt - Happy Path (Validate Length 100).
        /// </summary>
        [Fact]
        public void VerifyEncrypedStringLength100()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / 3D) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Encrypt - Happy Path (Validate Length 1000).
        /// </summary>
        [Fact]
        public void VerifyEncrypedStringLength1000()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R" +
                               "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R";

            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / 3D) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Decrypt - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyDecryption()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string cipherText = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, cipherText);

            Assert.True(expectedStr == decryptedStr);
        }

        /// <summary>
        /// Encrypt - Happy Path (With IV).
        /// </summary>
        [Fact]
        public void VerifyEncryptionWithIV()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string plainText = "Hello World";
            string expectedStr = "ct9piIgrLBasmnfNPLcHRA==";
            string encryptedStr = aesDelegate.Encrypt(key, iv, plainText);

            Assert.True(expectedStr == encryptedStr);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / 3D) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Decrypt - Happy Path (With IV).
        /// </summary>
        [Fact]
        public void VerifyDecryptionWithIV()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new AesCryptoDelegate(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string cipherText = "ct9piIgrLBasmnfNPLcHRA==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, iv, cipherText);

            Assert.True(expectedStr == decryptedStr);
        }
    }
}
