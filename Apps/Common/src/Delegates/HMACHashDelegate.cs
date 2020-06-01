﻿//-------------------------------------------------------------------------
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
    using System.Security.Cryptography;
    using HealthGateway.Common.Models;
    using HealthGateway.Database.Constants;
    using HealthGateway.Database.Models.Cacheable;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Delegate to create and validate HMAC hashes.
    /// </summary>
    public class HMACHashDelegate : IHashDelegate
    {
        private const string ConfigKey = "HMACHash";
        private readonly ILogger<HMACHashDelegate> logger;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="HMACHashDelegate"/> class.
        /// </summary>
        /// <param name="logger">Injected Logger Provider.</param>
        /// <param name="configuration">The injected configuration provider.</param>
        public HMACHashDelegate(
            ILogger<HMACHashDelegate> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.HashConfig = new HMACHashDelegateConfig();
            this.configuration.Bind(ConfigKey, this.HashConfig);
        }

        /// <summary>
        /// Gets or sets the instance configuration.
        /// </summary>
        public HMACHashDelegateConfig HashConfig { get; set; }

        /// <summary>
        /// Generates the HMAC Hash object.
        /// </summary>
        /// <param name="key">The string to hash.</param>
        /// <param name="salt">The salt to use for hashing.</param>
        /// <param name="prf">The pseudorandom function to use for the hash.</param>
        /// <param name="iterations">The number of iterations to process over the hash.</param>
        /// <returns>The hash object.</returns>
        public static HMACHash HMACHash(
            string? key,
            byte[] salt,
            KeyDerivationPrf prf = HMACHashDelegateConfig.DefaultPseudoRandomFunction,
            int iterations = HMACHashDelegateConfig.DefaultIterations)
        {
            HMACHash retHash = new HMACHash()
            {
                PseudoRandomFunction = HashFunction.HMACSHA512,
                Iterations = iterations,
            };

            if (!string.IsNullOrEmpty(key))
            {
                // The key is not null, so we can generate a hash
                // Calculate the length in bytes of the hash given the function size
                int hashLength = prf == KeyDerivationPrf.HMACSHA1 ? 20 : prf == KeyDerivationPrf.HMACSHA256 ? 32 : 64;
                retHash.Salt = Convert.ToBase64String(salt);
                retHash.Hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                                        password: key,
                                                        salt: salt,
                                                        prf: prf,
                                                        iterationCount: iterations,
                                                        numBytesRequested: hashLength));
            }

            return retHash;
        }

        /// <summary>
        /// Validates that the supplied key will hash to the compareHash.
        /// </summary>
        /// <param name="key">The key to hash and compare.</param>
        /// <param name="compareHash">The hash object to compare.</param>
        /// <returns>true if the key generates the same hash.</returns>
        public static bool Compare(string? key, HMACHash? compareHash)
        {
            bool result = false;
            if (key != null && compareHash != null && compareHash.Hash != null && compareHash.Salt != null)
            {
                HMACHash keyHash = HMACHash(
                    key,
                    Convert.FromBase64String(compareHash.Salt),
                    (KeyDerivationPrf)compareHash.PseudoRandomFunction,
                    compareHash.Iterations);
                result = compareHash.Hash == keyHash.Hash;
            }
            else
            {
                // If the key is null and the hash is null then they are the same
                result = compareHash != null && compareHash.Hash == null && key == null;
            }

            return result;
        }

        /// <summary>
        /// Creates a secure random salt of the specified length in bytes.
        /// </summary>
        /// <param name="saltLength">The length of the salt in bytes, defaults to 16 bytes.</param>
        /// <returns>A byte array containing the salt.</returns>
        public static byte[] GenerateSalt(int saltLength = HMACHashDelegateConfig.DefaultSaltLength)
        {
            byte[] salt = new byte[saltLength];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <inheritdoc />
        public IHash Hash(string? key)
        {
            return this.HMACHash(key);
        }

        /// <inheritdoc />
        public bool Compare(string? key, IHash compareHash)
        {
            return Compare(key, compareHash as HMACHash);
        }

        /// <summary>
        /// Creates a hash given the instance configuration.
        /// </summary>
        /// <param name="key">The string to hash.</param>
        /// <returns>The newly created HMAC Hash.</returns>
        public HMACHash HMACHash(string? key)
        {
            return HMACHash(
                key,
                GenerateSalt(this.HashConfig.SaltLength),
                this.HashConfig.PseudoRandomFunction,
                this.HashConfig.Iterations);
        }
    }
}
