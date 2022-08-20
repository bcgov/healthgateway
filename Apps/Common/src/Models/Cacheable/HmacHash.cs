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
namespace HealthGateway.Common.Models.Cacheable
{
    using HealthGateway.Common.Constants;

    /// <summary>
    /// A hash of something.
    /// </summary>
    public class HmacHash : IHash
    {
        /// <summary>
        /// Gets or sets the pseudo random function that was used to generate this hash.
        /// </summary>
        public HashFunction PseudoRandomFunction { get; set; } = HashFunction.HMACSHA512;

        /// <summary>
        /// Gets or sets the iterations used to generate this hash.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Gets or sets the base64 salt that was used in generating the hash.
        /// </summary>
        public string? Salt { get; set; }
    }
}
