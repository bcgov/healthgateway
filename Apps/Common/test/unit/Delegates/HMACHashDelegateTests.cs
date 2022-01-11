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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Models.Cacheable;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// HMACHashDelegate's Unit Tests.
    /// </summary>
    public class HMACHashDelegateTests
    {
        /// <summary>
        /// HMACHashDelegate - Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyConfigurationBinding()
        {
            HMACHashDelegateConfig expectedConfig = new()
            {
                PseudoRandomFunction = KeyDerivationPrf.HMACSHA1,
                Iterations = 100,
                SaltLength = 8,
            };

            Dictionary<string, string> myConfiguration = new()
            {
                { "HMACHash:PseudoRandomFunction", expectedConfig.PseudoRandomFunction.ToString() },
                { "HMACHash:Iterations", expectedConfig.Iterations.ToString(CultureInfo.CurrentCulture) },
                { "HMACHash:SaltLength", expectedConfig.SaltLength.ToString(CultureInfo.CurrentCulture) },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HMACHashDelegate hashDelegate = new(configuration);

            expectedConfig.ShouldDeepEqual(hashDelegate.HashConfig);
        }

        /// <summary>
        /// HMACHashDelegate - Default Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            HMACHashDelegateConfig expectedConfig = new()
            {
                PseudoRandomFunction = HMACHashDelegateConfig.DefaultPseudoRandomFunction,
                Iterations = HMACHashDelegateConfig.DefaultIterations,
                SaltLength = HMACHashDelegateConfig.DefaultSaltLength,
            };

            // test empty configuration
            Dictionary<string, string> myConfiguration = new();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HMACHashDelegate hashDelegate = new(configuration);

            expectedConfig.ShouldDeepEqual(hashDelegate.HashConfig);
        }

        /// <summary>
        /// Hash - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyHash()
        {
            // default configuration
            Dictionary<string, string> myConfiguration = new();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HMACHashDelegate hashDelegate = new(configuration);
            IHash hash = hashDelegate.Hash("qwerty");

            Assert.True(hashDelegate.Compare("qwerty", hash));
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA1).
        /// </summary>
        [Fact]
        public void VerifyHMACSHA1Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "s+QcBH+1K1kIPk2GnGMPMJFzBVA=";
            IHash hash = HMACHashDelegate.HMACHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA1);

            Assert.True(hash.Hash == expectedValue);
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA256).
        /// </summary>
        [Fact]
        public void VerifyHMACSHA256Hash()
        {
            string valueToHash = "qwerty";
            string salt = "GPQ/DRGs6RSYjOh1EE1CZwKNCqCP8Zhb4DAhczEQYpE=";
            string expectedValue = "eY+j2PXHewxRiVrz+ngCEfwHXqsmF151Y3M+xrL2HlM=";
            IHash hash = HMACHashDelegate.HMACHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA256);

            Assert.True(hash.Hash == expectedValue);
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA512).
        /// </summary>
        [Fact]
        public void VerifyHMACSHA512Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "EYxkddpZRM2KTR+fjT8G9jA2bYtjUMSrr8CfOgWyI2VXUYU3LrPPC2F9kVx7mRoGR0YaDEZppXXvkgCymDKWJQ==";
            IHash hash = HMACHashDelegate.HMACHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA512);

            Assert.True(hash.Hash == expectedValue);
        }

        /// <summary>
        /// Hash - Happy Path (Null Key).
        /// </summary>
        [Fact]
        public void VerifyNullKey()
        {
            string? valueToHash = null;

            // default configuration
            Dictionary<string, string> myConfiguration = new();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HMACHashDelegate hashDelegate = new(configuration);
            IHash hash = hashDelegate.Hash(valueToHash);

            Assert.True(hashDelegate.Compare(valueToHash, hash));
        }
    }
}
