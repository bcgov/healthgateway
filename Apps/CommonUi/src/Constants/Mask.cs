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
namespace HealthGateway.Common.Ui.Constants
{
    using MudBlazor;

    /// <summary>
    /// A class with fields representing the various format masks.
    /// </summary>
    public static class Mask
    {
        /// <summary>
        /// Format mask for Personal Health Number (PHN).
        /// </summary>
        public static readonly IMask PhnMask = new PatternMask("0000 000 000");

        /// <summary>
        /// Format mask for North American phone numbers.
        /// </summary>
        public static readonly IMask PhoneMask = new PatternMask("(000) 000-0000");

        /// <summary>
        /// Format mask for Canadian postal codes.
        /// </summary>
        public static readonly IMask PostalCodeMask = new PatternMask("a0a 0a0")
        {
            Transformation = char.ToUpperInvariant,
        };

        /// <summary>
        /// Format mask for American zip codes (00000 or 00000-0000).
        /// </summary>
        // Using a RegexMask instead of a PatternMask ensures the hyphen won't appear after 5 digits are entered.
        public static readonly IMask ZipCodeMask = new RegexMask(@"^\d{0,4}$|^\d{5}(-\d{0,4})?$");
    }
}
