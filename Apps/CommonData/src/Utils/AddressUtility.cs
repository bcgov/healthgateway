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
namespace HealthGateway.Common.Data.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Utilities for extracting and constructing values to and from a client registry address.
    /// </summary>
    public static class AddressUtility
    {
        /// <summary>
        /// Concatenates elements of the address into a single string, separated by commas and spaces.
        /// </summary>
        /// <param name="address">The address to format.</param>
        /// <param name="includeCountry">Whether the country should be included.</param>
        /// <returns>A string that includes all non-blank elements of the address, separated by commas and spaces.</returns>
        public static string GetAddressAsSingleLine(Address? address, bool includeCountry = false)
        {
            if (address == null)
            {
                return string.Empty;
            }

            IEnumerable<string> addressElements = address.StreetLines
                .Concat(new[] { address.City, address.State, address.PostalCode });

            if (includeCountry)
            {
                addressElements = addressElements.Append(address.Country);
            }

            return StringManipulator.JoinWithoutBlanks(addressElements, ", ");
        }
    }
}
