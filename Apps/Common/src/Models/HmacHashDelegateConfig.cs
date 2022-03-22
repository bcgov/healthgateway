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
namespace HealthGateway.Common.Models
{
    using Microsoft.AspNetCore.Cryptography.KeyDerivation;

    /// <summary>
    /// The configuration for the HmacHash delegate.
    /// </summary>
    public class HmacHashDelegateConfig
    {
        /// <summary>
        /// The default number of iterations for the hash.
        /// </summary>
        public const int DefaultIterations = 21013;

        /// <summary>
        /// The default PseudoRandomFunction to use for the hash generation.
        /// </summary>
        public const KeyDerivationPrf DefaultPseudoRandomFunction = KeyDerivationPrf.HMACSHA512;

        /// <summary>
        /// The default salth length in bytes for the hash.
        /// </summary>
        public const int DefaultSaltLength = 16;

        /// <summary>
        /// Gets or sets the PseudoRandomFunction to use for the hash generation.
        /// </summary>
        public KeyDerivationPrf PseudoRandomFunction { get; set; } = DefaultPseudoRandomFunction;

        /// <summary>
        /// Gets or sets the the number of iterations for the hash.
        /// </summary>
        public int Iterations { get; set; } = DefaultIterations;

        /// <summary>
        /// Gets or sets the salt length in bytes.
        /// </summary>
        public int SaltLength { get; set; } = DefaultSaltLength;
    }
}
