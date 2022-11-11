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
    using HealthGateway.Common.Models.Cacheable;

    /// <summary>
    /// A delegate to generate a hash.
    /// </summary>
    public interface IHashDelegate
    {
        /// <summary>
        /// Creates an implementation specific hash of a key.
        /// </summary>
        /// <param name="key">The string to hash, if null the hash is null.</param>
        /// <returns>The newly created Hash.</returns>
        IHash Hash(string? key);

        /// <summary>
        /// Validates that the supplied key will hash to instance.
        /// </summary>
        /// <param name="key">The key to hash and compare.</param>
        /// <param name="compareHash">The hash object to compare.</param>
        /// <returns>true if the key generates the same hash or the hash value is null and the key is null.</returns>
        bool Compare(string? key, IHash compareHash);
    }
}
