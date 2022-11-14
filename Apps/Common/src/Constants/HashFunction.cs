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
namespace HealthGateway.Common.Constants
{
    /// <summary>
    /// The enumeration of Hash functions.
    /// </summary>
    public enum HashFunction
    {
        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-1 hash function (FIPS 180-4).
        /// </summary>
        HmacSha1 = 0,

        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-256 hash function (FIPS 180-4).
        /// </summary>
        HmacSha256 = 1,

        /// <summary>
        /// The HMAC algorithm (RFC 2104) using the SHA-512 hash function (FIPS 180-4).
        /// </summary>
        HmacSha512 = 2,
    }
}
