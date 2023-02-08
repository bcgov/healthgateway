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
namespace HealthGateway.CommonTests.Utils
{
    using HealthGateway.Common.Models;
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// AddressUtility's Unit Tests.
    /// </summary>
    public class AddressUtilityTests
    {
        /// <summary>
        /// Should get address as a single line given 1 street line.
        /// </summary>
        [Fact]
        public void ShouldGetAddressAsSingleLineGiven1StreetLine()
        {
            string expected = "1025 Sutlej Street, Victoria, BC, V8V2V8";

            // Arrange
            Address address = new()
            {
                StreetLines = { "1025 Sutlej Street" },
                City = "Victoria",
                State = "BC",
                PostalCode = "V8V2V8",
            };

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should get address as a single line given 2 street lines.
        /// </summary>
        [Fact]
        public void ShouldGetAddressAsSingleLineGiven2StreetLines()
        {
            string expected = "1025 Sutlej Street, Suite 310, Victoria, BC, V8V2V8";

            // Arrange
            Address address = new()
            {
                StreetLines = { "1025 Sutlej Street", "Suite 310" },
                City = "Victoria",
                State = "BC",
                PostalCode = "V8V2V8",
            };

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should get address as a single line given no state.
        /// </summary>
        [Fact]
        public void ShouldGetAddressAsSingleLineGivenNoState()
        {
            string expected = "1025 Sutlej Street, Suite 310, Victoria, V8V2V8";

            // Arrange
            Address address = new()
            {
                StreetLines = { "1025 Sutlej Street", "Suite 310" },
                City = "Victoria",
                PostalCode = "V8V2V8",
            };

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should get address as a single line given no postal code.
        /// </summary>
        [Fact]
        public void ShouldGetAddressAsSingleLineGivenNoPostalCode()
        {
            string expected = "1025 Sutlej Street, Suite 310, Victoria, BC";

            // Arrange
            Address address = new()
            {
                StreetLines = { "1025 Sutlej Street", "Suite 310" },
                City = "Victoria",
                State = "BC",
            };

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }


        /// <summary>
        /// Should not get address as a single line given null address.
        /// </summary>
        [Fact]
        public void ShouldNotGetAddressAsSingleLineGivenNullAddress()
        {
            string expected = string.Empty;

            // Arrange
            Address? address = null;

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should not get address as a single line given emoty address.
        /// </summary>
        [Fact]
        public void ShouldNotGetAddressAsSingleLineGivenEmptyAddress()
        {
            string expected = string.Empty;

            // Arrange
            Address? address = new();

            // Act
            string actual = AddressUtility.GetAddressAsSingleLine(address);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
