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

namespace HealthGateway.Common.Utils
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Utilities for Hash.
    /// </summary>
    public static class HashGenerator
    {
        /// <summary>
        /// Hash Byte Array of a string.
        /// </summary>
        /// <param name="sSource">The string to hashed.</param>
        /// <returns>A byte array of the computed hash.</returns>
        public static byte[] ComputeHash(string sSource)
        {
            var tmpSource = Encoding.Default.GetBytes(sSource);
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
#pragma warning disable SCS0006 // Weak hashing function
            using var md5CryptoService = MD5.Create();
#pragma warning restore SCS0006 // Weak hashing function
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            return md5CryptoService.ComputeHash(tmpSource);
        }

        /// <summary>
        /// Hashed Guid of a string.
        /// </summary>
        /// <param name="sSource">The string to hashed.</param>
        /// <returns>A byte array of the computed hash.</returns>
        public static Guid ComputeHashToGuid(string sSource)
        {
            byte[] byteArrHash = ComputeHash(sSource);
            return new Guid(byteArrHash);
        }
    }
}
