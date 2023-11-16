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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;

    /// <summary>
    /// Utilities for interacting with addresses.
    /// </summary>
    public static partial class AddressUtility
    {
        /// <summary>
        /// Canadian province and territory abbreviations ordered alphabetically by province name.
        /// </summary>
        public static readonly IEnumerable<ProvinceAbbreviation> ProvinceAbbreviations = Enum.GetValues<ProvinceAbbreviation>()[1..];

        /// <summary>
        /// American state abbreviations ordered alphabetically by state name.
        /// </summary>
        public static readonly IEnumerable<StateAbbreviation> StateAbbreviations = Enum.GetValues<StateAbbreviation>()[1..];

        /// <summary>
        /// Country codes with Canada first, followed by USA, then the remaining countries ordered alphabetically by country name.
        /// </summary>
        public static readonly IEnumerable<CountryCode> CountryCodes = Enum.GetValues<CountryCode>()[1..];

        [SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "Country names don't require spellchecking")]
        private static readonly Dictionary<string, CountryCode> CountryAliases = new Dictionary<string, CountryCode>
        {
            ["Admiralty Islands (included in Papua New Guinea)"] = CountryCode.PG,
            ["Aegean Islands (included in Greece)"] = CountryCode.GR,
            ["Alofi Islands (included in Wallis and Futana)"] = CountryCode.WF,
            ["Andaman Islands (included in India)"] = CountryCode.IN,
            ["Ascension (included in Saint Helena)"] = CountryCode.SH,
            ["Aunu'u and Manua Islands (included in American Samoa)"] = CountryCode.AS,
            ["Azores (included in Portugal)"] = CountryCode.PT,
            ["Baker Island (included in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Balearic Islands (included in Spain)"] = CountryCode.ES,
            ["Bikini Island (included in Marshall Islands)"] = CountryCode.MH,
            ["Billiton Island (included in Indonesia)"] = CountryCode.ID,
            ["Bonaire (included in Dutch Caribbean (formerly Netherlands Antilles))"] = CountryCode.BQ,
            ["British Antarctic Territory (included in Falkland Islands)"] = CountryCode.FK,
            ["British Indian Ocean Territory (included in Seychelles)"] = CountryCode.SC,
            ["Burma (included in Myanmar)"] = CountryCode.MM,
            ["Campbell Island (included in New Zealand)"] = CountryCode.NZ,
            ["Canary Islands (included in Spain)"] = CountryCode.ES,
            ["Caroline Islands (included in Micronesia, Federal States of)"] = CountryCode.FM,
            ["Channel Islands (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Chatham Island (included in New Zealand)"] = CountryCode.NZ,
            ["Corfu (included in Greece)"] = CountryCode.GR,
            ["Corsica (included in France)"] = CountryCode.FR,
            ["Crete (included in Greece)"] = CountryCode.GR,
            ["Cyrenaica (included in Libya)"] = CountryCode.LY,
            ["Desroches (included in Seychelles)"] = CountryCode.SC,
            ["Dodecanese Island (included in Greece)"] = CountryCode.GR,
            ["Ducie Island (included in Pitcairn)"] = CountryCode.PN,
            ["East Timor (included in Timor-Leste)"] = CountryCode.TL,
            ["Easter Island (included in Chile)"] = CountryCode.CL,
            ["Ellice Island (included in Tuvalu)"] = CountryCode.TV,
            ["England (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Enwetok and Kwajelein Islands (included in Marshall Islands)"] = CountryCode.MH,
            ["Fanning Island (included in Kiribati)"] = CountryCode.KI,
            ["Farquhar Island (included in Seychelles)"] = CountryCode.SC,
            ["Friendly Islands (included in Tonga)"] = CountryCode.TO,
            ["Futuna Island (included in Wallis and Futuna)"] = CountryCode.WF,
            ["Gambier Islands (included in French Polynesia)"] = CountryCode.PF,
            ["Gilbert Islands (included in Kiribati)"] = CountryCode.KI,
            ["Great Britain (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Hawaii (included in United States of America)"] = CountryCode.US,
            ["Henderson Island (included in Pitcairn)"] = CountryCode.PN,
            ["Hervey Islands (included in New Zealand)"] = CountryCode.NZ,
            ["Holland (included in Netherlands)"] = CountryCode.NL,
            ["Howland Island (included in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Huon Islands (included in New Caledonia)"] = CountryCode.NC,
            ["Ifni Territory (included in Morocco)"] = CountryCode.MA,
            ["Inner Mongolia (included in China)"] = CountryCode.CN,
            ["Isle of Pines (included in New Caledonia)"] = CountryCode.NC,
            ["Ivory Coast (included in Côte d'Ivoire)"] = CountryCode.CI,
            ["Jarvis Island (included in U.S. Minor Outlying Island)"] = CountryCode.UM,
            ["Johnston Island (included in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Kamaran Islands (included in Yemen)"] = CountryCode.YE,
            ["Kermadec Islands (included in New Zealand)"] = CountryCode.NZ,
            ["Kosova (included in Kosovo)"] = CountryCode.XZ,
            ["Leeward Islands (included in British Virgin Islands)"] = CountryCode.VG,
            ["Line Island (included in Kiribati)"] = CountryCode.KI,
            ["Lord Howe Island (included in Australia)"] = CountryCode.AU,
            ["Loyalty Islands (included in New Caledonia)"] = CountryCode.NC,
            ["Madeira (included in Portugal)"] = CountryCode.PT,
            ["Malvinas (included in Falkland Islands)"] = CountryCode.FK,
            ["Manchuria (included in China)"] = CountryCode.CN,
            ["Manus Island (included in Papua New Guinea)"] = CountryCode.PG,
            ["Mariana Islands (included in Northern Mariana Islands)"] = CountryCode.MP,
            ["Marquesas Islands (included in French Polynesia)"] = CountryCode.PF,
            ["Midway Island (included in U.S. Minor Outlying Island)"] = CountryCode.UM,
            ["Moluccas Islands (included in Indonesia)"] = CountryCode.ID,
            ["Navassa Island (included in United States of America)"] = CountryCode.US,
            ["Netherlands Antilles (now known as Dutch Caribbean)"] = CountryCode.BQ,
            ["Nevis (included in Saint Kitts and Nevis)"] = CountryCode.KN,
            ["New Britain (included in Papua New Guinea)"] = CountryCode.PG,
            ["New Ireland (included in Papua New Guinea)"] = CountryCode.PG,
            ["Nicobar Island (included in India)"] = CountryCode.IN,
            ["Northern Ireland (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Ocean Island (included in Kiribati)"] = CountryCode.KI,
            ["Oeno Island (included in Pitcairn)"] = CountryCode.PN,
            ["Phoenix Island (included in Kiribati)"] = CountryCode.KI,
            ["Portuguese Timor (included in Timor-Leste)"] = CountryCode.TL,
            ["Principe (included in Sao Tome and Principe)"] = CountryCode.ST,
            ["Rarotonga (included in Cook Islands)"] = CountryCode.CK,
            ["Rodriguez Island (included in Mauritius)"] = CountryCode.MU,
            ["Ross Dependency (included in New Zealand)"] = CountryCode.NZ,
            ["Saba (included in Dutch Caribbean (formerly Netherlands Antilles))"] = CountryCode.BQ,
            ["Sand Island (included in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Santas Cruz Island (included in Solomon Islands)"] = CountryCode.SB,
            ["Savage Island (included in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Scotland (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Shortland Island (included in Solomon Islands)"] = CountryCode.SB,
            ["Sicily (included in Italy)"] = CountryCode.IT,
            ["Sint Eustatius (included Dutch Caribbean (formerly Netherlands Antilles))"] = CountryCode.BQ,
            ["Society Islands (included in French Polynesia)"] = CountryCode.PF,
            ["South Georgia & South Sandwich Islands (included in Falkland Islands)"] = CountryCode.FK,
            ["South Orkney Islands (included in Falkland Islands)"] = CountryCode.FK,
            ["South Shetland Islands (included in Falkland Islands)"] = CountryCode.FK,
            ["Spanish Territories of North Africa (included in Spain)"] = CountryCode.ES,
            ["St Croix (included in U.S. Virgin Islands)"] = CountryCode.VI,
            ["St John Island (included in U.S. Virgin Islands)"] = CountryCode.VI,
            ["St Thomas Island (included in U.S. Virgin Islands)"] = CountryCode.VI,
            ["Suwarrow Island (included in New Zealand)"] = CountryCode.NZ,
            ["Swains Island (included in American Samoa)"] = CountryCode.AS,
            ["Tahiti (included in French Polynesia)"] = CountryCode.PF,
            ["Tarawa Island (included in Kiribati)"] = CountryCode.KI,
            ["Tasmania (included in Australia)"] = CountryCode.AU,
            ["Tibet (included in China)"] = CountryCode.CN,
            ["Torres Island (included in Vanuatu)"] = CountryCode.VU,
            ["Tortola Island (included in British Virgin Islands)"] = CountryCode.VG,
            ["Touamotu Islands (included in French Polynesia)"] = CountryCode.PF,
            ["Tripolitania (included in Libya)"] = CountryCode.LY,
            ["Truk Island (included in Micronesia, Federal States Of)"] = CountryCode.FM,
            ["Trust Territory of Pacific (included in United States of America)"] = CountryCode.US,
            ["Tubuai Islands (included in French Polynesia)"] = CountryCode.PF,
            ["Tutuila Island (included in American Samoa)"] = CountryCode.AS,
            ["Union Group (included in Tokelau)"] = CountryCode.TK,
            ["Vatican (included in Holy See (Vatican))"] = CountryCode.VA,
            ["Vojvodina (included in Serbia)"] = CountryCode.RS,
            ["Wake Island (including in U.S. Minor Outlying Islands)"] = CountryCode.UM,
            ["Wales (included in United Kingdom (Great Britain))"] = CountryCode.GB,
            ["Washington Island (included in Kiribati)"] = CountryCode.KI,
            ["Yap Island (included in Micronesia, Federal States Of)"] = CountryCode.FM,
            ["Yugoslavia (included in Serbia)"] = CountryCode.RS,
            ["Zaire (included in Congo, Democratic Republic Of)"] = CountryCode.CD,
        };

        /// <summary>
        /// Gets country names, with Canada first, followed by USA, then the remaining countries ordered alphabetically.
        /// </summary>
        public static IEnumerable<string> Countries => CountryCodes.Select(GetCountryName);

        /// <summary>
        /// Gets country names, with Canada first, followed by USA, then the remaining countries and aliases ordered
        /// alphabetically.
        /// </summary>
        public static IEnumerable<string> CountriesWithAliases =>
            CountryCodes.Take(2)
                .Select(GetCountryName)
                .Concat(
                    CountryCodes.Skip(2)
                        .Select(GetCountryName)
                        .Concat(CountryAliases.Select(c => c.Key))
                        .Order());

        /// <summary>
        /// Gets the names of Canadian provinces in alphabetical order.
        /// </summary>
        public static IEnumerable<string> Provinces => ProvinceAbbreviations.Select(p => EnumUtility.ToEnumString(p, true));

        /// <summary>
        /// Gets the names of American states in alphabetical order.
        /// </summary>
        public static IEnumerable<string> States => StateAbbreviations.Select(s => EnumUtility.ToEnumString(s, true));

        /// <summary>
        /// Returns the name of a country given its country code.
        /// </summary>
        /// <param name="code">The country code in question.</param>
        /// <returns>A string containing the name of the country.</returns>
        public static string GetCountryName(CountryCode code)
        {
            return EnumUtility.ToEnumString(code, true);
        }

        /// <summary>
        /// Returns the country code corresponding to a country (or country alias).
        /// </summary>
        /// <param name="name">The name of the country in question, or one of its aliases.</param>
        /// <returns>
        /// The <see cref="CountryCode"/> corresponding to the country, or <see cref="CountryCode.Unknown"/> if there's no
        /// matching value.
        /// </returns>
        public static CountryCode GetCountryCode(string name)
        {
            try
            {
                return CountryAliases.TryGetValue(name, out CountryCode code) ? code : EnumUtility.ToEnum<CountryCode>(name, true);
            }
            catch (ArgumentException)
            {
                return CountryCode.Unknown;
            }
        }

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

        /// <summary>
        /// Regular expression for Canadian postal codes.
        /// </summary>
        [GeneratedRegex(@"^[A-Z]\d[A-Z] \d[A-Z]\d\z")]
        public static partial Regex PostalCodeRegex();

        /// <summary>
        /// Regular expression for American ZIP codes and ZIP+4 codes.
        /// </summary>
        [GeneratedRegex(@"^\d{5}(-\d{4})?\z")]
        public static partial Regex ZipCodeRegex();

        /// <summary>
        /// Regular expression for North American phone numbers.
        /// </summary>
        [GeneratedRegex(@"^\([2-9]\d{2}\) [2-9]\d{2}-\d{4}\z")]
        public static partial Regex PhoneNumberRegex();
    }
}
