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
    using DeepEqual.Syntax;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Cacheable;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    /// <summary>
    /// HmacHashDelegate's Unit Tests.
    /// </summary>
    public class HmacHashDelegateTests
    {
        /// <summary>
        /// HmacHashDelegate - Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyConfigurationBinding()
        {
            HmacHashDelegateConfig expectedConfig = new()
            {
                PseudoRandomFunction = KeyDerivationPrf.HMACSHA1,
                Iterations = 100,
                SaltLength = 8,
            };

            Dictionary<string, string?> myConfiguration = new()
            {
                { "HmacHash:PseudoRandomFunction", expectedConfig.PseudoRandomFunction.ToString() },
                { "HmacHash:Iterations", expectedConfig.Iterations.ToString(CultureInfo.CurrentCulture) },
                { "HmacHash:SaltLength", expectedConfig.SaltLength.ToString(CultureInfo.CurrentCulture) },
            };

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HmacHashDelegate hashDelegate = new(configuration);

            hashDelegate.HashConfig.ShouldDeepEqual(expectedConfig);
        }

        /// <summary>
        /// HmacHashDelegate - Default Configuration Binding.
        /// </summary>
        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            HmacHashDelegateConfig expectedConfig = new()
            {
                PseudoRandomFunction = HmacHashDelegateConfig.DefaultPseudoRandomFunction,
                Iterations = HmacHashDelegateConfig.DefaultIterations,
                SaltLength = HmacHashDelegateConfig.DefaultSaltLength,
            };

            // test empty configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            HmacHashDelegate hashDelegate = new(configuration);

            hashDelegate.HashConfig.ShouldDeepEqual(expectedConfig);
        }

        /// <summary>
        /// Hash - Happy Path.
        /// </summary>
        [Fact]
        public void VerifyHash()
        {
            // default configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            HmacHashDelegate hashDelegate = new(configuration);
            IHash hash = hashDelegate.Hash("qwerty");

            Assert.True(hashDelegate.Compare("qwerty", hash));
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA1).
        /// </summary>
        [Fact]
        public void VerifyHmacSha1Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "s+QcBH+1K1kIPk2GnGMPMJFzBVA=";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA1);

            Assert.Equal(expectedValue, hash.Hash);
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA256).
        /// </summary>
        [Fact]
        public void VerifyHmacSha256Hash()
        {
            string valueToHash = "qwerty";
            string salt = "GPQ/DRGs6RSYjOh1EE1CZwKNCqCP8Zhb4DAhczEQYpE=";
            string expectedValue = "eY+j2PXHewxRiVrz+ngCEfwHXqsmF151Y3M+xrL2HlM=";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA256);

            Assert.Equal(expectedValue, hash.Hash);
        }

        /// <summary>
        /// Hash - Happy Path (HMACSHA512).
        /// </summary>
        [Fact]
        public void VerifyHmacSha512Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "EYxkddpZRM2KTR+fjT8G9jA2bYtjUMSrr8CfOgWyI2VXUYU3LrPPC2F9kVx7mRoGR0YaDEZppXXvkgCymDKWJQ==";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt));

            Assert.Equal(expectedValue, hash.Hash);
        }

        /// <summary>
        /// Hash - Happy Path (Null Key).
        /// </summary>
        [Fact]
        public void VerifyNullKey()
        {
            string? valueToHash = null;

            // default configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>().ToList())
                .Build();

            HmacHashDelegate hashDelegate = new(configuration);
            IHash hash = hashDelegate.Hash(valueToHash);

            Assert.True(hashDelegate.Compare(valueToHash, hash));
        }
    }
}
