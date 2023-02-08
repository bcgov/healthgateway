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
namespace HealthGateway.Common.Utils
{
    using System.Linq;
    using System.Text;
    using HealthGateway.Common.Models;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Utilities for extracting and constructing values to and from a client registry address.
    /// </summary>
    public static class AddressUtility
    {
        /// <summary>
        /// Constructs a single string line for the provided address.
        /// </summary>
        /// <param name="address">The address to convert.</param>
        /// <returns>Address as a single string line or empty string.</returns>
        public static string GetAddressAsSingleLine(Address? address)
        {
            if (address != null)
            {
                string streetLines = string.Join(", ", address.StreetLines.Where(s => !string.IsNullOrEmpty(s)));
                string city = address.City;
                string postalCode = address.PostalCode;

                StringBuilder sb = new();
                BuildString(sb, streetLines);
                BuildString(sb, city);
                BuildString(sb, postalCode);
                return sb.ToString();
            }

            return string.Empty;
        }

        private static void BuildString(StringBuilder sb, string value)
        {
            if (value is { Length: > 0 })
            {
                if (!sb.ToString().IsNullOrEmpty())
                {
                    sb.Append(", ");
                }

                sb.Append(value);
            }
        }
    }
}
