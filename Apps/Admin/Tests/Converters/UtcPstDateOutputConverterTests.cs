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
namespace HealthGateway.Admin.Client.Converters
{
    using System;
    using HealthGateway.Admin.Server.Converters;
    using Xunit;

    /// <summary>
    /// Test class for the utc pst date output converter class.
    /// </summary>
    public class UtcPstDateOutputConverterTests
    {
        /// <summary>
        /// Should convert to date time given value in string.
        /// </summary>
        [Fact]
        public void ShouldConvertFromString()
        {
            // Arrange
            UtcPstDateOutputConverter converter = new();
            string dateTime = "12/31/2022 17:00:00";
            DateTime expected = new(2022, 12, 31, 09, 00, 00);

            // Act
            object? actual = converter.ConvertFromString(dateTime, null, null);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should return null given null value in string.
        /// </summary>
        [Fact]
        public void ShouldConvertFromStringReturnsNullGivenNullValue()
        {
            // Arrange
            UtcPstDateOutputConverter converter = new();
            string? dateTime = null;
            DateTime? expected = null;

            // Act
            object? actual = converter.ConvertFromString(dateTime, null, null);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should convert to string given value in date time.
        /// </summary>
        [Fact]
        public void ShouldConvertToString()
        {
            // Arrange
            UtcPstDateOutputConverter converter = new();
            DateTime dateTime = new(2022, 12, 31, 17, 00, 00);
            string expected = "2022-12-31 9:00 AM";

            // Act
            object? actual = converter.ConvertToString(dateTime, null, null);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should return empty string given null value in date time.
        /// </summary>
        [Fact]
        public void ShouldConvertToStringReturnsEmptyStringGivenNullValue()
        {
            // Arrange
            UtcPstDateOutputConverter converter = new();
            DateTime? dateTime = null;
            string expected = string.Empty;

            // Act
            object? actual = converter.ConvertToString(dateTime, null, null);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
