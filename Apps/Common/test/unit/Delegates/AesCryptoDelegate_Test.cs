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
    using System.Collections.Generic;
    using DeepEqual.Syntax;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using System;
    using System.Text;

    public class AesCryptoDelegate_Test
    {
        [Fact]
        public void VerifyConfigurationBinding()
        {
            AESCryptoDelegateConfig expectedConfig = new AESCryptoDelegateConfig()
            {
                KeySize = 256,
                IV = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF")),
            };

            var myConfiguration = new Dictionary<string, string>
            {
                {"AESCrypto:KeySize", expectedConfig.KeySize.ToString()},
                {"AESCrypto:IV", Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"))},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            Assert.True(expectedConfig.IsDeepEqual(aesDelegate.AesConfig));
        }

        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            AESCryptoDelegateConfig expectedConfig = new AESCryptoDelegateConfig()
            {
                KeySize = AESCryptoDelegateConfig.DefaultKeySize,
            };

            var myConfiguration = new Dictionary<string, string>
            {
                //test empty configuration
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            Assert.True(expectedConfig.IsDeepEqual(aesDelegate.AesConfig));
        }

        [Fact]
        public void VerifyKeyGeneration()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                {"AESCrypto:KeySize", "128"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = aesDelegate.GenerateKey();
            byte[] keyBytes = Convert.FromBase64String(key);

            Assert.True(keyBytes.Length == aesDelegate.AesConfig.KeySize / 8);
        }

        [Fact]
        public void VerifyEncryption()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "Hello World";
            string expectedStr = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            Assert.True(expectedStr == encryptedStr);
        }

        [Fact]
        public void VerifyEncrypedStringLength100()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string plainText = "TSm73LxxXt21TOtu6HBHGPOBqHTFKf4VIMxGWoJrCtyHVdcNwSBdh8F86C9Jwjn2aWRdElQyC3PsuRMA2IXxvQ7m9oHHfm5woo5R";
            string encryptedStr = aesDelegate.Encrypt(key, plainText);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / (double)3) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        [Fact]
        public void VerifyEncrypedStringLength1000()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

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
            double expectedEncodedSize = Math.Ceiling(expectedSize / (double)3) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        [Fact]
        public void VerifyDecryption()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));

            string cipherText = "m8zvM4eT5e4Wn1cJvbo+WQ==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, cipherText);

            Assert.True(expectedStr == decryptedStr);
        }

        [Fact]
        public void VerifyEncryptionwithIV()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string plainText = "Hello World";
            string expectedStr = "ct9piIgrLBasmnfNPLcHRA==";
            string encryptedStr = aesDelegate.Encrypt(key, iv, plainText);

            Assert.True(expectedStr == encryptedStr);

            int blockSize = 16;
            int expectedSize = plainText.Length + (blockSize - (plainText.Length % blockSize));
            double expectedEncodedSize = Math.Ceiling(expectedSize / (double)3) * 4;
            byte[] encryptedBytes = Convert.FromBase64String(encryptedStr);

            Assert.True(expectedSize == encryptedBytes.Length && expectedEncodedSize >= encryptedStr.Length);
        }

        [Fact]
        public void VerifyDecryptionwithIV()
        {
            var myConfiguration = new Dictionary<string, string>
            {
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            AESCryptoDelegate aesDelegate = new AESCryptoDelegate(
                new Mock<ILogger<AESCryptoDelegate>>().Object,
                configuration);

            string key = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUV"));
            string iv = Convert.ToBase64String(Encoding.ASCII.GetBytes("0123456789ABCDEF"));

            string cipherText = "ct9piIgrLBasmnfNPLcHRA==";
            string expectedStr = "Hello World";
            string decryptedStr = aesDelegate.Decrypt(key, iv, cipherText);

            Assert.True(expectedStr == decryptedStr);
        }
    }
}
