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

namespace HealthGateway.Common.Services
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Implementation of the <see cref="NonceService"/> with cryptograpgic generation.
    /// </summary>
    public class NonceService : INonceService
    {
        private string currentNonce;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonceService"/> class.
        /// </summary>
        public NonceService()
        {
            // Generate the nonce
            RNGCryptoServiceProvider rngProvider = new RNGCryptoServiceProvider();
            byte[] nonceBytes = new byte[32];
            rngProvider.GetBytes(nonceBytes);
            this.currentNonce = Convert.ToBase64String(nonceBytes);
        }

        /// <inheritdoc />
        public string GetCurrentNonce()
        {
            return currentNonce;
        }
    }
}
