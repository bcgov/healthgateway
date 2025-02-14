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
    using System.Linq;
    using System.Text;
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using Microsoft.Extensions.Configuration;
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
            AesCryptoDelegateConfig expectedConfig = new()
            {
                KeySize = 256,
                Iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF")),
            };

            Dictionary<string, string?> myConfiguration = new()
            {
                { "AESCrypto:KeySize", expectedConfig.KeySize.ToString(CultureInfo.CurrentCulture) },
                { "AESCrypto:Iv", Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF")) },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            aesDelegate.AesConfig.ShouldDeepEqual(expectedConfig);
        }

        /// <summary>
        /// AesCryptoDelegateTests - Default Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            AesCryptoDelegateConfig expectedConfig = new()
            {
                KeySize = AesCryptoDelegateConfig.DefaultKeySize,
            };

            // test empty configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            aesDelegate.AesConfig.ShouldDeepEqual(expectedConfig);
        }

        /// <summary>
        /// GenerateKey - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyKeyGeneration()
        {
            Dictionary<string, string?> myConfiguration = new()
            {
                { "AESCrypto:KeySize", "128" },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = aesDelegate.GenerateKey();
            byte[] keyBytes = Convert.FromBase64String(key);

            Assert.Equal(aesDelegate.AesConfig.KeySize / 8, keyBytes.Length);
        }

        /// <summary>
        /// Encrypt - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyEncryption()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "Hello World";
            string expectedStr = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            Assert.Equal(expectedStr, encryptedStr);
        }

        /// <summary>
        /// Encrypt - Happy Path (Validate Length 100).
        /// </summary>
        [Fact]
        public void VerifyEncryptedStringLength100()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / 3D) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.Equal(expectedSize, encryptedBytes.Length);
            Assert.True(expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Encrypt - Happy Path (Validate Length 1000).
        /// </summary>
        [Fact]
        public void VerifyEncryptedStringLength1000()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

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

            Assert.Equal(expectedSize, encryptedBytes.Length);
            Assert.True(expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Decrypt - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyDecryption()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string cipherText = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, cipherText);

            Assert.Equal(expectedStr, decryptedStr);
        }

        /// <summary>
        /// Encrypt - Happy Path (With Iv).
        /// </summary>
        [Fact]
        public void VerifyEncryptionWithIv()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string plainText = "Hello World";
            string expectedStr = "ct9piIgrLBasmnfNPLcHRA==";
            string encryptedStr = aesDelegate.Encrypt(key, iv, plainText);

            Assert.Equal(expectedStr, encryptedStr);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / 3D) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.Equal(expectedSize, encryptedBytes.Length);
            Assert.True(expectedEncodedSize >= encryptedStr.Length);
        }

        /// <summary>
        /// Decrypt - Happy Path (With Iv).
        /// </summary>
        [Fact]
        public void VerifyDecryptionWithIv()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            AesCryptoDelegate aesDelegate = new(configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string cipherText = "ct9piIgrLBasmnfNPLcHRA==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, iv, cipherText);

            Assert.Equal(expectedStr, decryptedStr);
        }
    }
}
