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

    public class HmacHashDelegate_Test
    {
        [Fact]
        public void VerifyConfigurationBinding()
        {
            HmacHashDelegateConfig expectedConfig = new HmacHashDelegateConfig()
            {
                PseudoRandomFunction = Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA1,
                Iterations = 100,
                SaltLength = 8,
            };

            var myConfiguration = new Dictionary<string, string>
            {
                { "HMACHash:PseudoRandomFunction", expectedConfig.PseudoRandomFunction.ToString() },
                { "HMACHash:Iterations", expectedConfig.Iterations.ToString(CultureInfo.CurrentCulture) },
                { "HMACHash:SaltLength", expectedConfig.SaltLength.ToString(CultureInfo.CurrentCulture) },
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HmacHashDelegate hashDelegate = new HmacHashDelegate(configuration);

            Assert.True(expectedConfig.IsDeepEqual(hashDelegate.HashConfig));
        }

        [Fact]
        public void VerifyDefaultConfigurationBinding()
        {
            HmacHashDelegateConfig expectedConfig = new HmacHashDelegateConfig()
            {
                PseudoRandomFunction = HmacHashDelegateConfig.DefaultPseudoRandomFunction,
                Iterations = HmacHashDelegateConfig.DefaultIterations,
                SaltLength = HmacHashDelegateConfig.DefaultSaltLength,
            };

            var myConfiguration = new Dictionary<string, string>
            {
                // test empty configuration
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HmacHashDelegate hashDelegate = new HmacHashDelegate(configuration);

            Assert.True(expectedConfig.IsDeepEqual(hashDelegate.HashConfig));
        }

        [Fact]
        public void VerifyHash()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                // default configuration
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HmacHashDelegate hashDelegate = new HmacHashDelegate(configuration);
            IHash hash = hashDelegate.Hash("qwerty");

            Assert.True(hashDelegate.Compare("qwerty", hash));
        }

        [Fact]
        public void VerifyHMACSHA1Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "s+QcBH+1K1kIPk2GnGMPMJFzBVA=";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA1);

            Assert.True(hash.Hash == expectedValue);
        }

        [Fact]
        public void VerifyHMACSHA256Hash()
        {
            string valueToHash = "qwerty";
            string salt = "GPQ/DRGs6RSYjOh1EE1CZwKNCqCP8Zhb4DAhczEQYpE=";
            string expectedValue = "eY+j2PXHewxRiVrz+ngCEfwHXqsmF151Y3M+xrL2HlM=";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA256);

            Assert.True(hash.Hash == expectedValue);
        }

        [Fact]
        public void VerifyHMACSHA512Hash()
        {
            string valueToHash = "qwerty";
            string salt = "/KzcYC/PC4c4ucgriabmhA==";
            string expectedValue = "EYxkddpZRM2KTR+fjT8G9jA2bYtjUMSrr8CfOgWyI2VXUYU3LrPPC2F9kVx7mRoGR0YaDEZppXXvkgCymDKWJQ==";
            IHash hash = HmacHashDelegate.HmacHash(valueToHash, Convert.FromBase64String(salt), KeyDerivationPrf.HMACSHA512);

            Assert.True(hash.Hash == expectedValue);
        }

        [Fact]
        public void VerifyNullKey()
        {
            string? valueToHash = null;
            var myConfiguration = new Dictionary<string, string>
            {
                // default configuration
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            HmacHashDelegate hashDelegate = new HmacHashDelegate(configuration);
            IHash hash = hashDelegate.Hash(valueToHash);

            Assert.True(hashDelegate.Compare(valueToHash, hash));
        }
    }
}
