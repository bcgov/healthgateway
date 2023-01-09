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
    using System;
    using HealthGateway.Common.Data.Utils;
    using Xunit;

    /// <summary>
    /// Test class for the date formatter class.
    /// </summary>
    public class DateFormatterTests
    {
        private static readonly DateTime DefaultDateTime = new(2022, 12, 31, 17, 00, 00);

        /// <summary>
        /// Should short date given date only.
        /// </summary>
        [Fact]
        public void ShouldBeShortDateGivenDateOnly()
        {
            // Arrange
            DateOnly dateOnly = DateOnly.FromDateTime(DefaultDateTime);
            string expected = "2022-12-31";

            // Act
            string actual = DateFormatter.ToShortDate(dateOnly);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be short date given date time.
        /// </summary>
        [Fact]
        public void ShouldBeShortDateGivenDateTime()
        {
            // Arrange
            string expected = "2022-12-31";

            // Act
            string actual = DateFormatter.ToShortDate(DefaultDateTime);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be long date given date only.
        /// </summary>
        [Fact]
        public void ShouldBeLongDateGivenDateOnly()
        {
            // Arrange
            DateOnly dateOnly = DateOnly.FromDateTime(DefaultDateTime);
            string expected = "Saturday, December 31, 2022";

            // Act
            string actual = DateFormatter.ToLongDate(dateOnly);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be long date given date time.
        /// </summary>
        [Fact]
        public void ShouldBeLongDateGivenDateTime()
        {
            // Arrange
            string expected = "Saturday, December 31, 2022";

            // Act
            string actual = DateFormatter.ToLongDate(DefaultDateTime);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be short time given time only.
        /// </summary>
        [Fact]
        public void ShouldBeShortTimeGivenTimeOnly()
        {
            // Arrange
            TimeOnly timeOnly = new(17, 00, 00);
            string expected = "5:00 PM";

            // Act
            string actual = DateFormatter.ToShortTime(timeOnly);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be short time given date time.
        /// </summary>
        [Fact]
        public void ShouldBeShortTimeGivenDateTime()
        {
            // Arrange
            string expected = "5:00 PM";

            // Act
            string actual = DateFormatter.ToShortTime(DefaultDateTime);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be short date and time given date time.
        /// </summary>
        [Fact]
        public void ShouldBeShortDateAndTimeGivenDateTime()
        {
            // Arrange
            string expected = "2022-12-31 5:00 PM";

            // Act
            string actual = DateFormatter.ToShortDateAndTime(DefaultDateTime);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be long date and time given date time.
        /// </summary>
        [Fact]
        public void ShouldBeLongDateAndTimeGivenDateTime()
        {
            // Arrange
            string expected = "Saturday, December 31, 2022 5:00 PM";

            // Act
            string actual = DateFormatter.ToLongDateAndTime(DefaultDateTime);

            // Assert
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Should be long date and time given date time.
        /// </summary>
        [Fact]
        public void ShouldParseGivenDateTimeAndFormat()
        {
            // Arrange
            string format = "yyyy-MM-dd HH:mm:ss";
            string dateTime = "2022-12-31 17:00:00";
            DateTime expectedDateTime = DefaultDateTime;
            bool expected = true;

            // Act
            bool actual = DateFormatter.TryParse(dateTime, format, out DateTime actualDateTime);

            // Assert
            Assert.Equal(expectedDateTime, actualDateTime);
            Assert.Equal(expected, actual);
        }
    }
}
