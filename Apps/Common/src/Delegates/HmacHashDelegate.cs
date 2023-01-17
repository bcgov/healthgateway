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
    using System.Security.Cryptography;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Models.Cacheable;
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Delegate to create and validate HMAC hashes.
    /// </summary>
    public class HmacHashDelegate : IHashDelegate
    {
        private const string ConfigKey = "HmacHash";

        /// <summary>
        /// Initializes a new instance of the <see cref="HmacHashDelegate"/> class.
        /// </summary>
        /// <param name="configuration">The injected configuration provider.</param>
        public HmacHashDelegate(IConfiguration configuration)
        {
            this.HashConfig = new HmacHashDelegateConfig();
            configuration.Bind(ConfigKey, this.HashConfig);
        }

        /// <summary>
        /// Gets or sets the instance configuration.
        /// </summary>
        public HmacHashDelegateConfig HashConfig { get; set; }

        /// <summary>
        /// Generates the HMAC Hash object.
        /// </summary>
        /// <param name="key">The string to hash.</param>
        /// <param name="salt">The salt to use for hashing.</param>
        /// <param name="prf">The pseudorandom function to use for the hash.</param>
        /// <param name="iterations">The number of iterations to process over the hash.</param>
        /// <returns>The hash object.</returns>
        public static HmacHash HmacHash(
            string? key,
            byte[] salt,
            KeyDerivationPrf prf = HmacHashDelegateConfig.DefaultPseudoRandomFunction,
            int iterations = HmacHashDelegateConfig.DefaultIterations)
        {
            HmacHash retHash = new()
            {
                PseudoRandomFunction = HashFunction.HmacSha512,
                Iterations = iterations,
            };

            if (!string.IsNullOrEmpty(key))
            {
                // The key is not null, so we can generate a hash
                // Calculate the length in bytes of the hash given the function size
                int hashLength;
                switch (prf)
                {
                    case KeyDerivationPrf.HMACSHA1:
                        hashLength = 20;
                        break;
                    case KeyDerivationPrf.HMACSHA256:
                        hashLength = 32;
                        break;
                    default:
                        hashLength = 64;
                        break;
                }

                retHash.Salt = Convert.ToBase64String(salt);
                retHash.Hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(key, salt, prf, iterations, hashLength));
            }

            return retHash;
        }

        /// <summary>
        /// Creates a secure random salt of the specified length in bytes.
        /// </summary>
        /// <param name="saltLength">The length of the salt in bytes, defaults to 16 bytes.</param>
        /// <returns>A byte array containing the salt.</returns>
        public static byte[] GenerateSalt(int saltLength = HmacHashDelegateConfig.DefaultSaltLength)
        {
            byte[] salt = new byte[saltLength];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }

        /// <inheritdoc/>
        public IHash Hash(string? key)
        {
            return this.HmacHash(key);
        }

        /// <inheritdoc/>
        public bool Compare(string? key, IHash compareHash)
        {
            return Compare(key, compareHash as HmacHash);
        }

        /// <summary>
        /// Creates a hash given the instance configuration.
        /// </summary>
        /// <param name="key">The string to hash.</param>
        /// <returns>The newly created HMAC Hash.</returns>
        public HmacHash HmacHash(string? key)
        {
            return HmacHash(
                key,
                GenerateSalt(this.HashConfig.SaltLength),
                this.HashConfig.PseudoRandomFunction,
                this.HashConfig.Iterations);
        }

        private static bool Compare(string? key, HmacHash? compareHash)
        {
            bool result;
            if (key != null && compareHash != null && compareHash.Hash != null && compareHash.Salt != null)
            {
                HmacHash keyHash = HmacHash(
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
    }
}
