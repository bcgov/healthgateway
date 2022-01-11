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
    using System;
    using System.Globalization;
    using HealthGateway.Common.Utils;
    using Xunit;

    /// <summary>
    /// DateTimeFormatter's Unit Tests.
    /// </summary>
    public class DateTimeFormatterTests
    {
        /// <summary>
        /// FormatDate - Happy Path.
        /// </summary>
        [Fact]
        public void ShouldFormat()
        {
            DateTime dt = DateTime.ParseExact("20200101", "yyyyMMdd", CultureInfo.InvariantCulture);
            string expectedDateStr = "2020-01-01";

            string actualDateStr = DateTimeFormatter.FormatDate(dt);

            Assert.True(actualDateStr == expectedDateStr);
        }

        /// <summary>
        /// FormatDate - Null.
        /// </summary>
        [Fact]
        public void ShouldNullReturnEmpty()
        {
            string result = DateTimeFormatter.FormatDate(null);

            Assert.True(string.IsNullOrEmpty(result));
        }
    }
}
