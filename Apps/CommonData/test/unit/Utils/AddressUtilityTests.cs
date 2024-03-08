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
namespace HealthGateway.Common.Data.Tests.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using HealthGateway.Common.Data.Constants;
    using HealthGateway.Common.Data.Models;
    using HealthGateway.Common.Data.Utils;
    using Xunit;

    /// <summary>
    /// Unit Tests for AddressUtility.
    /// </summary>
    public class AddressUtilityTests
    {
        private const string StreetAddress = "1025 Sutlej Street";
        private const string Apartment = "Suite 310";
        private const string City = "Victoria";
        private const string State = "BC";
        private const string PostalCode = "V8V2V8";
        private const string Country = "Canada";

        /// <summary>
        /// Gets parameters for broadcast unit test(s).
        /// </summary>
        public static TheoryData<Address?, bool, string> SingleLineAddressTheoryData =>
            new()
            {
                {
                    new()
                    {
                        StreetLines = [StreetAddress],
                        City = City,
                        State = State,
                        PostalCode = PostalCode,
                    },
                    false,
                    "1025 Sutlej Street, Victoria, BC, V8V2V8"
                },
                {
                    new()
                    {
                        StreetLines = [StreetAddress],
                        City = City,
                        State = State,
                        PostalCode = PostalCode,
                        Country = Country,
                    },
                    false,
                    "1025 Sutlej Street, Victoria, BC, V8V2V8"
                },
                {
                    new()
                    {
                        StreetLines = [StreetAddress],
                        City = City,
                        State = State,
                        PostalCode = PostalCode,
                        Country = Country,
                    },
                    true,
                    "1025 Sutlej Street, Victoria, BC, V8V2V8, Canada"
                },
                {
                    new()
                    {
                        StreetLines = [StreetAddress, Apartment],
                        City = City,
                        State = State,
                        PostalCode = PostalCode,
                    },
                    false,
                    "1025 Sutlej Street, Suite 310, Victoria, BC, V8V2V8"
                },
                {
                    new()
                    {
                        StreetLines = [StreetAddress, Apartment],
                        City = City,
                        PostalCode = PostalCode,
                    },
                    false,
                    "1025 Sutlej Street, Suite 310, Victoria, V8V2V8"
                },
                {
                    new()
                    {
                        StreetLines = [StreetAddress, Apartment],
                        City = City,
                        State = State,
                    },
                    false,
                    "1025 Sutlej Street, Suite 310, Victoria, BC"
                },
                {
                    new(),
                    false,
                    string.Empty
                },
                {
                    null,
                    false,
                    string.Empty
                },
            };

        /// <summary>
        /// Tests for GetAddressAsSingleLine.
        /// </summary>
        /// <param name="address">The address to format.</param>
        /// <param name="includeCountry">Whether the country should be included.</param>
        /// <param name="expected">Expected result.</param>
        [Theory]
        [MemberData(nameof(SingleLineAddressTheoryData))]
        public void ShouldGetAddressAsSingleLine(Address? address, bool includeCountry, string expected)
        {
            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address, includeCountry);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Tests for GetCountryCode.
        /// </summary>
        /// <param name="countryName">The name of the country in question, or one of its aliases.</param>
        /// <param name="expected">Expected result.</param>
        [Theory]
        [InlineData("Canada", CountryCode.CA)]
        [InlineData("Cranada", CountryCode.Unknown)]
        [InlineData("Sicily (included in Italy)", CountryCode.IT)]
        public void ShouldGetCountryCode(string countryName, CountryCode expected)
        {
            // Act
            CountryCode actual = AddressUtility.GetCountryCode(countryName);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// CountriesWithAliases.
        /// </summary>
        [Fact]
        public void ShouldGetCountriesWithAliases()
        {
            // Act
            List<string> actual = AddressUtility.CountriesWithAliases.ToList();

            // Assert
            Assert.NotEmpty(actual);
            Assert.Equal("Canada", actual.First());
            Assert.Contains("Sicily (included in Italy)", actual);
        }

        /// <summary>
        /// Countries.
        /// </summary>
        [Fact]
        public void ShouldGetCountries()
        {
            // Act
            List<string> actual = AddressUtility.Countries.ToList();

            // Assert
            Assert.NotEmpty(actual);
            Assert.Equal("Canada", actual.First());
            Assert.DoesNotContain("Sicily (included in Italy)", actual);
        }

        /// <summary>
        /// Provinces.
        /// </summary>
        [Fact]
        public void ShouldGetProvinces()
        {
            // Act
            List<string> actual = AddressUtility.Provinces.ToList();

            // Assert
            Assert.NotEmpty(actual);
            Assert.Equal(13, actual.Count);
            Assert.Equal("Alberta", actual.First());
        }

        /// <summary>
        /// States.
        /// </summary>
        [Fact]
        public void ShouldGetStates()
        {
            // Act
            List<string> actual = AddressUtility.States.ToList();

            // Assert
            Assert.NotEmpty(actual);
            Assert.Equal(51, actual.Count);
            Assert.Equal("Alabama", actual.First());
        }
    }
}
